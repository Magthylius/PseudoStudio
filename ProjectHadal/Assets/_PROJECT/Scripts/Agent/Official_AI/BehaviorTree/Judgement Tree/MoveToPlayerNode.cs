using Hadal.AI.Caverns;
using UnityEngine;
using Tenshi;
using Tenshi.UnitySoku;

namespace Hadal.AI.TreeNodes
{
    public class MoveToPlayerNode : BTNode
    {
        private AIBrain _brain;
        private PointNavigationHandler _navigator;
        private Transform _previousTarget;
        private Transform _target;
        private Transform _pilot;
        private NavPoint _pointPrefab;
        private float _closeDistanceThreshold;
        private float _farDistanceThreshold;
        private bool _followPersistently;

        public MoveToPlayerNode(AIBrain brain, NavPoint prefab, float closeDistanceThreshold, float farDistanceThreshold, bool followPersistently)
        {
            _brain = brain;
            _navigator = _brain.NavigationHandler;
            _pilot = _navigator.PilotTransform;
            _pointPrefab = prefab;
            _closeDistanceThreshold = closeDistanceThreshold;
            _farDistanceThreshold = farDistanceThreshold;
            _followPersistently = followPersistently;
            _previousTarget = null;
        }

        public override NodeState Evaluate(float deltaTime)
        {
            Debugger();
            if (_brain.CurrentTarget == null)
            {
                //return NodeState.FAILURE;
            }

            Debug.Log("My Target:" + _brain.CurrentTarget);
            if (_target != _brain.CurrentTarget.transform)
            {
                _previousTarget = _target;
                _target = _brain.CurrentTarget.transform;
                SetNavPoint(_target);
            }

            // Move();
            if (CloseThresholdReached()) return NodeState.SUCCESS;
            if (FarThresholdReached() && !_followPersistently) return NodeState.FAILURE;

            return NodeState.RUNNING;
        }

        private void Move()
        {
            NavPoint point = _target.GetComponentInChildren<NavPoint>();
            if (point == null)
            {
                point = Object.Instantiate(_pointPrefab, _target.position, Quaternion.identity);
                point.AttachTo(_target);
                _navigator.SetCustomPath(point, true);
            }
        }

        private void SetNavPoint(Transform target)
        {
            NavPoint point = target.GetComponentInChildren<NavPoint>();
            if (point == null)
            {
                point = Object.Instantiate(_pointPrefab, target.position, Quaternion.identity);
                point.AttachTo(target);
                point.SetCavernTag(CavernTag.Custom_Point);
                _navigator.SetCustomPath(point, true);

            }
        }

        private bool CloseThresholdReached()
            => (_pilot.position - _target.position).sqrMagnitude < _closeDistanceThreshold * _closeDistanceThreshold;

        private bool FarThresholdReached()
            => (_pilot.position - _target.position).sqrMagnitude > _farDistanceThreshold * _farDistanceThreshold;

        public MoveToPlayerNode WithDebugName(string msg)
        {
            debugName = msg.AddSpacesBeforeCapitalLetters(false) + "Node";
            return this;
        }

        private void Debugger()
        {
            if (EnableDebug)
                $"Name: {debugName}, Target: {_brain.CurrentTarget}".Msg();
        }
    }
}
