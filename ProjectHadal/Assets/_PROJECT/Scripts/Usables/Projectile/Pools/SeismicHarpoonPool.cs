using UnityEngine;

//Created by Jet
namespace Hadal.Usables.Projectiles
{
    public class SeismicHarpoonPool : ProjectilePool<SeismicHarpoonBehaviour>
    {
        protected override void Start()
        {
            data = (ProjectileData) Resources.Load($"{PathManager.ProjectileDataPath}/Seismic Harpoon Data");
            base.Start();
        }
    }
}
