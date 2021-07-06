using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.AudioSystem
{
    public class AudioSourceHandler : MonoBehaviour
    {
        private AudioSource source;
        private void OnEnable()
        {
            if (!source) source = GetComponent<AudioSource>();
        }

        public void Setup()
        {
            //! Need to setup from scriptable
        }
    }
}
