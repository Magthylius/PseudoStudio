using Tenshi.SaveHigan;
using UnityEngine;
using Tenshi.UnitySoku;

namespace Hadal
{
    public class RuntimeSaveManager : MonoBehaviour
    {
        [ContextMenu("Delete Root Save Directory")]
        private void ResetRootSaveDirectory()
        {
            SaveManager.DeleteRootSaveDirectory();
        }

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
