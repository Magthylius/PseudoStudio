using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Interactables
{
    public class SonicVFX : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private MeshRenderer sonicRenderer;
        [SerializeField] private float startHeight;
        [SerializeField] private float endHeight;
        [SerializeField] private float speed;

        private MaterialPropertyBlock materialProp;

        void Awake()
        {
            materialProp = new MaterialPropertyBlock();
            if (materialProp != null) sonicRenderer.GetPropertyBlock(materialProp);
            sonicRenderer.SetPropertyBlock(materialProp);
        }
        void OnEnable()
        {
            SonicActivate();
        }

        public void SonicActivate()
        {
            //StartCoroutine(Sonic());
            StartCoroutine(DissolveAnim());
        }

        IEnumerator Sonic()
        {
            for(float t = 0.0f; t <= 3.0f; t+= Time.deltaTime * 2.5f)
            {
                this.transform.localScale = this.transform.localScale * 1.035f;
                yield return null;
            }

        }
        IEnumerator DissolveAnim() {
            materialProp.SetFloat("_CuttoffHeight", startHeight);
            for(float t = startHeight; t <= endHeight; t+= Time.deltaTime * speed)
            {
                materialProp.SetFloat("_CuttoffHeight", t);
                sonicRenderer.SetPropertyBlock(materialProp);
                yield return null;
            }
            //materialProp.SetFloat("_CuttoffHeight", endHeight);
        }
    }
}
