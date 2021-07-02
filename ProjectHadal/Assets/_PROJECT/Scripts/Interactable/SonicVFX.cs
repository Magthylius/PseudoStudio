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
            StartCoroutine(DissolveAnim());
        }

        IEnumerator DissolveAnim() {
            materialProp.SetFloat("_CuttoffHeight", startHeight);
            for(float t = startHeight; t <= endHeight; t+= Time.deltaTime * speed)
            {
                materialProp.SetFloat("_CuttoffHeight", t);
                sonicRenderer.SetPropertyBlock(materialProp);
                yield return null;
            }
            materialProp.SetFloat("_CuttoffHeight", endHeight);
        }
    }
}
