using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! C: Jon
namespace Hadal.AI.Caverns
{
    /// <summary>
    /// Used to handle ambush point logics. Preferably use a sphere collider.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class AmbushPointBehaviour : MonoBehaviour
    {
        new Collider collider;

        void OnValidate()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        void OnEnable()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        void Start()
        {
        
        }

        
        void Update()
        {
        
        }
    }
}
