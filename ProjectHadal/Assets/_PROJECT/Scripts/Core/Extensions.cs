using System;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Utility;

//Created by Jet
namespace Hadal
{
    public static class TimerExten
    {
        public static Timer AttachTimer(this MonoBehaviour monoOwner, float duration,
            Action onComplete, Action<float> onUpdate = null, bool loop = false, bool shouldPersist = false)
            => Timer.Register(duration, onComplete, onUpdate, loop, monoOwner, shouldPersist);

        public static Timer AttachTimer(this MonoBehaviour monoOwner, Timer timer)
            => Timer.Register(timer.Duration, timer.OnCompleteEvent, timer.OnUpdateEvent, timer.Loop, monoOwner, timer.ShouldPersist);

        public static TimerBuilder Create_A_Timer(this MonoBehaviour mono) => TimerBuilder.BuildATimer(mono);
    }
    
    public static class VectorExten
    {
        public static Vector3 WithVectorForward(this Quaternion rotation) => rotation * Vector3.forward;
        public static Vector3 WithVectorRight(this Quaternion rotation) => rotation * Vector3.right;
        public static Vector3 WithVectorUp(this Quaternion rotation) => rotation * Vector3.up;
    }

    public static class UnityExten
    {
        public static int MaskToLayer(this LayerMask mask)
        {
            int index = 0;
            int layer = mask.value;
            while(layer > 0)
            {
                layer >>= 1;
                index++;
            }
            return index - 1;
        }
        public static UnityEngine.Object Instantiate0(this UnityEngine.Object prefab)
        {
            return UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }

    public static class UnityBuilderExten
    {
        public static T WithGObjectSetActive<T>(this T obj, bool state) where T : Component
        {
            obj.gameObject.SetActive(state);
            return obj;
        }
    }

    public static class CollectionsExten
    {
        public static bool IsEmpty<T>(this Queue<T> q) => q.Count == 0;
        public static bool IsEmpty<T>(this List<T> l) => l.Count == 0;
    }
}