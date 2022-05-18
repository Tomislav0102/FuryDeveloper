using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrviZadatak
{
    /// <summary>
    /// Ova skripta je samo spremnik vreijednosti za blok.
    /// Boja odreduje namjenu (funkcionalnost), a ne samo prikaz za korisnika
    /// </summary>
    public class Blok : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprajt; //samo za boju
        Color _boja;
        public Color Boja 
        {
            get
            {
                return _boja;
            }
            set
            {
                _boja = value;
                sprajt.color = _boja;
            }
        }
    }
}
