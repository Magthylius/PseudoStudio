using System.Collections;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;

namespace Hadal.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class MushroomBehaviour : MonoBehaviour
    {
        [Header("Colour Data")]
        [SerializeField] private MushroomColourData reactiveColourData;
        [SerializeField, ReadOnly] private MushroomColourData unreactiveColourData;
        [SerializeField, ReadOnly] private bool _isReactive;
        [SerializeField, ReadOnly] private float _percent;

        [Header("Renderers")]
        [SerializeField] private List<MeshRenderer> renderersInner;
        [SerializeField] private List<MeshRenderer> renderersOuter;

        [Header("Misc")]
        [SerializeField] private float reactiveTransitionSpeed;
        [SerializeField] private float unreactiveTransitionSpeed;
        [SerializeField] private LayerMask reactiveMasks;
        [SerializeField, ReadOnly] private List<Material> materialsInner;
        [SerializeField, ReadOnly] private List<Material> materialsOuter;

        private readonly string BorderColour = "_Color";

        [ContextMenu("Trigger Awake")]
        private void Awake()
        {
            renderersInner ??= new List<MeshRenderer>();
            renderersOuter ??= new List<MeshRenderer>();
            materialsInner ??= new List<Material>();
            materialsOuter ??= new List<Material>();
            _isReactive = false;
            _percent = 0f;

            const int count = 2;
            int i = -1;
            while (++i < count)
            {
                materialsInner.Add(renderersInner[i].material);
                materialsOuter.Add(renderersOuter[i].material);
            }
            return;
            //! Default colour data
            unreactiveColourData = new MushroomColourData
            {
                InnerColour = materialsInner[0].GetColor(BorderColour),
                OuterColour = materialsOuter[0].GetColor(BorderColour)
            };
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasViableColourData || !CanTransition) return;
            if (CanCollide(other))
            {
                StopAllCoroutines();
                _isReactive = true;
                StartCoroutine(ReactiveTransition());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!HasViableColourData || !CanTransition) return;
            if (CanCollide(other))
            {
                StopAllCoroutines();
                _isReactive = false;
                StartCoroutine(UnreactiveTransition());
            }
        }

        private void SetMaterialColour()
        {
            return;
            _percent = _percent.Clamp01();
            MushroomColourData data = _isReactive ? reactiveColourData : unreactiveColourData;
            Material innerMat = materialsInner[0];
            Material outerMat = materialsOuter[0];
            Color innerCol = Color.Lerp(innerMat.GetColor(BorderColour), data.InnerColour, _percent);
            Color outerCol = Color.Lerp(outerMat.GetColor(BorderColour), data.OuterColour, _percent);

            const int count = 2;
            int i = -1;
            while (++i < count)
            {
                materialsInner[i].SetColor(BorderColour, innerCol);
                materialsOuter[i].SetColor(BorderColour, outerCol);
            }
        }

        private IEnumerator ReactiveTransition()
        {
            while (_percent < 1f)
            {
                _percent += DeltaTime * reactiveTransitionSpeed;
                SetMaterialColour();
                yield return null;
            }
        }

        private IEnumerator UnreactiveTransition()
        {
            while (_percent > 0f)
            {
                _percent -= DeltaTime * unreactiveTransitionSpeed;
                SetMaterialColour();
                yield return null;
            }
        }

        private float DeltaTime => Time.deltaTime;
        private bool CanCollide(Collider other) => other.gameObject.layer.IsAMatchingMask(reactiveMasks);
        private bool CanTransition
            => !renderersInner.IsNullOrEmpty()
            && !renderersOuter.IsNullOrEmpty()
            && !materialsInner.IsNullOrEmpty()
            && !materialsOuter.IsNullOrEmpty();
        private bool HasViableColourData => unreactiveColourData != null && reactiveColourData != null;
    }

    [System.Serializable]
    public class MushroomColourData
    {
        public Color OuterColour = Color.white;
        public Color InnerColour = Color.white;
    }
}
