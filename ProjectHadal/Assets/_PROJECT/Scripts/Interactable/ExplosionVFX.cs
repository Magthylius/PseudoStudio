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

        // Start is called before the first frame update
        void Start()
        {
            
        }
        void OnEnable()
        {
            StartCoroutine(ExplosionActivate());
        }

        IEnumerator ExplosionActivate()
        {
            yield return null;
        }
    }
}
