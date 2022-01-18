using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

//! C: Jon
namespace Hadal
{
    public class InGameDebugger : MonoBehaviour
    {
        bool internalToggler = false;

        [Header("Overall Display")]
        [SerializeField] bool startEnabled = false;
        [SerializeField] string displayKey = "F2";
        [SerializeField] GameObject wholeCanvasParent;

        [Header("FPS Display")]
        [SerializeField] TextMeshProUGUI fpsTMP;
        [SerializeField] TextMeshProUGUI avgFPSTMP;
        [SerializeField] TextMeshProUGUI lowFPSTMP;
        [SerializeField] TextMeshProUGUI highFPSTMP;
        [SerializeField, Min(0f)] float fpsUpdateCycle = 0.1f;

        private float totalFPS = 0f;
        private float totalFPSDiv = 0f;
        private float avgFPS = 0f;
        private float lowFPS = float.MaxValue; 
        private float highFPS = 0f;

        void Start()
        {
            if (startEnabled) internalToggler = true;
            else internalToggler = false;

            wholeCanvasParent.SetActive(internalToggler);

            StartCoroutine(FPSCoroutine());
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleScreen();
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        { 
            totalFPS = 0f;
            avgFPS = 0f;
            lowFPS = float.MaxValue;
            highFPS = 0f;
        }

        void ToggleScreen()
        {
            internalToggler = !internalToggler;
            wholeCanvasParent.SetActive(internalToggler);
        }

        void UpdateFPS()
        {
            float fps = 1.0f / Time.deltaTime;
            totalFPS += fps;
            totalFPSDiv++;
            if (fps > highFPS) highFPS = fps;
            if (fps < lowFPS) lowFPS = fps;
            avgFPS = totalFPS / totalFPSDiv;
            
            fpsTMP.text = "FPS: " + (int)fps;
            avgFPSTMP.text = "AVG: " + (int)avgFPS;
            lowFPSTMP.text = "LOW: " + (int)lowFPS;
            highFPSTMP.text = "HI: " + (int)highFPS;
        }

        IEnumerator FPSCoroutine()
        {
            while (true)
            {
                UpdateFPS();
                yield return new WaitForSeconds(fpsUpdateCycle);
            }
        }
    }
}
