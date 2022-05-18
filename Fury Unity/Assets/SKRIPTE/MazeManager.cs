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
        [SerializeField] bool dopustenoDijagonalno = true; //kretajne po dijagonali. ako je 'false' kretanje je samo ortogonalno
        [SerializeField] Toggle tog_Dop; //UI element za ukljucivanje/iskljucivanje gornjeg bool-a
        Vector2Int mainPoz; //pozicija misa
        [SerializeField] Camera cam; //kamera, potrebno zbog odredivanja pozcije misa
        [SerializeField] TextMeshProUGUI prikazGlavni; 
        [SerializeField] Polje pPrefab; //prefab polja koji ce se spawnati. Osnovni element labirinta
        [SerializeField] Transform parPolja; //parent transform za polja. jedina svrha je preglednost u editor-u
        [SerializeField] int dimenzije; //dimenzije labirinta

        Polje[,] polja; //raspored polja u labirintu prikazanat kroz 2D array
        List<Polje> krajevi = new List<Polje>(); //polja koja su izlaz iz labirinta
        bool postojiKraj; //potvrda da je barem jedan izlaz generiran
        const int disRavno = 10; //"cijena" kretanja ortogonalno
        const int disDijagonal = 14; //"cijena" kretnja dijagonalno
        List<Polje> otvorena; //polja koja treba pretraziti
        List<Polje> zatvorena; //vec pretrazena polja

        private void Awake()
        {
            Inicijalizacija();
        }
        /// <summary>
        /// Namjesti kameru ovisno o dimenzijama, stvori i definira polja ovisno o nasumicnim parametrima.
        /// </summary>
        void Inicijalizacija()
        {
            cam.transform.position = new Vector3(dimenzije * 0.5f, dimenzije * 0.5f, -10f);
            cam.orthographicSize = 0.67f * dimenzije;
            polja = new Polje[dimenzije, dimenzije];
            for (int i = 0; i < dimenzije; i++)
            {
                for (int j = 0; j < dimenzije; j++)
                {
                    if (Random.value > 0.4f)
                    {
                        polja[i, j] = Instantiate(pPrefab, new Vector2(i, j), Quaternion.identity, parPolja);
                        polja[i, j].koor = new Vector2Int(i, j);
                        polja[i, j].name = "Polje (" + i + "," + j + ")";
                        if ((i == 0 || j == 0 || i == dimenzije - 1 || j == dimenzije - 1) && Random.value > 0.9f)
                        {
                            polja[i, j].Vrsta = VrstaTile.Kraj;
                            krajevi.Add(polja[i, j]);
                            postojiKraj = true;

                        }
                    }

                }
            }
        }

        private void Start()
        {
            MetodaPrikaziUpute();
            tog_Dop.isOn = dopustenoDijagonalno;
        }
        void MetodaPrikaziUpute() => prikazGlavni.text = "Klikni bilo gdje na labirintu. Put do najblizeg izlaz ce biti oznacen zelenom bojom.";
        public void G_Restart() => SceneManager.LoadScene(gameObject.scene.name);
        public void G_ToggleDopDijagonala() => dopustenoDijagonalno = tog_Dop.isOn;
        /// <summary>
        /// Prva linija je samo za "build"
        /// Druga linija je oznacavanje pocetnog polja za trazenej izlaza
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            if (Input.GetMouseButtonDown(0)) Upravljanje();
        }

        /// <summary>
        /// Nalazenje pocetne tocke.
        /// Ako je klik validan pokrece glavnu metodu.
        /// </summary>
        private void Upravljanje()
        {
            Vector2 poz = cam.ScreenToWorldPoint(Input.mousePosition);
            mainPoz = new Vector2Int(Mathf.FloorToInt(poz.x), Mathf.FloorToInt(poz.y));
            if (mainPoz.x < 0 || mainPoz.y < 0 || mainPoz.x > dimenzije - 1 || mainPoz.y > dimenzije - 1 || polja[mainPoz.x, mainPoz.y] == null)
            {
                prikazGlavni.text = "To nije prohodni dio labirinta. Klikni na neku drugu poziciju.";
                Invoke(nameof(MetodaPrikaziUpute), 3f);
                return;
            }
            OdrediPut(polja[mainPoz.x, mainPoz.y]);
        }

        /// <summary>
        /// Glavna metoda.
        /// Prvo odreduje kraj ako takav ne postoji. 
        /// Resetira boju tile-ova.
        /// Za sve krajeve pronalazi puteve (lista) i odreduje najkraci te ga iscrtava.
        /// Ako nema izlaza to javlja korisniku.
        /// </summary>
        /// <param name="_poc">ovdje ulazi polje na koje je korisnik kliknuo</param>
        public void OdrediPut(Polje _poc)
        {
            if (!postojiKraj)
            {
                krajevi.Add(polja[0, 0]);
                polja[0, 0].Vrsta = VrstaTile.Kraj;
                postojiKraj = true;
            }
            for (int i = 0; i < dimenzije; i++)
            {
                for (int j = 0; j < dimenzije; j++)
                {
                    if (polja[i, j] != null && polja[i, j].Vrsta == VrstaTile.Prohodno) polja[i, j].sprite.color = Color.white;
                }
            }
            List<Polje> konacniPut = new List<Polje>();
            List<int> udaljenostiDoKrajeva = new List<int>();
            for (int i = 0; i < krajevi.Count; i++)
            {
                konacniPut = Put(_poc, krajevi[i]);
                if (konacniPut != null) udaljenostiDoKrajeva.Add(konacniPut.Count);
                else udaljenostiDoKrajeva.Add(int.MaxValue);
            }
            int najkraci = udaljenostiDoKrajeva.Min();
            if (najkraci == int.MaxValue)
            {
                NemaIzlaza();
                return;
            }
            for (int i = 0; i < udaljenostiDoKrajeva.Count; i++)
            {
                if (udaljenostiDoKrajeva[i] == najkraci)
                {
                    konacniPut = Put(_poc, krajevi[i]);
                    foreach (Polje item in konacniPut)
                    {
                        if (item.Vrsta == VrstaTile.Prohodno) item.sprite.color = Color.green;
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
        /// <summary>
        /// Odredivanje pojedinacnog puta po svakom izlazu
        /// </summary>
        /// <param name="_poc">polje na koje je korisnik kliknuo</param>
        /// <param name="_zav">izlaz</param>
        /// <returns></returns>
        List<Polje> Put(Polje _poc, Polje _zav)
        {
            otvorena = new List<Polje>();
            otvorena.Add(_poc);
            zatvorena = new List<Polje>();

            for (int i = 0; i < dimenzije; i++) //resetiranje polja
            {
                for (int j = 0; j < dimenzije; j++)
                {
                    if (polja[i, j] != null)
                    {
                        polja[i, j].Scost = int.MaxValue;
                        polja[i, j].prethodnoPolje = null;

                    }
                }
            }

            _poc.Scost = 0;
            _poc.Ecost = Udaljenost(_poc.koor, _zav.koor);

            while (otvorena.Count > 0)
            {
                Polje radni = P_najmanjiTOT(otvorena);
                if (radni == _zav)
                {
                    //zavrsetak
                    return UkupniPut(_zav);
                }
                otvorena.Remove(radni);
                zatvorena.Add(radni);
                foreach (Polje item in Susjedi(radni.koor))
                {
                    if (zatvorena.Contains(item)) continue;

                    int kostSusjed = radni.Scost + Udaljenost(radni.koor, item.koor);
                    if (kostSusjed < item.Scost)
                    {
                        item.prethodnoPolje = radni;
                        item.Scost = kostSusjed;
                        item.Ecost = Udaljenost(item.koor, _zav.koor);

                        if (!otvorena.Contains(item)) otvorena.Add(item);
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
        List<Polje> Susjedi(Vector2Int _koor)
        {
            List<Polje> lll = new List<Polje>();

            if (dopustenoDijagonalno)
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        if (ValjanSusjed(i, j)) lll.Add(polja[_koor.x + i, _koor.y + j]);
                    }
                }
            }
            else
            {
                if (ValjanSusjed(1, 0)) lll.Add(polja[_koor.x + 1, _koor.y]);
                if (ValjanSusjed(- 1, 0)) lll.Add(polja[_koor.x - 1, _koor.y]);
                if (ValjanSusjed(0, 1)) lll.Add(polja[_koor.x, _koor.y + 1]);
                if (ValjanSusjed(0, - 1)) lll.Add(polja[_koor.x, _koor.y - 1]);
            }

            bool ValjanSusjed(int x, int y) //lokalna metoda
            {
                if (x + _koor.x < 0 || y + _koor.y < 0 || x + _koor.x > dimenzije - 1 || y + _koor.y > dimenzije - 1) return false;
                if (polja[x + _koor.x, y + _koor.y] == null) return false;
                if (x == 0 && y == 0) return false;
                return true;
            }

            return lll;
        }
        /// <summary>
        /// Ako izlaz postoji ovo ce biti lista polja koja ga odreduje
        /// </summary>
        /// <param name="zavrsnoPolje"></param>
        /// <returns></returns>
        List<Polje> UkupniPut(Polje zavrsnoPolje)
        {
            List<Polje> ll = new List<Polje>();
            ll.Add(zavrsnoPolje);
            Polje radno = zavrsnoPolje;
            while (radno.prethodnoPolje != null)
            {
                ll.Add(radno.prethodnoPolje);
                radno = radno.prethodnoPolje;
            }
            ll.Reverse();
            return ll;
        }
        /// <summary>
        /// Definira polje najblize izlazu iz ponudjenih koristeci A* algoritam logiiku
        /// </summary>
        /// <param name="_polje">lista svih polja koja pretrazujemo</param>
        /// <returns></returns>
        Polje P_najmanjiTOT(List<Polje> _polje)
        {
            Polje najmanji = _polje[0];
            for (int i = 0; i < _polje.Count; i++)
            {
                if (_polje[i].Totcost() < najmanji.Totcost()) najmanji = _polje[i];
            }

            return najmanji;
        }
        /// <summary>
        /// Udaljenost izmedu polja
        /// </summary>
        /// <param name="_poc">prvo polje</param>
        /// <param name="_zav">drugo polje</param>
        /// <returns></returns>
        int Udaljenost(Vector2Int _poc, Vector2Int _zav)
        {
            int hor = Mathf.Abs(_zav.x - _poc.x);
            int ver = Mathf.Abs(_zav.y - _poc.y);
            int ostatak = Mathf.Abs(hor - ver);
            return disDijagonal * Mathf.Min(hor, ver) + disRavno * ostatak;
        }

    }
}
