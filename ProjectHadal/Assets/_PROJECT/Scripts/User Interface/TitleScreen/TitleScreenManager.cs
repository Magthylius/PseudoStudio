using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hadal.UI
{
    public class TitleScreenManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audio;
        [SerializeField] private float audioDelay;
        [SerializeField] private float animationDelay;
        [SerializeField][Scene] private string mainMenuScene;
        
        void Start()
        {
            StartCoroutine(AudioDelay());
            StartCoroutine(LoadDelay());
        }

        IEnumerator AudioDelay()
        {
            yield return new WaitForSeconds(audioDelay);
            audio.Play();
        }

        IEnumerator LoadDelay()
        {
            yield return new WaitForSeconds(animationDelay);
            SceneManager.LoadSceneAsync(mainMenuScene);
        }
        
    }
}
