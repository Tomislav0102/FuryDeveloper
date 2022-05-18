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
        [SerializeField] SpriteRenderer spriteBlokCarry; //ovaj child se aktivira ako nosi blok
        [SerializeField] Collider2D[] containerCols; //mete gdje blkokve treba odnijeti
        bool _carryBlok;
        bool CarryBlok
        {
            get
            {
                return _carryBlok;
            }
            set
            {
                _carryBlok = value;
                spriteBlokCarry.enabled = _carryBlok;
                if (!_carryBlok)
                {
                    spriteBlokCarry.color = Color.white;
                }
            }
        }
        [SerializeField] LayerMask lejerBlok;
        Collider2D[] colliders2D;
        Transform targe; //moze biti null (na sceni nema ni jednog bloka), blok (ako ne nosi nista) ili posuda (nosi blok)
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
        [SerializeField] float speed;

        private void Awake()
        {
            gm = GameManager.gm;
            myTransform = transform;
            r2D = GetComponent<Rigidbody2D>();
        }
        private void Start()
        {
            CarryBlok = false;
        }

        /// <summary>
        /// samo odreduje orijentaciju robota. nema veze sa funkcionalnosti
        /// </summary>
        private void Update()
        {
            if (targe == null) Hor = 0f;
            else
            {
                Hor = targe.position.x - myTransform.position.x;
                Hor = Mathf.Clamp(Hor, -1f, 1f);
            }
        }

        /// <summary>
        /// Odreduje kretanje i pronalazi novi blok ako nista ne nosi a blok je na sceni
        /// </summary>
        private void FixedUpdate()
        {
            r2D.velocity = Hor * speed * Vector2.right;

            if (CarryBlok) return;
            if (targe != null) return;

            colliders2D = Physics2D.OverlapBoxAll(Vector2.up * 2f, new Vector2(20f, 4f), 0f, lejerBlok);
            if (colliders2D.Length <= 0) return;
            targe = colliders2D[Random.Range(0, colliders2D.Length)].transform;

        }

        /// <summary>
        /// Odeduje kontakt sa blokovima i posudama
        /// </summary>
        /// <param name="collision">blokovi ili posude</param>
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (CarryBlok)
            {
                if(collision == containerCols[0]) //crvena
                {
                    if (spriteBlokCarry.color != gm.redColor) return;
                    Istovar();
                }
                else if (collision == containerCols[1]) //plava
                {
                    if (spriteBlokCarry.color != gm.blueColor) return;
                    Istovar();
                }
            }
            else if(collision.TryGetComponent<Blok>(out Blok kol))
            {
                if (kol.transform != targe) return;

                spriteBlokCarry.color = kol.ColorBlok;
                CarryBlok = true;
                if (spriteBlokCarry.color == gm.redColor) targe = containerCols[0].transform;
                else targe = containerCols[1].transform;
                kol.gameObject.SetActive(false);
            }

        }

        /// <summary>
        /// Istovar bloka u popsudu
        /// </summary>
        private void Istovar()
        {
            CarryBlok = false;
            gm.currentBlokNum--;
            targe = null;
        }
    }
}
