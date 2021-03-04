using Tenshi.SaveHigan;
using UnityEngine;

namespace Hadal
{
    public class RuntimeSaveManager : MonoBehaviour
    {
        private void Start()
        {
            SaveManager.OnLoadGameInvoke();
        }

        private void OnDestroy()
        {
            SaveManager.OnSaveGameInvoke();
        }
    }
}
