using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.AI.States;
using Tenshi;
using Hadal.Networking;
using ExitGames.Client.Photon;

namespace Hadal.AI
{
    public class AIEgg : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] int maxHealth = 40;
        [SerializeField, ReadOnly] int curHealth;

        [Header("VFX")]
        [SerializeField, ReadOnly] private Transform[] randomHitPoints;
        [SerializeField] private VFXData vfx_OnDamaged;
        [SerializeField] private int vfxCountPerHit = 2;
        [SerializeField] private int vfxCountPerDeath = 8;

        [Header("Graphics")]
        [SerializeField] private MeshRenderer[] mRenderers;

        public GameObject Obj => gameObject;

        public delegate void MaxConfidenceOnEggDestroyed(bool isEggDestroyed);
        public event MaxConfidenceOnEggDestroyed eggDestroyedEvent;

        void Awake()
        {
            curHealth = maxHealth;
        }

        private void Start()
        {
            NetworkEventManager.Instance.AddListener(ByteEvents.AI_EGG_DAMAGED, Receive_EggDamaged);
        }

        private void OnDestroy()
        {
            NetworkEventManager.Instance.RemoveListener(ByteEvents.AI_EGG_DAMAGED, Receive_EggDamaged);
        }

        void CheckEggDestroyed()
        {
            if (curHealth <= 0)
            {
                int i = -1;
                if (randomHitPoints.IsNotEmpty())
                {
                    while (++i < vfxCountPerDeath)
                        PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
                }

                i = -1;
                while (++i < mRenderers.Length)
                    mRenderers[i].enabled = false;
                
                GetComponent<Collider>().enabled = false;
                eggDestroyedEvent?.Invoke(true);
            }
        }

        void DoOnHitEffects(int damage)
        {
            if (randomHitPoints.IsNotEmpty() && curHealth > 0)
            {
                int i = -1;
                while (++i < vfxCountPerHit)
                    PlayVFXAt(vfx_OnDamaged, GetRandomHitPosition());
            }
        }

        public bool TakeDamage(int damage)
        {
            if (curHealth <= 0) return false;

            damage = damage.Abs();
            if (NetworkEventManager.Instance.IsMasterClient) //! Only do actual damage on master client
            {
                curHealth -= damage;
                CheckEggDestroyed();
                DoOnHitEffects(damage);
                Send_EggDamaged(damage, false);
                return true;
            }

            //! Send damage to master client
            Send_EggDamaged(damage, true);
            return true;
        }

        private void Send_EggDamaged(int damage, bool sendToMasterClient)
        {
            object[] content;
            if (sendToMasterClient)
            {
                content = new object[]
                {
                    damage,
                    sendToMasterClient
                };
            }
            else //! send to non-master clients
            {
                content = new object[]
                {
                    damage,
                    sendToMasterClient,
                    curHealth <= 0
                };
            }
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.AI_EGG_DAMAGED, content, SendOptions.SendReliable);
        }
        private void Receive_EggDamaged(EventData eventData)
        {
            object[] content = (object[])eventData.CustomData;

            int damage = (int)content[0];
            bool sendToMasterClient = (bool)content[1];
            if (sendToMasterClient) //! evaluate on master client
            {
                curHealth -= damage;
                CheckEggDestroyed();
                DoOnHitEffects(damage);
                Send_EggDamaged(damage, false);
            }
            else //! evaluate on non-master client
            {
                bool isDead = (bool)content[2];
                if (isDead)
                {
                    curHealth = 0;
                    CheckEggDestroyed();
                }
                else
                    DoOnHitEffects(damage);
            }
        }

        private void PlayVFXAt(VFXData vfx, Vector3 position)
        {
            if (vfx == null) return;
            vfx.SpawnAt(position);
        }

        private Vector3 GetRandomHitPosition() => randomHitPoints.RandomElement().position;
    }
}
