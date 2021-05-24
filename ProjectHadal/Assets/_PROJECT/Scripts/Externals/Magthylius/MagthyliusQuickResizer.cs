using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magthylius.Utilities
{
    public class MagthyliusQuickResizer : MonoBehaviour
    {
        [SerializeField] Transform explicitTransform;
        [SerializeField] Vector3 ratio = new Vector3(1, 1, 1);
        [SerializeField] float scale;
        [SerializeField] bool RefreshOnValidate = true;

        void OnValidate()
        {
            if (RefreshOnValidate) UpdateTransform();
        }

        void UpdateTransform()
        {
            if (explicitTransform == null)
            {
                explicitTransform = transform;

                if (explicitTransform == null)
                {
                    Debug.LogError("There is no transform to resize!");
                    return;
                }
            }

            explicitTransform.localScale = ratio * scale;
        }
    }
}
