using Hadal.Utility;
using UnityEngine;

namespace Hadal
{
    public static class Create
    {
        public static TimerBuilder Create_A_Timer(this MonoBehaviour mono) => TimerBuilder.BuildATimer(mono);
    }
}