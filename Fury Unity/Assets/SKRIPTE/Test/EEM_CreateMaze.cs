using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrugiZadatak
{
    [ExecuteInEditMode]
    public class EEM_CreateMaze : MonoBehaviour
    {
        [SerializeField] GameObject poljePrefab;
        [SerializeField] Transform parPrefab;
        public bool pokreni;


        private void Update()
        {
            if (pokreni)
            {
                for (int i = 0; i < 100; i++)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        Instantiate(poljePrefab, parPrefab);
                    }
                }

                pokreni = false;
            }
        }
    }
}
