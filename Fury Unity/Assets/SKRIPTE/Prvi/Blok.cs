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
        [SerializeField] SpriteRenderer sprite; //samo za boju
        Color _colorBlok;
        public Color ColorBlok 
        {
            get
            {
                return _colorBlok;
            }
            set
            {
                _colorBlok = value;
                sprite.color = _colorBlok;
            }
        }
    }
}
