using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.Player;
using Tenshi.UnitySoku;
using Tenshi;
using Photon.Pun;
using NaughtyAttributes;
using Hadal.Networking;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System;

namespace Hadal.AI
{
    public class AIDamageManager : MonoBehaviour, ILeviathanComponent
    {
        public AIBrain Brain { get; private set; }
        public void Initialise(AIBrain brain) => Brain = brain;
        public void DoUpdate(in float deltaTime) { }
        public void DoFixedUpdate(in float fixedDeltaTime) { }
        public void DoLateUpdate(in float deltaTime) { }
        public UpdateMode LeviathanUpdateMode => UpdateMode.MainUpdate;

        /// <summary> Damages the chosen player over the network (even the local player gets the event). </summary>
        public void Send_DamagePlayer(PlayerController player, int damage)
        {
            //! raise event with data
            object[] data = { player.ViewID, damage };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            NetworkEventManager.Instance.RaiseEvent(ByteEvents.PLAYER_RECEIVE_DAMAGE, data, options, SendOptions.SendReliable);
        }

        #region Damage over Time Methods

        private Coroutine doTRoutine;

        /// <summary>
        /// The damage function that the AI's thresh attack uses.
        /// </summary>
        public void ApplyDoT(PlayerController player, int durationSeconds, int damagePerSecond, Action dotFinishedEvent)
        {
            if (doTRoutine != null) StopCoroutine(doTRoutine);
            doTRoutine = null;
            doTRoutine = StartCoroutine(TickDamageInSeconds(player, durationSeconds, damagePerSecond, dotFinishedEvent));
        }

        private IEnumerator TickDamageInSeconds(PlayerController player, int durationSeconds, int dps, Action dotFinishedEvent)
        {
            int timer = durationSeconds;
            var waitTime = new WaitForSeconds(1f);
            while (timer > 0)
            {
                timer -= 1;
                Send_DamagePlayer(player, dps);
                yield return waitTime;
            }
            dotFinishedEvent?.Invoke();
        }

        #endregion
    }
}
