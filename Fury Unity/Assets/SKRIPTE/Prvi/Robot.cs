using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrviZadatak
{
    /// <summary>
    /// Glavna skripta, sva logika je tu
    /// </summary>
    public class Robot : MonoBehaviour
    {
        GameManager gm;
        Transform myTransform;
        Rigidbody2D r2D;
        [SerializeField] SpriteRenderer spriteBlokNosi; //ovaj child se aktivira ako nosi blok
        [SerializeField] Collider2D[] posudaCols; //mete gdje blkokve treba odnijeti
        bool _nosiBlok;
        bool NosiBlok
        {
            get
            {
                return _nosiBlok;
            }
            set
            {
                _nosiBlok = value;
                spriteBlokNosi.enabled = _nosiBlok;
                if (!_nosiBlok)
                {
                    spriteBlokNosi.color = Color.white;
                }
            }
        }
        [SerializeField] LayerMask lejerBlok;
        Collider2D[] hits2D;
        Transform meta; //moze biti null (na sceni nema ni jednog bloka), blok (ako ne nosi nista) ili posuda (nosi blok)
        float _hor;
        float Hor //odreduje smjer kretanja. format je 'property' jer utjece na localScale.x, sto stvara iluziju da je robot okrenut na neku stranu
        {
            get
            {
                return _hor;
            }
            set
            {
                _hor = value;
                if (_hor < 0f) myTransform.localScale = new Vector3(-1f, 2f, 1f);
                else if (_hor > 0f) myTransform.localScale = new Vector3(1f, 2f, 1f);
            }
        }
        [SerializeField] float brzina;

        private void Awake()
        {
            gm = GameManager.gm;
            myTransform = transform;
            r2D = GetComponent<Rigidbody2D>();
        }
        private void Start()
        {
            NosiBlok = false;
        }

        /// <summary>
        /// samo odreduje orijentaciju robota. nema veze sa funkcionalnosti
        /// </summary>
        private void Update()
        {
            if (meta == null) Hor = 0f;
            else
            {
                Hor = meta.position.x - myTransform.position.x;
                Hor = Mathf.Clamp(Hor, -1f, 1f);
            }
        }

        /// <summary>
        /// Odreduje kretanje i pronalazi novi blok ako nista ne nosi a blok je na sceni
        /// </summary>
        private void FixedUpdate()
        {
            r2D.velocity = Hor * brzina * Vector2.right;

            if (NosiBlok) return;
            if (meta != null) return;

            hits2D = Physics2D.OverlapBoxAll(Vector2.up * 2f, new Vector2(20f, 4f), 0f, lejerBlok);
            if (hits2D.Length <= 0) return;
            meta = hits2D[Random.Range(0, hits2D.Length)].transform;

        }

        /// <summary>
        /// Odeduje kontakt sa blokovima i posudama
        /// </summary>
        /// <param name="collision">blokovi ili posude</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (NosiBlok)
            {
                if(collision == posudaCols[0]) //crvena
                {
                    if (spriteBlokNosi.color != gm.crvena) return;
                    Istovar();
                }
                else if (collision == posudaCols[1]) //plava
                {
                    if (spriteBlokNosi.color != gm.plava) return;
                    Istovar();
                }
            }
            else if(collision.TryGetComponent<Blok>(out Blok kol))
            {
                if (kol.transform != meta) return;

                spriteBlokNosi.color = kol.Boja;
                NosiBlok = true;
                if (spriteBlokNosi.color == gm.crvena) meta = posudaCols[0].transform;
                else meta = posudaCols[1].transform;
                kol.gameObject.SetActive(false);
            }

        }

        /// <summary>
        /// Istovar bloka u popsudu
        /// </summary>
        private void Istovar()
        {
            NosiBlok = false;
            gm.trenutniBrojBlokova--;
            meta = null;
        }
    }
}
