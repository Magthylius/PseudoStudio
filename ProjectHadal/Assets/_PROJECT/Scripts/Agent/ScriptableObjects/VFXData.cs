using UnityEngine;
using UnityEngine.VFX;

namespace Hadal.AI
{
    [CreateAssetMenu(menuName = "VFX/Data")]
    public class VFXData : ScriptableObject
    {
        [SerializeField] private VFXPlayer vfxPrefab;

        public void SpawnAt(Vector3 position)
        {
            var vfx = Instantiate(vfxPrefab, position, Quaternion.identity);
            vfx.PlayEffect();
        }
    }
}
