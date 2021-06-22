using System.Collections;
using System.Collections.Generic;
using Tenshi;
using UnityEngine;

namespace Hadal.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class MushroomBehaviour : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, MinMaxSlider(0.1f, 10f)] private Vector2 reactiveTimeoutRange;

        [Header("Colour Data")]
        [SerializeField] private MushroomShaderData reactiveData;
        [SerializeField, ReadOnly] private MushroomShaderData unreactiveData;
        [SerializeField, ReadOnly] private MushroomShaderData currentData;
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

        private bool _canBecomeUnreactive;
        private bool _isReactive;
        private float reactiveTimeoutTimer;
        private HashSet<Collider> contacts;

        private readonly string BorderColour = "_BorderColor";
        private readonly string Colour = "_Color";
        private readonly string BorderPower = "_BorderPower";
        private readonly string NoiseScale = "_NoiseScale";
		private readonly string Alpha = "_Alpha";
		private readonly string EmissionRate = "_EmissionRate";
		private readonly string EmissionBreathing = "_EmissionBreathing";

        private void Awake()
        {
            renderersInner ??= new List<MeshRenderer>();
            renderersOuter ??= new List<MeshRenderer>();
            materialsInner ??= new List<Material>();
            materialsOuter ??= new List<Material>();
            _canBecomeUnreactive = false;
            _isReactive = false;
            contacts = new HashSet<Collider>();
            _percent = 0f;

            currentData = new MushroomShaderData();
            int count = renderersOuter.Count;
            int i = -1;
            while (++i < count)
            {
                materialsOuter.Add(renderersOuter[i].material);
                materialsOuter[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                materialsOuter[i].SetFloat(BorderPower, 0.3f);

                materialsInner.Add(renderersInner[i].material);
                materialsInner[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            //! Default colour data
            unreactiveData = new MushroomShaderData
            {
                BorderColour = materialsOuter[0].GetColor(BorderColour),
                Colour = materialsInner[0].GetColor(Colour),
                BorderPower = materialsOuter[0].GetFloat(BorderPower),
                NoiseScale = materialsOuter[0].GetFloat(NoiseScale),
				Alpha = materialsOuter[0].GetFloat(Alpha)
            };
			
			//! Default emission settings
			materialsInner[0].SetFloat(EmissionRate, Random.Range(reactiveData.EmissionRate.x, reactiveData.EmissionRate.y));
			materialsInner[0].SetFloat(EmissionBreathing, Random.Range(reactiveData.EmissionBreathing.x, reactiveData.EmissionBreathing.y));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasViableColourData || !CanTransition) return;
            if (CanCollide(other))
            {
                if (contacts.Contains(other))
                    return;
                contacts.Add(other);

                if (_isReactive)
                    return;
                _canBecomeUnreactive = false;
                StopAllCoroutines();
                StartCoroutine(ReactiveTransition());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!HasViableColourData || !CanTransition) return;
            if (CanCollide(other))
            {
                contacts.Remove(other);
                if (contacts.IsEmpty())
                    _canBecomeUnreactive = true;
            }
        }

        private void SetMaterialColour()
        {
            _percent = _percent.Clamp01();
            currentData.Lerp(unreactiveData, reactiveData, _percent);

            int count = materialsOuter.Count;
            int i = -1;
            while (++i < count)
            {
                materialsOuter[i].SetColor(BorderColour, currentData.BorderColour);
                materialsInner[i].SetColor(Colour, currentData.Colour);
                materialsOuter[i].SetFloat(BorderPower, currentData.BorderPower);
                materialsOuter[i].SetFloat(NoiseScale, currentData.NoiseScale);
				materialsOuter[i].SetFloat(Alpha, currentData.Alpha);
            }
        }

        private IEnumerator ReactiveTransition()
        {
            _isReactive = true;

            //! Become Reactive
            while (_percent < 1f)
            {
                _percent += DeltaTime * reactiveTransitionSpeed;
                SetMaterialColour();
                yield return null;
            }

            //! Wait for unreactive cue & timer
            while (!_canBecomeUnreactive)
                yield return null;

            yield return new WaitForSeconds(Random.Range(reactiveTimeoutRange.x, reactiveTimeoutRange.y));

            //! Become Unreactive
            StartCoroutine(UnreactiveTransition());
        }

        private IEnumerator UnreactiveTransition()
        {
            while (_percent > 0f)
            {
                _percent -= DeltaTime * unreactiveTransitionSpeed;
                SetMaterialColour();
                yield return null;
            }

            _isReactive = false;
        }

        private float DeltaTime => Time.deltaTime;
        private bool CanCollide(Collider other) => other.gameObject.layer.IsAMatchingMask(reactiveMasks);
        private bool CanTransition
            => !renderersOuter.IsNullOrEmpty()
            && !materialsOuter.IsNullOrEmpty();
        private bool HasViableColourData => unreactiveData != null && reactiveData != null;

        private void ResetTimeoutTimer() => reactiveTimeoutTimer = Random.Range(reactiveTimeoutRange.x, reactiveTimeoutRange.y);
        private float TickTimeoutTimer(in float deltaTime) => reactiveTimeoutTimer -= deltaTime;
        private bool ReactiveTimeoutTimerReached => reactiveTimeoutTimer <= 0f;
    }

    [System.Serializable]
    public class MushroomShaderData
    {
        public Color BorderColour = Color.white;
        public Color Colour = Color.white;
        public float BorderPower = 0f;
        public float NoiseScale = 0f;
		public float Alpha = 0.9f;
		[MinMaxSlider(0.1f, 50f)] public Vector2 EmissionRate;
		[MinMaxSlider(0.1f, 50f)] public Vector2 EmissionBreathing;
        public void Lerp(MushroomShaderData a, MushroomShaderData b, in float percent)
        {
            BorderColour = Color.Lerp(a.BorderColour, b.BorderColour, percent);
            Colour = Color.Lerp(a.Colour, b.Colour, percent);
            BorderPower = Mathf.Lerp(a.BorderPower, b.BorderPower, percent);
            NoiseScale = Mathf.Lerp(a.NoiseScale, b.NoiseScale, percent);
			Alpha = Mathf.Lerp(a.Alpha, b.Alpha, percent);
        }
    }
}