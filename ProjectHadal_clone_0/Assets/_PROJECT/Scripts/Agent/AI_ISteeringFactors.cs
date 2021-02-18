using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nicholas.AI.VectorExtension;

namespace Nicholas.AI.Agent
{
    public interface AI_ISteeringFactors
    {
        AI_VectorWeight GetVectorWeight();
    }
}


