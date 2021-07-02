using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

namespace Hadal.AI
{
    public class VFXPlayer : MonoBehaviour
    {
        [Header("Visual Effect")]
        [SerializeField] private VisualEffect vfx;
        [SerializeField] private string playEventKeyword;
        
        [Header("Auto Destroy Settings")]
        [SerializeField] private bool shouldDestroy;
        [SerializeField] private int destroyTime;
        
        public void PlayEffect()
        {
            if (vfx == null)
                return;
            
            vfx.SendEvent(playEventKeyword);
            if (shouldDestroy)
                StartCoroutine(StartDestroyTimer());
        }

        private IEnumerator StartDestroyTimer()
        {
            float timer = destroyTime;
            while (true)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    Destroy(gameObject);
                    yield break;
                }
                yield return null;
            }
        }

        public static void DestroyAllVFXOfType(VFXPlayer vfx)
        {
            var list = FindObjectsOfType<VFXPlayer>(true).Where(v => v.playEventKeyword == vfx.playEventKeyword).ToList();
            list.ForEach(v => Destroy(v.gameObject));
        }
    }
}
