using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        [SerializeField, Min(0f)] float fpsUpdateCycle = 0.1f;

        void Start()
        {
            if (startEnabled) internalToggler = true;
            else internalToggler = false;

            wholeCanvasParent.SetActive(internalToggler);

            StartCoroutine(FPSCoroutine());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ToggleScreen();
            }
        }

        void ToggleScreen()
        {
            internalToggler = !internalToggler;
            wholeCanvasParent.SetActive(internalToggler);
        }

        void UpdateFPS()
        {
            fpsTMP.text = "FPS: " + (int)(1.0f / Time.deltaTime);
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
