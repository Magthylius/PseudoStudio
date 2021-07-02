using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Interactables
{
    public class SonicVFX : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Material sonicVFX;
        //[SerializeField] private MeshRenderer sonicRenderer;
        [SerializeField] private float startHeight;
        [SerializeField] private float endHeight;
        [SerializeField] private float speed;

        //private MaterialPropertyBlock materialProp;

        void Start()
        {
            /* materialProp = new MaterialPropertyBlock();
            if (materialProp != null) sonicRenderer.GetPropertyBlock(materialProp);
            sonicRenderer.SetPropertyBlock(materialProp); */
        }
        void OnEnable()
        {
            SonicActivate();
        }

        public void SonicActivate()
        {
            StartCoroutine(DissolveAnim());
        }

        IEnumerator DissolveAnim() {
            sonicVFX.SetFloat("_CuttoffHeight", startHeight);
            for(float t = startHeight; t <= endHeight; t+= Time.deltaTime * speed)
            {
                yield return null;
                sonicVFX.SetFloat("_CuttoffHeight", t);
            }
            sonicVFX.SetFloat("_CuttoffHeight", endHeight);
        }
    }
}
