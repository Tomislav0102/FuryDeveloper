using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TK.Enums;

namespace DrugiZadatak
{
    /// <summary>
    /// Skripta koja odreduje varijable svakog polja u labirintu
    /// </summary>
    public class TileData : MonoBehaviour
    {
        [HideInInspector] public Vector2Int koor; // koordinate u labirintu
        public SpriteRenderer spriteTile; //boja aodreduje funkciju polja
        [HideInInspector] public int Scost; //udaljenost da pocetne tocke
        [HideInInspector] public int Ecost; //udaljenost do izlaza
        public int Totcost()
        {
            return Scost + Ecost;
        }

        public TileData previousTile; //prethodno polje u nizu, ako je ovo polje dio puta prema izlazu
        TileType _tileType;
        public TileType Tiletype //vrsta polja.
        {
            get
            {
                return _tileType;
            }
            set
            {
                _tileType = value;
                switch (_tileType)
                {
                    case TileType.Walkable:
                        spriteTile.enabled = true;
                        break;
                    case TileType.Exit:
                        spriteTile.color = Color.red;
                        spriteTile.enabled = true;
                        break;
                }
            }
        }

    }
}
