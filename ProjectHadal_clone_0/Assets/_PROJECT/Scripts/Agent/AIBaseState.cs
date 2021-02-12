using UnityEngine;
using System;
using UnityEngine.AI;

namespace Hadal.AI
{
    public abstract class AIBaseState
    {
        protected GameObject gameObject;
        protected Transform transform;
        protected NavMeshAgent agent;

        public abstract Type Tick();
        public abstract Type Start();

        public AIBaseState(GameObject GameObject)
        {
            this.gameObject = GameObject;
            this.transform = GameObject.transform;
            this.agent = GameObject.GetComponent<NavMeshAgent>();
        }


    }
}