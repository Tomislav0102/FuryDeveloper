using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TK.Enums;

namespace TK
{
    public class Magic
    {

        public static T[] GetAllChildren<T>(Transform _parent)
        {
            T[] _temp = new T[_parent.childCount];
            for (int i = 0; i < _parent.childCount; i++)
            {
                _temp[i] = _parent.GetChild(i).GetComponent<T>();
            }
            return _temp;
        }
        public static void IzaberiJednoDjete(GameObject[] djeca, bool rdn, int RB)
        {
            for (int i = 0; i < djeca.Length; i++)
            {
                djeca[i].SetActive(false);
            }
            if (rdn) djeca[Random.Range(0, djeca.Length - 1)].SetActive(true);
            else djeca[RB].SetActive(true);
        }

        public static List<int> Stoh_IntSimple(int velicina)
        {
            List<int> ula = new List<int>();
            List<int> zav = new List<int>();
            for (int i = 0; i < velicina; i++)
            {
                ula.Add(i);
            }
            int broj = ula.Count;
            for (int i = 0; i < broj; i++)
            {
                int rdn = Random.Range(0, ula.Count);
                zav.Add(ula[rdn]);
                ula.RemoveAt(rdn);
            }
            return zav;
        }

    }

    namespace Enums
    {
        public enum TileType
        {
            Walkable,
            NotWalkable,
            Exit
        }
    }
}