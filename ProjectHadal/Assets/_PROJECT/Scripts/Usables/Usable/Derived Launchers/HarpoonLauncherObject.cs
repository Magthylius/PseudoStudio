//Create by Jet
using UnityEngine;

namespace Hadal.Usables
{
    public class HarpoonLauncherObject : UsableLauncherObject
    {
        [SerializeField] int poweredUpReserveCapacity;
		[SerializeField] float poweredUpChamberReloadTime;
		[SerializeField] float poweredUpReserveRegenTime;
		
        public override void PowerUp()
        {
            maxReserveCapacity = poweredUpReserveCapacity;
			
			//! Jet's test addition
			ChangeChamberReloadTime(poweredUpChamberReloadTime);
			ChangeReserveRegenTime(poweredUpReserveRegenTime);
			
            SetDefaults();
            return;
        }
    }
}
