using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TK;

namespace PrviZadatak
{
    /// <summary>
    /// Samo za spawn novih blokova
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager gm; //radimo instance zbog jednostavnosti (tj. navike). nema neke potrebe za ovim konceptom
        [SerializeField] Transform par_blokovi; //ovdje se nalaze svi blokovi
        Blok[] bloks; //svi blokovi. koristimo 'pool' sustav
        int RBblkovi; //brojac blokova, za kotrolu pool-a
        float areaSpawnPoint = 18f; //unutar ove udaljenosti (po x-u) se spawnaju blokovi
        [HideInInspector] public int currentBlokNum; //pratimo koliko ih je na sceni
        const int maxBlokNum = 55; //maximalan broj blokovca na sceni
        [HideInInspector] public  Color redColor = Color.red;
        [HideInInspector] public Color blueColor = Color.blue;
        [SerializeField] float timeBetweenSpawns = 1f; //rate-of-fire spawn-a blokova
        private void Awake()
        {
            gm = this;
        }

        private void Start()
        {
            bloks = Magic.GetAllChildren<Blok>(par_blokovi); //koristim metodu ih helper scripte. uzima svu djecu iz 'par_blokovi' is stavlja ih u array
            InvokeRepeating(nameof(SpawnBloks), 1f, timeBetweenSpawns);
        }

        /// <summary>
        /// Prvo provjerava da li je previse blokova.
        /// Zatim ih uzima iz pool-a, daje boju i poziciju po nasumicnim vrijednostima.
        /// </summary>
        void SpawnBloks()
        {
            if (currentBlokNum >= maxBlokNum) return;
            bloks[RBblkovi].ColorBlok = (Random.value < 0.5f) ? redColor : blueColor;
            bloks[RBblkovi].transform.position = new Vector2(Random.Range(-areaSpawnPoint * 0.5f, areaSpawnPoint * 0.5f), 0f);
            bloks[RBblkovi].gameObject.SetActive(true);
            RBblkovi = (1 + RBblkovi) % bloks.Length;
            currentBlokNum++;
        }

    }
}
