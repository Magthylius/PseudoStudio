using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.Player
{
    public class SubmarineGraphicsSmoothener : MonoBehaviour
    {
        public Transform Following;
        public Transform Follower;
        
        public float MoveLerpSpeed = 10f;
        public bool AllowUpdate = true;

        private Vector3 followerPosition;

        private void Start()
        {
            followerPosition = Follower.position;
        }

        void Update()
        {
            if (!AllowUpdate) return;

            followerPosition = Vector3.Lerp(followerPosition, Following.position, Time.deltaTime * MoveLerpSpeed);
            Follower.position = followerPosition;
        }
    }
}
