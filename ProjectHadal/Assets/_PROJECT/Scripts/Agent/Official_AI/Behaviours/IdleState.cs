using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.AStarPathfinding;
using System;
using System.Threading.Tasks;
using System.Linq;
using Hadal.AI.GeneratorGrid;

namespace Hadal.AI.States
{
    public class IdleState : IState
    {
        #region Variables
        AIBrain brain;
        Queue<Vector3> pathQueue;
        Vector3 curDestination;
        Vector3 pathDestination;
        Vector3 prevDest;
        float newDestTimer = 0f;
        float newDestTimeDelay;
        bool isFirstPath;
        bool isFindingPath;

        #endregion

        public IdleState(AIBrain brain, float destinationChangeTimer)
        {
            this.brain = brain;
            pathQueue = new Queue<Vector3>();
            newDestTimeDelay = destinationChangeTimer;
            isFirstPath = false;
            isFindingPath = false;
        }
        public async void OnStateStart()
        {
            CancelPath();
            ResetNewDestinationTimer();
            await SelectRandomPathAsync();
        }
        public async void StateTick()
        {
            await CheckForNewDestinationTimerCompletedAsync();
            WalkPath();
        }
        public void OnStateEnd()
        {
            CancelPath();
            ResetNewDestinationTimer();
            isFindingPath = false;
        }

        void WalkPath()
        {
            if (pathQueue.IsNullOrEmpty()) return;

            if (isFirstPath)
            {
                isFirstPath = false;
                pathDestination = pathQueue.Dequeue();
            }

            //Set the next (sub)destination by dequeueing the nodes
            if (Vector3.Distance(brain.transform.position, pathDestination).IsLessThan(0.1f))
            {
                if (pathQueue.Count == 0)
                {
                    pathQueue.Clear();
                    return;
                }
                pathDestination = pathQueue.Dequeue();
            }

            Vector3 direction = (pathDestination - brain.transform.position).normalized;
            //float multiplier = (Vector3.Distance(curDestination, brain.transform.position)) * brain.idleSpeed * Time.deltaTime; 
            // brain.transform.position = Vector3.Lerp(brain.transform.position, pathDestination, brain.idleSpeed * Time.deltaTime);
            // Vector3 randomThing = Vector3.zero;
            // brain.transform.position = Vector3.SmoothDamp(brain.transform.position, pathDestination, ref randomThing, brain.idleSpeed * Time.deltaTime, brain.idleSpeed);
            // if (pathQueue.Count > 1)
            //     brain.transform.position += direction * (multiplier * Time.deltaTime);
            // else
            //     brain.transform.position = Vector3.Lerp(brain.transform.position, pathDestination, multiplier * Time.deltaTime);
            if (Vector3.Distance(brain.transform.position, pathDestination) > 0)
            {
                brain.transform.position = Vector3.Lerp(brain.transform.position, pathDestination, brain.idleSpeed * Time.deltaTime);
                brain.transform.LookAt(curDestination);
            }

        }

        void CancelPath()
        {
            pathQueue.Clear();
            isFirstPath = false;
        }

        void CheckForNewDestinationTimerCompleted()
        {
            newDestTimer -= Time.deltaTime;
            if (newDestTimer < 0.0f)
            {
                ResetNewDestinationTimer();
                SelectRandomPath();
            }
        }

        async Task CheckForNewDestinationTimerCompletedAsync()
        {
            newDestTimer -= Time.deltaTime;
            if (newDestTimer < 0.0f)
            {
                ResetNewDestinationTimer();
                CancelPath();
                bool pathExists = await SelectRandomPathAsync();
                if (!pathExists) newDestTimer = -1f;
            }
        }

        void SelectRandomPath()
        {
            var list = brain.destinations.Select(i => i.position).ToList();
            list.Remove(prevDest);
            curDestination = list.RandomElement();

            prevDest = curDestination;
            Stack<Node> fullPath = PathFinder.Instance.Find(brain.transform.position, curDestination);
            if (fullPath.IsNullOrEmpty()) return;

            pathQueue.Enqueue(brain.transform.position);
            while (fullPath.IsNotEmpty())
                pathQueue.Enqueue(fullPath.Pop().Position);
            pathQueue.Enqueue(curDestination);

            isFirstPath = true;
        }

        async Task<bool> SelectRandomPathAsync()
        {
            if (isFindingPath) return true;
            isFindingPath = true;

            var list = brain.destinations.Select(i => i.position).ToList();
            list.Remove(prevDest);
            curDestination = list.RandomElement();

            prevDest = curDestination;

            Stack<Node> fullPath = await PathFinder.Instance.FindAsync(brain.transform.position, curDestination);
            bool finish = false;
            if (fullPath.IsNotEmpty())
            {
                pathQueue.Enqueue(brain.transform.position);
                while (fullPath.IsNotEmpty())
                    pathQueue.Enqueue(fullPath.Pop().Position);
                pathQueue.Enqueue(curDestination);

                finish = true;
                isFirstPath = true;
            }
            isFindingPath = false;
            return finish;
        }

        void ResetNewDestinationTimer() => newDestTimer = newDestTimeDelay;

        public Func<bool> ShouldTerminate() => () => false;
    }
}
