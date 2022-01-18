using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Hadal.UI
{
    public class UIHiveDamageHandler : MonoBehaviour
    {
        [SerializeField, ReadOnly] private List<Animator> hives;
        
        private static readonly int Flash = Animator.StringToHash("Flash");
        
        [Button("Get all children as hives")]
        public void GetAllChildrenAsHives()
        {
            hives = GetComponentsInChildren<Animator>().ToList();
        }

        [Button("Random Hive Popup")]
        public void RandomHivePopup()
        {
            int r1 = Random.Range(0, hives.Count);
            int r2 = Random.Range(0, hives.Count);
            int r3 = Random.Range(0, hives.Count);
            int r4 = Random.Range(0, hives.Count);
            int r5 = Random.Range(0, hives.Count);
            
            hives[r1].SetTrigger(Flash);
            hives[r2].SetTrigger(Flash);
            hives[r3].SetTrigger(Flash);
            hives[r4].SetTrigger(Flash);
            hives[r5].SetTrigger(Flash);

            //StartCoroutine(StopHive());
            
            /*IEnumerator StopHive()
            {
                yield return new WaitForSeconds(1f);
                hives[r1].gameObject.SetActive(false);
                hives[r2].gameObject.SetActive(false);
                hives[r3].gameObject.SetActive(false);
                hives[r4].gameObject.SetActive(false);
                hives[r5].gameObject.SetActive(false);
            }*/
        }

        [Button("Clear All")]
        public void ClearAllHive()
        {
            hives.ForEach(o => o.ResetTrigger(Flash));
        }
    }
}
