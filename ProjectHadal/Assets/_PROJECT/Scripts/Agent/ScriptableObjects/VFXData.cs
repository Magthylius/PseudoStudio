using UnityEngine;
using UnityEngine.VFX;

namespace Hadal.AI
{
    [CreateAssetMenu(menuName = "VFX/Data")]
    public class VFXData : ScriptableObject
    {
        [SerializeField] private VisualEffect vfxPrefab;
        
    }
}
