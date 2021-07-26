using System.Collections;
using System.Collections.Generic;
using Hadal.Networking;
using Magthylius.Utilities;
using NaughtyAttributes;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.Networking.UI
{
    public class MainMenuHighlightBehaviour : MonoBehaviour
    {
        //public RectTransform iconRect;
        //public float targetScale = 1.2f;
        public Image image;
        public Image IconHighlight;
        public PlayerClassType ClassType;

        [SerializeField, ReadOnly] private bool selectedByOthers;

        public void Select(Color setColor, bool chosenByOthers)
        {
            selectedByOthers = chosenByOthers;
            image.enabled = true;
            image.color = setColor;
            //IconHighlight.color = setColor;

            /*StartCoroutine(Enlarge());

            IEnumerator Enlarge()
            {
                while (Vector3.SqrMagnitude(iconRect.localScale - TargetRectScale) > 0.1f)
                {
                    iconRect.localScale = Vector3.Lerp(iconRect.localScale, TargetRectScale, 5f * Time.deltaTime);
                    Debug.LogWarning("??");
                    yield return new WaitForEndOfFrame();
                }

                iconRect.localScale = TargetRectScale;
            }*/
        }

        public void Deselect()
        {
            image.enabled = false;
            selectedByOthers = false;
            image.color = Color.white;

            /*StartCoroutine(Shrink());
            
            IEnumerator Shrink()
            { 
                while (Vector3.SqrMagnitude(Vector3.one - TargetRectScale) > 0.1f)
                {
                    iconRect.localScale = Vector3.Lerp(iconRect.localScale, Vector3.one, 5f * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }

                iconRect.localScale = Vector3.one;
            }*/
            
        }

        public bool IsSelectedByOthers => selectedByOthers;
        //private Vector3 TargetRectScale => new Vector3(targetScale, targetScale, targetScale);
    }
}
