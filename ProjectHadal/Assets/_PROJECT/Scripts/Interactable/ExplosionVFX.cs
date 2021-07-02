using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Interactables
{
    public class ExplosionVFX : MonoBehaviour
    {
        [Header("Settings")]

        [SerializeField] private Material explosion;
        [SerializeField] private Transform explosionAura;
        [SerializeField] private Transform explosionAura2;

        [SerializeField] private float startScale;
        [SerializeField] private float endScale;
        [SerializeField] private float speed;

        // Start is called before the first frame update
        void Start()
        {
            
        }
        
        void OnEnable()
        {
            //explosionAura.transform.localScale = new Vector3(0,0,0);
            //explosionAura2.transform.localScale = new Vector3(0,0,0);
            StartCoroutine(ExplosionActivate());
        }

        IEnumerator ExplosionActivate()
        {
            for(float t = 1.0f; t >= -1.1f; t-= Time.deltaTime * speed)
            {
                explosion.SetFloat("_Addition", t);
                yield return null;
            }
        }
    }
}
