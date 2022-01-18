using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Interactables
{
    public class ExplosionVFX : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private MeshRenderer explosion1;
        [SerializeField] private MeshRenderer explosion2;
        [SerializeField] private MeshRenderer explosion3;

        private MaterialPropertyBlock materialProp1;
        private MaterialPropertyBlock materialProp2;
        private MaterialPropertyBlock materialProp3;

        [SerializeField] private float speed1;
        [SerializeField] private float speed2;
        [SerializeField] private float speed3;

        // Start is called before the first frame update
        void Awake()
        {
            materialProp1 = new MaterialPropertyBlock();
            if (materialProp1 != null) explosion1.GetPropertyBlock(materialProp1);
            materialProp2 = new MaterialPropertyBlock();
            if (materialProp2 != null) explosion2.GetPropertyBlock(materialProp2);
            materialProp3 = new MaterialPropertyBlock();
            if (materialProp3 != null) explosion3.GetPropertyBlock(materialProp3);

            explosion1.SetPropertyBlock(materialProp1);
            explosion2.SetPropertyBlock(materialProp2);
            explosion3.SetPropertyBlock(materialProp3);
        }
        
        void OnEnable()
        {
            //explosionAura.transform.localScale = new Vector3(0,0,0);
            //explosionAura2.transform.localScale = new Vector3(0,0,0);
            //StartCoroutine(Explosion());
            StartCoroutine(ExplosionActivate());
            StartCoroutine(ExplosionActivate2());
            StartCoroutine(ExplosionActivate3());
        }

        IEnumerator Explosion()
        {
            for(float t = 0.0f; t <= 3.0f; t+= Time.deltaTime * 2.5f)
            {
                explosion1.transform.localScale = explosion1.transform.localScale * 1.005f;
                explosion2.transform.localScale = explosion2.transform.localScale * 1.005f;
                explosion3.transform.localScale = explosion3.transform.localScale * 1.005f;
                yield return null;
            }

        }

        IEnumerator ExplosionActivate()
        {
            for(float t = 1.0f; t >= -1.3f; t-= Time.deltaTime * speed1)
            {
                materialProp1.SetFloat("_Addition", t);
                explosion1.SetPropertyBlock(materialProp1);
                yield return null;
            }

        }

        IEnumerator ExplosionActivate2()
        {
            for(float t = 0.5f; t >= 0.0f; t-= Time.deltaTime * speed2)
            {
                materialProp2.SetFloat("_Alpha", t);
                explosion2.SetPropertyBlock(materialProp2);
                yield return null;
            }
            materialProp2.SetFloat("_Alpha", 0.0f);
            explosion2.SetPropertyBlock(materialProp2);
        }

        IEnumerator ExplosionActivate3()
        {
            for(float t = -0.8f; t >= -1.0f; t-= Time.deltaTime * speed3)
            {
                materialProp3.SetFloat("_Dissolve", t);
                explosion3.SetPropertyBlock(materialProp3);
                yield return null;
            }
        }
    }
}
