using UnityEngine;

//Created by Jet, edited Jin
namespace Hadal.Usables.Projectiles
{
    public class SonicGrenadeBehaviour : ProjectileBehaviour
    {
        private NoiseEventTrigger noiseTrigger;
        public SelfDeactivationMode selfDeactivation;
        public delegate void SonicExplodeEvent();
        public event SonicExplodeEvent sonicExploded;

        protected override void Start()
        {
            base.Start();
            noiseTrigger = GetComponent<NoiseEventTrigger>();
        }

        public void SubscribeModeEvent()
        {
            selfDeactivation = GetComponentInChildren<SelfDeactivationMode>();
            selfDeactivation.selfDeactivated += SonicExplode;
        }

        private void SonicExplode()
        {
            noiseTrigger.NoisePing();
        }
    }
}
