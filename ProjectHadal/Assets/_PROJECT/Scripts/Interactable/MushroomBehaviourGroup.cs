using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tenshi;
using UnityEngine;

namespace Hadal.Interactables
{
    public class MushroomBehaviourGroup : MonoBehaviour
    {
        [Header("Mushrooms")]
        [SerializeField] private string groupName = string.Empty;
        [SerializeField] private List<MushroomBehaviour> mushrooms;

        [Header("Internal Data")]
        [SerializeField] private MushroomShaderData reactiveData;
        [SerializeField, ReadOnly] private MushroomShaderData defaultData;
        [SerializeField, ReadOnly] private MushroomShaderData currentData;
        [SerializeField, ReadOnly] private float percent;
        private bool _canBecomeUnreactive;
        private bool _isReactive;
        private HashSet<Collider> contacts;

        [Header("Settings")]
        [SerializeField, Range(0.1f, 60f)] private float reactiveTimeoutRange;
        [SerializeField] private LayerMask reactiveMask;
        [SerializeField] private float reactiveTransitionSpeed;
        [SerializeField] private float unreactiveTransitionSpeed;
        
        private void Awake()
        {
            contacts = new HashSet<Collider>();
            if (groupName != string.Empty)
            {
                gameObject.name += $" - {groupName}";
            }
            if (mushrooms.IsNullOrEmpty())
            {
                mushrooms = new List<MushroomBehaviour>();
                return;
            }

            int i = -1;
            while (++i < mushrooms.Count)
                mushrooms[i].Initialise();
            
            //! Caching default material/shader data
            MushroomBehaviour mush = mushrooms.FirstOrDefault();
            if (mush == null) return;
            var rendOuter = mush.GetOuterRenderer;
            var rendInner = mush.GetInnerRenderer;
            defaultData = new MushroomShaderData
            {
                BorderColour = rendOuter.sharedMaterial.GetColor(BorderColour),
                Colour = rendInner.sharedMaterial.GetColor(Colour),
                BorderPower = rendOuter.sharedMaterial.GetFloat(BorderPower),
                NoiseScale = rendOuter.sharedMaterial.GetFloat(NoiseScale),
                Alpha = rendOuter.sharedMaterial.GetFloat(Alpha)
            };

            //! Set starting emission settings
            float rate = reactiveData.EmissionRate;
            float breathing = reactiveData.EmissionBreathing;
            i = -1;
            while (++i < mushrooms.Count)
                mushrooms[i].SetMaterialEmissionData(rate, breathing);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!CanCollide(other) || contacts.Contains(other)) return;
            contacts.Add(other);

            if (_isReactive)
                return;
            _canBecomeUnreactive = false;
            StopAllCoroutines();
            StartCoroutine(ReactiveTransition());
        }

        private void OnTriggerExit(Collider other)
        {
            if (!CanCollide(other)) return;
            contacts.Remove(other);
            
            if (contacts.Count == 0)
                _canBecomeUnreactive = true;
        }

        private void UpdateShaderData()
        {
            percent = percent.Clamp01();
            currentData.Lerp(defaultData, reactiveData, percent);

            int count = mushrooms.Count;
            int i = -1;
            while (++i < count)
                mushrooms[i].SetMaterialData(currentData);
        }

        private IEnumerator ReactiveTransition()
        {
            _isReactive = true;

            //! Reactive lerp
            while (percent < 1f)
            {
                percent += DeltaTime * reactiveTransitionSpeed;
                UpdateShaderData();
                yield return null;
            }

            //! Wait for unreactive cue & timer
            WaitForSeconds waitTime = new WaitForSeconds(1f);
            while (!_canBecomeUnreactive)
                yield return waitTime;

            //! Persisting reactivity timeout
            yield return new WaitForSeconds(reactiveTimeoutRange);

            StartCoroutine(UnreactiveTransition());
        }
        private IEnumerator UnreactiveTransition()
        {
            //! Unreactive lerp
            while (percent > 0f)
            {
                percent -= DeltaTime * unreactiveTransitionSpeed;
                UpdateShaderData();
                yield return null;
            }
            
            _isReactive = false;
        }

        private float DeltaTime => Time.deltaTime;
        private bool CanCollide(Collider other) => other.gameObject.layer.IsAMatchingMask(reactiveMask);

        #region Macro Strings
        private readonly string BorderColour = "_BorderColor";
        private readonly string Colour = "_Color";
        private readonly string BorderPower = "_BorderPower";
        private readonly string NoiseScale = "_NoiseScale";
        private readonly string Alpha = "_Alpha";
        #endregion
    }
}
