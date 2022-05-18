using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DrugiZadatak
{
    public class test : MonoBehaviour
    {
        Camera cam;
        public Vector2Int mPoz;
        Vector2 v2;
        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void Update()
        {
            v2 = cam.ScreenToWorldPoint(Input.mousePosition);
            mPoz = new Vector2Int(Mathf.FloorToInt(v2.x), Mathf.FloorToInt(v2.y));
        }
    }
}
