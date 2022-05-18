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
        Blok[] blokovi; //svi blokovi. koristimo 'pool' sustav
        int RBblkovi; //brojac blokova, za kotrolu pool-a
        float rasponSpawnPoint = 18f; //unutar ove udaljenosti (po x-u) se spawnaju blokovi
        [HideInInspector] public int trenutniBrojBlokova; //pratimo koliko ih je na sceni
        const int maxBrojBlokova = 55; //maximalan broj blokovca na sceni
        [HideInInspector] public  Color crvena = Color.red;
        [HideInInspector] public Color plava = Color.blue;
        [SerializeField] float vremenskiRazmakSpawn = 1f; //rate-of-fire spawn-a blokova
        private void Awake()
        {
            gm = this;
        }

        private void Start()
        {
            blokovi = Magic.DjecaGeneralno<Blok>(par_blokovi); //koristim metodu ih helper scripte. uzima svu djecu iz 'par_blokovi' is stavlja ih u array
            InvokeRepeating(nameof(SpawnBlokova), 1f, vremenskiRazmakSpawn);
        }

        /// <summary>
        /// Prvo provjerava da li je previse blokova.
        /// Zatim ih uzima iz pool-a, daje boju i poziciju po nasumicnim vrijednostima.
        /// </summary>
        void SpawnBlokova()
        {
            if (trenutniBrojBlokova >= maxBrojBlokova) return;
            blokovi[RBblkovi].Boja = (Random.value < 0.5f) ? crvena : plava;
            blokovi[RBblkovi].transform.position = new Vector2(Random.Range(-rasponSpawnPoint * 0.5f, rasponSpawnPoint * 0.5f), 0f);
            blokovi[RBblkovi].gameObject.SetActive(true);
            RBblkovi = (1 + RBblkovi) % blokovi.Length;
            trenutniBrojBlokova++;
        }

    }
}
