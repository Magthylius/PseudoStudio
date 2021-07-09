using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    public class UIShootTracer : MonoBehaviour
    {
        public LineRenderer line;
        public Transform lineStartTransform;
        private Vector3 lineEndPoint;

        private bool isActive = false;

        private void Start()
        {
            //Deactivate();
            Vector3 startPos = lineStartTransform.position;
            line.SetPositions(new [] {startPos, startPos});
        }

        private void FixedUpdate()
        {
            if (!isActive) return;
            line.SetPositions(new [] {lineStartTransform.position, lineEndPoint});
        }

        public void SetStartPoint(Transform startTransform) => lineStartTransform = startTransform;
        public void SetEndPoint(Vector3 endWorldPoint) => lineEndPoint = endWorldPoint;

        public void Activate() => isActive = true;

        public void Deactivate()
        {
            isActive = false;
            //! make it hide itself
            line.SetPosition(1, lineStartTransform.position);
        }

        public bool IsActive => isActive;
    }

}