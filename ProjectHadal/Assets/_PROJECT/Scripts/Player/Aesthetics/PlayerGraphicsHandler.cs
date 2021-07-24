using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Hadal.Player
{
    public class PlayerGraphicsHandler : MonoBehaviour
    {
        [SerializeField] private MeshRenderer emissiveRenderer;
        [SerializeField] private string emissiveColorString;
        public GameObject GraphicsObject => gameObject;

        public void ChangeEmissiveColor(Color newColor)
        {
            if (emissiveRenderer.material.HasProperty(emissiveColorString))
                emissiveRenderer.material.SetColor(emissiveColorString, newColor);
        }

        [Button("TestColorRed")]
        void Debug_ToColoRed()
        {
            ChangeEmissiveColor(Color.red);
        }
    }
}
