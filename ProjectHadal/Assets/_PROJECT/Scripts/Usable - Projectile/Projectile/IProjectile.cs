using UnityEngine;

//Created by Jet
namespace Hadal.Equipment
{
    public interface IProjectile
    {
        ProjectileData Data {get;}
        bool Use(ProjectileHandlerInfo info);
    }
}