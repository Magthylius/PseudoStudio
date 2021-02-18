using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Nicholas.AI.Utilities;
using Nicholas.AI.VectorExtension;

namespace Nicholas.AI.Agent
{
    [RequireComponent(typeof(Rigidbody))]
    //!This is an abstract class to be implemented by AnimalAgent.
    public abstract class AI_BaseAgent : MonoBehaviour
    {
        [SerializeField] protected Rigidbody m_Rigidbody;
        AI_BaseAgent m_ActiveAgent;
        protected List<AI_ISteeringFactors> m_SteeringFactors = new List<AI_ISteeringFactors>();

        /// <summary>To reset the AnimalAgent on disable.</summary>
        protected virtual void Reset()
        {
            if (m_Rigidbody == null)
                m_Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>Prevent value to be less than 0.0f</summary>
        protected virtual void OnValidate()
        {
            if (m_Rigidbody == null)
                m_Rigidbody = GetComponent<Rigidbody>();
        }

        private void Awake()
        {
            OnAgentInit();
        }

        private void FixedUpdate()
        {
            OnAgentUpdate(Time.deltaTime);
        }

        protected virtual void OnEnable()
        {
            m_ActiveAgent.enabled = true;
        }

        protected virtual void OnDisable()
        {
            m_ActiveAgent.enabled = false;
        }

        /// <summary>Registers steering force on awake</summary>
        protected abstract void OnAgentInit();
        protected abstract void AgentReset();

        /// <summary>Updates all factors within fixedUpdate</summary>
        /// <param name="fixedDeltaTime"></param>
        protected virtual void OnAgentUpdate(float fixedDeltaTime)
        {
            Vector3 desiredVector = CalculateSteeringFactors();
            if(!desiredVector.isNaN())
            {
                ToDestination(fixedDeltaTime, desiredVector);
            }
        }

        /// <summary>Calculate final destination based on registered factors.</summary>
        /// <returns>A Vector3 pointed towards the final destination.</returns>
        private Vector3 CalculateSteeringFactors()
        {
            AI_VectorWeight steeringForceWeight = AI_VectorWeight.zero;
            int count = m_SteeringFactors.Count;
            for (int i = 0; i < count; i++)
            {
                steeringForceWeight += m_SteeringFactors[i].GetVectorWeight();
            }

            return steeringForceWeight.centroid;
        }

        protected abstract void ToDestination(float fixedDeltaTime, Vector3 desiredVector);


        ///<summary>Update destination, triggering the calculation for new path.///</summary>
        ///<param name="target">The target to navigate towards to.</param>
        ///<returns>bool True if destination requested is successful, else false.</returns>
        public abstract bool SetDestination(Vector3 target);
        protected static readonly Vector3 Vector3Zero = Vector3.zero;
        protected static readonly Vector3 Vector3Forward = Vector3.forward;
        protected static readonly Vector3 Vector3Up = Vector3.up;
        protected static readonly Vector3 Vector3Up45 = Quaternion.AngleAxis(-45f, Vector3.right) * Vector3.forward;
        protected static readonly Vector3 Vector3Down45 = Quaternion.AngleAxis(45f, Vector3.right) * Vector3.forward;
        protected static readonly Vector3 Vector3Right45 = Quaternion.AngleAxis(45f, Vector3.up) * Vector3.forward;
        protected static readonly Vector3 Vector3Left45 = Quaternion.AngleAxis(-45f, Vector3.up) * Vector3.forward;


    }
}

