using UnityEngine;

namespace Hadal.Player
{
    [CreateAssetMenu(menuName = "Player/Generic Settings Data")]
    public class GenericPlayerSettingsData : ScriptableObject
    {
        public int InventorySize;
        
        public static GenericPlayerSettingsData Get()
            => (GenericPlayerSettingsData) Resources.Load(PathManager.GenericPlayerSettingsDataPath);
    }
}