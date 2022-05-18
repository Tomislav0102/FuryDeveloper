using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TK.Enums;

namespace DrugiZadatak
{
    /// <summary>
    /// Skripta koja odreduje varijable svakog polja u labirintu
    /// </summary>
    public class Polje : MonoBehaviour
    {
        public Vector2Int koor; // koordinate u labirintu
        public SpriteRenderer sprite; //boja aodreduje funkciju polja
        public int Scost; //udaljenost da pocetne tocke
        public int Ecost; //udaljenost do izlaza
        public int Totcost()
        {
            return Scost + Ecost;
        }

        public Polje prethodnoPolje; //prethodno polje u nizu, ako je ovo polje dio puta prema izlazu
        VrstaTile _vrsta;
        public VrstaTile Vrsta //vrsta polja. VrstaTile.Zid se ne koristi
        {
            get
            {
                return _vrsta;
            }
            set
            {
                _vrsta = value;
                switch (_vrsta)
                {
                    case VrstaTile.Prohodno:
                        sprite.enabled = true;
                        break;
                    case VrstaTile.Kraj:
                        sprite.color = Color.red;
                        sprite.enabled = true;
                        break;
                }
            }
        }

    }
}
