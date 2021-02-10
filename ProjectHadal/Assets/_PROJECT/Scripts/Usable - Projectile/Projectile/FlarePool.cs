using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public class FlarePool : ProjectilePool
    {
        protected override void Awake()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Flare Data");
            base.Awake();
        }
    }
}