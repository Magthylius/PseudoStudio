using UnityEngine;

namespace Nicholas.AI.VectorExtension
{
    public struct AI_VectorWeight
    {
        public float x, y, z, w;
        public AI_VectorWeight(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public AI_VectorWeight(Vector3 v, float w) : this(v.x, v.y, v.z, w) { }
        public AI_VectorWeight(Vector4 v) : this(v.x, v.y, v.z, v.w) { }

        //!A Static Readonly type variable's value can be assigned at runtime or assigned at compile time and changed at runtime. 
        //!But this variable's value can only be changed in the static constructor. 
        //!And cannot be changed further. It can change only once at runtime.
        public static readonly AI_VectorWeight zero = Vector4.zero;

        public Vector3 vector { get { return (Vector3)this; } }
        public float weight { get { return w; } }
        public Vector3 centroid { get { return weight > 0f ? vector / weight : Vector3.zero; } }

        /// <see cref="https://stackoverflow.com/questions/1176641/explicit-and-implicit-c-sharp"/>
        public static implicit operator AI_VectorWeight(Vector4 v)
        {
            return new AI_VectorWeight(v.x, v.y, v.z, v.w);
        }

        public static implicit operator Vector4(AI_VectorWeight v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }

        public static explicit operator Vector3(AI_VectorWeight v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        //!I'm not very sure about the lhs and rhs as it's referenced, but i believe it's to balance the agent.
        public static AI_VectorWeight operator +(AI_VectorWeight lhs, AI_VectorWeight rhs)
        {
            return new AI_VectorWeight(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
        }
    }
}

