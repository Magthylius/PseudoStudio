using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tenshi.AIDolls;
using Tenshi;
using Tenshi.UnitySoku;
using Hadal.AI.AStarPathfinding;
using System;
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
        bool isGridInitialised;
        #endregion

        public IdleState(AIBrain brain, float destinationChangeTimer)
        {
            this.brain = brain;
            pathQueue = new Queue<Vector3>();
            newDestTimeDelay = destinationChangeTimer;
            isFirstPath = false;
            isGridInitialised = false;
            GridGenerator.GridLoadedEvent += OnGridLoaded;
        }
        public void OnStateStart()
        {
           
        }
        public void StateTick()
        {
            ChooseRandomDestination();
            WalkPath();
        }
        public void OnStateEnd()
        {
            
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
            if (Vector3.Distance(brain.transform.position, pathDestination).IsLessThan(0.01f))
            {
                if (pathQueue.Count == 0)
                {
                    pathQueue.Clear();
                    return;
                }
                pathDestination = pathQueue.Dequeue();
            }

            Vector3 direction = (pathDestination - brain.transform.position).normalized;
            float multiplier = (Vector3.Distance(brain.transform.position, curDestination) + 1f);
            brain.transform.position = Vector3.Lerp(brain.transform.position, pathDestination, multiplier * Time.deltaTime);
        }

        void CancelPath()
        {
            pathQueue.Clear();
            isFirstPath = false;
        }

        void ChooseRandomDestination()
        {
            newDestTimer -= Time.deltaTime;
            if (newDestTimer < 0.0f && isGridInitialised)
            {
                newDestTimer = newDestTimeDelay;
                
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
        }

        private void OnGridLoaded(Grid grid)
        {
            GridGenerator.GridLoadedEvent -= OnGridLoaded;
            isGridInitialised = true;
        }

        public Func<bool> ShouldTerminate() => () => false;
    }
}
