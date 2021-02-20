using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class HarpoonPool : ProjectilePool<HarpoonBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Harpoon Data");
            base.Start();
        }
    }
}
