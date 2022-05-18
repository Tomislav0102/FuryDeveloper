using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TK.Enums;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DrugiZadatak
{
    /// <summary>
    /// Glavna skripta koja odreduje svu logiku
    /// </summary>
    public class MazeManager : MonoBehaviour
    {
        [SerializeField] bool canDiagonal = true; //kretajne po dijagonali. ako je 'false' kretanje je samo ortogonalno
        [SerializeField] Toggle toggleDiagonal; //UI element za ukljucivanje/iskljucivanje gornjeg bool-a
        Vector2Int mainPoz; //pozicija misa
        [SerializeField] Camera cam; //kamera, potrebno zbog odredivanja pozcije misa
        [SerializeField] TextMeshProUGUI displayMainInfo; 
        [SerializeField] TileData tdPrefab; //prefab polja koji ce se spawnati. Osnovni element labirinta
        [SerializeField] Transform par_tileData; //parent transform za polja. jedina svrha je preglednost u editor-u
        [SerializeField] int dim; //dimenzije labirinta

        TileData[,] tileDatas; //raspored polja u labirintu prikazanat kroz 2D array
        List<TileData> labirintExits = new List<TileData>(); //polja koja su izlaz iz labirinta
        bool hasEnd; //potvrda da je barem jedan izlaz generiran
        const int disOrtogonal = 10; //"cijena" kretanja ortogonalno
        const int disDijagonal = 14; //"cijena" kretnja dijagonalno
        List<TileData> openTD; //polja za koja treba utvrditi najbliže izlazu (u kontekstu A* algoritma)
        List<TileData> closedTD; //vec pretrazena polja

        private void Awake()
        {
            Initialization();
        }
        /// <summary>
        /// Namjesti kameru ovisno o dimenzijama, stvori i definira polja ovisno o nasumicnim parametrima.
        /// </summary>
        void Initialization()
        {
            cam.transform.position = new Vector3(dim * 0.5f, dim * 0.5f, -10f);
            cam.orthographicSize = 0.67f * dim;
            tileDatas = new TileData[dim, dim];
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    if (Random.value > 0.4f)
                    {
                        tileDatas[i, j] = Instantiate(tdPrefab, new Vector2(i, j), Quaternion.identity, par_tileData);
                        tileDatas[i, j].koor = new Vector2Int(i, j);
                        tileDatas[i, j].name = "Polje (" + i + "," + j + ")";
                        if ((i == 0 || j == 0 || i == dim - 1 || j == dim - 1) && Random.value > 0.9f)
                        {
                            tileDatas[i, j].Tiletype = TileType.Exit;
                            labirintExits.Add(tileDatas[i, j]);
                            hasEnd = true;

                        }
                    }

                }
            }
        }

        private void Start()
        {
            ShowInfo();
            toggleDiagonal.isOn = canDiagonal;
        }
        void ShowInfo() => displayMainInfo.text = "Klikni bilo gdje na labirintu. Put do najblizeg izlaz ce biti oznacen zelenom bojom.";
        public void G_Restart() => SceneManager.LoadScene(gameObject.scene.name);
        public void G_ToggleDopDijagonala() => canDiagonal = toggleDiagonal.isOn;
        /// <summary>
        /// Prva linija je samo za "build"
        /// Druga linija je oznacavanje pocetnog polja za trazenej izlaza
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            if (Input.GetMouseButtonDown(0)) MouseControls();
        }

        /// <summary>
        /// Nalazenje pocetne tocke.
        /// Ako je klik validan pokrece glavnu metodu.
        /// </summary>
        private void MouseControls()
        {
            Vector2 poz = cam.ScreenToWorldPoint(Input.mousePosition);
            mainPoz = new Vector2Int(Mathf.FloorToInt(poz.x), Mathf.FloorToInt(poz.y));
            if (mainPoz.x < 0 || mainPoz.y < 0 || mainPoz.x > dim - 1 || mainPoz.y > dim - 1 || tileDatas[mainPoz.x, mainPoz.y] == null)
            {
                displayMainInfo.text = "To nije prohodni dio labirinta. Klikni na neku drugu poziciju.";
                Invoke(nameof(ShowInfo), 3f);
                return;
            }
            DefinePaths(tileDatas[mainPoz.x, mainPoz.y]);
        }

        /// <summary>
        /// Glavna metoda.
        /// Prvo odreduje kraj ako takav ne postoji. 
        /// Resetira boju tile-ova.
        /// Za sve krajeve pronalazi puteve (lista) i odreduje najkraci te ga iscrtava.
        /// Ako nema izlaza to javlja korisniku.
        /// </summary>
        /// <param name="_startTD">ovdje ulazi polje na koje je korisnik kliknuo</param>
        public void DefinePaths(TileData _startTD)
        {
            if (!hasEnd)
            {
                labirintExits.Add(tileDatas[0, 0]);
                tileDatas[0, 0].Tiletype = TileType.Exit;
                hasEnd = true;
            }
            for (int i = 0; i < dim; i++)
            {
                for (int j = 0; j < dim; j++)
                {
                    if (tileDatas[i, j] != null && tileDatas[i, j].Tiletype == TileType.Walkable) tileDatas[i, j].spriteTile.color = Color.white;
                }
            }
            List<TileData> finalPath = new List<TileData>();
            List<int> disToExits = new List<int>();
            for (int i = 0; i < labirintExits.Count; i++)
            {
                finalPath = Path(_startTD, labirintExits[i]);
                if (finalPath != null) disToExits.Add(finalPath.Count);
                else disToExits.Add(int.MaxValue);
            }
            int shortestDis = disToExits.Min();
            if (shortestDis == int.MaxValue)
            {
                NoExit();
                return;
            }
            for (int i = 0; i < disToExits.Count; i++)
            {
                if (disToExits[i] == shortestDis)
                {
                    finalPath = Path(_startTD, labirintExits[i]);
                    foreach (TileData item in finalPath)
                    {
                        if (item.Tiletype == TileType.Walkable) item.spriteTile.color = Color.green;
                    }
                    return;
                }
            }
        }
        void NoExit()
        {
            displayMainInfo.text = "Nema izlaza sa te pozicije! Klikni na neku drugu poziciju.";
            Invoke(nameof(ShowInfo), 5f);
        }
        /// <summary>
        /// Odredivanje pojedinacnog puta po svakom izlazu
        /// </summary>
        /// <param name="_startTD">polje na koje je korisnik kliknuo</param>
        /// <param name="_endTD">izlaz</param>
        /// <returns></returns>
        List<TileData> Path(TileData _startTD, TileData _endTD)
        {
            openTD = new List<TileData>();
            openTD.Add(_startTD);
            closedTD = new List<TileData>();

            for (int i = 0; i < dim; i++) //resetiranje polja
            {
                for (int j = 0; j < dim; j++)
                {
                    if (tileDatas[i, j] != null)
                    {
                        tileDatas[i, j].Scost = int.MaxValue;
                        tileDatas[i, j].previousTile = null;

                    }
                }
            }

            _startTD.Scost = 0;
            _startTD.Ecost = Distances(_startTD.koor, _endTD.koor);

            while (openTD.Count > 0)
            {
                TileData currentTD = P_smallestTOT(openTD);
                if (currentTD == _endTD)
                {
                    //zavrsetak
                    return UkupniPut(_endTD);
                }
                openTD.Remove(currentTD);
                closedTD.Add(currentTD);
                foreach (TileData item in Neighbours(currentTD.koor))
                {
                    if (closedTD.Contains(item)) continue;

                    int costNeighbour = currentTD.Scost + Distances(currentTD.koor, item.koor);
                    if (costNeighbour < item.Scost)
                    {
                        item.previousTile = currentTD;
                        item.Scost = costNeighbour;
                        item.Ecost = Distances(item.koor, _endTD.koor);

                        if (!openTD.Contains(item)) openTD.Add(item);
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Odreduje validne susjede za definirano polje.
        /// Metoda ovisi o nacinu kretanja (dijagonalno ili samo ortogonalano)
        /// </summary>
        /// <param name="_koor">polje za koje odredujemo susjede</param>
        /// <returns></returns>
        List<TileData> Neighbours(Vector2Int _koor)
        {
            List<TileData> _list = new List<TileData>();

            if (canDiagonal)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (ValidNeighbour(i, j)) _list.Add(tileDatas[_koor.x + i, _koor.y + j]);
                    }
                }
            }
            else
            {
                if (ValidNeighbour(1, 0)) _list.Add(tileDatas[_koor.x + 1, _koor.y]);
                if (ValidNeighbour(- 1, 0)) _list.Add(tileDatas[_koor.x - 1, _koor.y]);
                if (ValidNeighbour(0, 1)) _list.Add(tileDatas[_koor.x, _koor.y + 1]);
                if (ValidNeighbour(0, - 1)) _list.Add(tileDatas[_koor.x, _koor.y - 1]);
            }

            bool ValidNeighbour(int x, int y) //lokalna metoda
            {
                if (x + _koor.x < 0 || y + _koor.y < 0 || x + _koor.x > dim - 1 || y + _koor.y > dim - 1) return false;
                if (tileDatas[x + _koor.x, y + _koor.y] == null) return false;
                if (x == 0 && y == 0) return false;
                return true;
            }

            return _list;
        }
        /// <summary>
        /// Ako izlaz postoji ovo ce biti lista polja koja ga odreduje
        /// </summary>
        /// <param name="_endTD"></param>
        /// <returns></returns>
        List<TileData> UkupniPut(TileData _endTD)
        {
            List<TileData> _list = new List<TileData>();
            _list.Add(_endTD);
            TileData currentTD = _endTD;
            while (currentTD.previousTile != null)
            {
                _list.Add(currentTD.previousTile);
                currentTD = currentTD.previousTile;
            }
            _list.Reverse();
            return _list;
        }
        /// <summary>
        /// Definira polje najblize izlazu iz ponudjenih koristeci A* algoritam logiiku
        /// </summary>
        /// <param name="_listTD">lista svih polja koja pretrazujemo</param>
        /// <returns></returns>
        TileData P_smallestTOT(List<TileData> _listTD)
        {
            TileData smallest = _listTD[0];
            for (int i = 0; i < _listTD.Count; i++)
            {
                if (_listTD[i].Totcost() < smallest.Totcost()) smallest = _listTD[i];
            }

            return smallest;
        }
        /// <summary>
        /// Udaljenost izmedu polja
        /// </summary>
        /// <param name="_start">prvo polje</param>
        /// <param name="_end">drugo polje</param>
        /// <returns></returns>
        int Distances(Vector2Int _start, Vector2Int _end)
        {
            int hor = Mathf.Abs(_end.x - _start.x);
            int ver = Mathf.Abs(_end.y - _start.y);
            int ostatak = Mathf.Abs(hor - ver);
            return disDijagonal * Mathf.Min(hor, ver) + disOrtogonal * ostatak;
        }

    }
}
