using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TK.Enums;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DrugiZadatak
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager gm;
        [SerializeField] GridLayoutGroup gridLayout;
        public int dimenzije;
        TileData[,] polja;
        [SerializeField] TextMeshProUGUI prikazGlavni;
        const int disRavno = 10;
        const int disDijagonal = 14;

        List<TileData> otvorena;
        List<TileData> zatvorena;
        List<TileData> krajevi = new List<TileData>();
        bool postojiKraj;
        private void Awake()
        {
            gm = this;
            dimenzije = gridLayout.constraintCount;
            polja = new TileData[dimenzije, dimenzije];
        }
        private void Start()
        {
            MetodaPrikaziUpute();
        }
        void MetodaPrikaziUpute()
        {
            prikazGlavni.text = "Klikni bilo gdje na mrezi. Put do najblizeg izlaz ce biti oznacen zelenom bojom.";
        }
        public void G_Restart()
        {
            SceneManager.LoadScene(1);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        }
        public void UpisPolja(TileData _plj)
        {
            polja[_plj.koor.x, _plj.koor.y] = _plj;
            if (_plj.Tiletype == TileType.Exit)
            {
                krajevi.Add(_plj);
                postojiKraj = true;
            }
        }
        public void OdrediPut(TileData _poc)
        {
            if (!postojiKraj)
            {
                krajevi.Add(polja[0, 0]);
                polja[0, 0].Tiletype = TileType.Exit;
                postojiKraj = true;
            }
            for (int i = 0; i < dimenzije; i++)
            {
                for (int j = 0; j < dimenzije; j++)
                {
                    if (polja[i, j].Tiletype == TileType.Walkable) polja[i, j].spriteTile.color = Color.white;
                }
            }
            List<TileData> konacniPut = new List<TileData>();
            List<int> udaljenostiDoKrajeva = new List<int>();
            for (int i = 0; i < krajevi.Count; i++)
            {
                konacniPut = Put(_poc, krajevi[i]);
                if (konacniPut != null) udaljenostiDoKrajeva.Add(konacniPut.Count);
                else udaljenostiDoKrajeva.Add(int.MaxValue);
            }
            int najkraci = udaljenostiDoKrajeva.Min();
            if(najkraci == int.MaxValue)
            {
                NemaIzlaza();
                return;
            }
            for (int i = 0; i < udaljenostiDoKrajeva.Count; i++)
            {
                if (udaljenostiDoKrajeva[i] == najkraci)
                {
                    konacniPut = Put(_poc, krajevi[i]);
                    foreach (TileData item in konacniPut)
                    {
                        if (item.Tiletype == TileType.Walkable) item.spriteTile.color = Color.green;
                    }
                    return;
                }
            }
        }
        void NemaIzlaza()
        {
            prikazGlavni.text = "Nema izlaza sa te pozicije! Klikni na neku drugu poziciju.";
            Invoke(nameof(MetodaPrikaziUpute), 5f);
        }
        List<TileData> Put(TileData _poc, TileData _zav)
        {
            otvorena = new List<TileData>();
            otvorena.Add(_poc);
            zatvorena = new List<TileData>();

            for (int i = 0; i < dimenzije; i++) //resetiranje polja
            {
                for (int j = 0; j < dimenzije; j++)
                {
                    if (polja[i, j].Tiletype != TileType.NotWalkable)
                    {
                        polja[i, j].Scost = int.MaxValue;
                        polja[i, j].previousTile = null;

                    }
                }
            }

            _poc.Scost = 0;
            _poc.Ecost = Udaljenost(_poc.koor, _zav.koor);

            while (otvorena.Count > 0)
            {
                TileData radni = P_najmanjiTOT(otvorena);
                if(radni == _zav)
                {
                    //zavrsetak
                    return UkupniPut(_zav);
                }
                otvorena.Remove(radni);
                zatvorena.Add(radni);
                foreach (TileData item in Susjedi(radni.koor))
                {
                    if (zatvorena.Contains(item)) continue;

                    int kostSusjed = radni.Scost + Udaljenost(radni.koor, item.koor);
                    if(kostSusjed < item.Scost)
                    {
                        item.previousTile = radni;
                        item.Scost = kostSusjed;
                        item.Ecost = Udaljenost(item.koor, _zav.koor);

                        if (!otvorena.Contains(item)) otvorena.Add(item);
                    }
                }
            }

            return null;
        }
        List<TileData> Susjedi(Vector2Int _koor)
        {
            List<TileData> lll = new List<TileData>();

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (ValjanSusjed(i, j)) lll.Add(polja[_koor.x + i, _koor.y + j]);
                }
            }

            bool ValjanSusjed(int x, int y) //lokalna metoda
            {
                if (x == 0 && y == 0) return false;
                if (x + _koor.x < 0 || y + _koor.y < 0 || x + _koor.x > dimenzije - 1 || y + _koor.y > dimenzije - 1) return false;
                if (polja[x + _koor.x, y + _koor.y].Tiletype == TileType.NotWalkable) return false;
                return true;
            }

            return lll;
        }
        List<TileData> UkupniPut(TileData zavrsnoPolje)
        {
            List<TileData> ll = new List<TileData>();
            ll.Add(zavrsnoPolje);
            TileData radno = zavrsnoPolje;
            while (radno.previousTile != null)
            {
                ll.Add(radno.previousTile);
                radno = radno.previousTile;
            }
            ll.Reverse();
            return ll;
        }
        TileData P_najmanjiTOT(List<TileData> _polje)
        {
            TileData najmanji = _polje[0];
            for (int i = 0; i < _polje.Count; i++)
            {
                if (_polje[i].Totcost() < najmanji.Totcost()) najmanji = _polje[i];
            }

            return najmanji;
        }
        int Udaljenost(Vector2Int _poc, Vector2Int _zav)
        {
            int hor = Mathf.Abs(_zav.x - _poc.x);
            int ver = Mathf.Abs(_zav.y - _poc.y);
            int ostatak = Mathf.Abs(hor - ver);
            return disDijagonal * Mathf.Min(hor, ver) + disRavno * ostatak;
        }
    }
}
