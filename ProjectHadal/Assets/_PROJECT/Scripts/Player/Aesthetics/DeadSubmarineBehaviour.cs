using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class DeadSubmarineBehaviour : MonoBehaviour
    {
        public Light BeaconLight;
        public MeshRenderer EmissiveMaterialMesh;
        public float flashSpeed;
        public Vector2 flashOffsetRange;

        //public float randomOffset
        private float lightIntensity;
        //private Color emissiveMatIntensity;
        private float ticker;
        IEnumerator Start()
        {
            lightIntensity = BeaconLight.intensity;
            //emissiveMatIntensity = EmissiveMaterialMesh.material.color;

            float randomOffset = Random.Range(flashOffsetRange.x, flashOffsetRange.y);
            ticker = randomOffset;
            
            while (true)
            {
                ticker += Time.deltaTime;
                float d = Mathf.Sin(ticker * flashSpeed);
                d = Mathf.Abs(d);
                BeaconLight.intensity = lightIntensity * d;
                //EmissiveMaterialMesh.material.color = emissiveMatIntensity * d;
                
                yield return null;
            }
        }

    }
}
