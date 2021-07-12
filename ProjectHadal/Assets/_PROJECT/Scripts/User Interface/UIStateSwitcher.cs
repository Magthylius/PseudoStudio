using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hadal.UI
{
    /// <summary>
    /// GameObject active handling and make sures at most only one active per given time.
    /// </summary>
    public class UIStateSwitcher : MonoBehaviour
    {
        public bool enableFirstIfNoActives = true;
        public List<GameObject> gameObjectStates;

        private GameObject currentActiveObject;
        private int currentState = 0;

        private void Start()
        {
            if (!HasValidStates) return;

            foreach (GameObject obj in gameObjectStates)
            {
                if (currentActiveObject == null && obj.activeInHierarchy) currentActiveObject = obj;
                else obj.SetActive(false);
            }

            if (currentActiveObject == null && enableFirstIfNoActives)
            {
                currentActiveObject = gameObjectStates[0];
                currentActiveObject.SetActive(true);
            }
        }

        public void NextState()
        {
            if (!HasValidStates) return;
            
            currentActiveObject.SetActive(false);
            currentState++;
            if (currentState >= gameObjectStates.Count) currentState = 0;
            currentActiveObject = gameObjectStates[currentState];
            currentActiveObject.SetActive(true);
        }
        
        public void PreviousState()
        {
            if (!HasValidStates) return;
            
            currentActiveObject.SetActive(false);
            currentState--;
            if (currentState < 0) currentState = gameObjectStates.Count - 1;
            currentActiveObject = gameObjectStates[currentState];
            currentActiveObject.SetActive(true);
        }

        public void SetCurrentStage(int index)
        {
            if (index >= gameObjectStates.Count || index < 0)
            {
                Debug.LogWarning("Stats out of scope!");
                return;
            }

            currentState = index;
            currentActiveObject.SetActive(false);
            currentActiveObject = gameObjectStates[index];
            currentActiveObject.SetActive(true);
        }

        public bool HasValidStates => gameObjectStates != null;
        public GameObject CurrentActiveObject => currentActiveObject;
    }
}
