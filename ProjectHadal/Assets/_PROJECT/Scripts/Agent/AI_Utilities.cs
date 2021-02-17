using UnityEngine;

namespace Nicholas.AI.Utilities
{
    public static class AI_Utilities
    {
        /// <summary>Find if Vector3 isNaN(Not a number)</summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool isNaN(this Vector3 self)
        {
            return float.IsNaN(self.x) || float.IsNaN(self.y) || float.IsNaN(self.z);
        }

        /// <summary>Find if vector3 is infinity</summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static bool isInfinity(this Vector3 self)
        {
            return float.IsInfinity(self.x) || float.IsInfinity(self.y) || float.IsInfinity(self.z);
        }
    }

   
}

