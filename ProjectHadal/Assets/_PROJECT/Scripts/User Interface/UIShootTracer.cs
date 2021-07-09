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

        private void Start()
        {
            Deactivate();
        }

        private void FixedUpdate()
        {
            if (!IsActive) return;
            line.SetPositions(new [] {lineStartTransform.position, lineEndPoint});
            print(lineEndPoint);
        }

        public void SetStartPoint(Transform startTransform) => lineStartTransform = startTransform;
        public void SetEndPoint(Vector3 endWorldPoint) => lineEndPoint = endWorldPoint;

        public void Activate() => line.gameObject.SetActive(true);
        public void Deactivate() => line.gameObject.SetActive(false);

        public bool IsActive => line.gameObject.activeInHierarchy;
    }

}