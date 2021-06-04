using UnityEngine;

namespace Hadal.AI.TreeNodes
{
    public class MoveToPlayerNode : BTNode
    {
        private AIBrain _brain;
        private PointNavigationHandler _navigator;
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
        }

        public override NodeState Evaluate()
        {
            _target = _brain.CurrentTarget.transform;
            if (_target == null) return NodeState.FAILURE;

            MoveIfNotMoving();
            if (CloseThresholdReached()) return NodeState.SUCCESS;
            if (FarThresholdReached() && !_followPersistently) return NodeState.FAILURE;

            return NodeState.RUNNING;
        }

        private void MoveIfNotMoving()
        {
            NavPoint point = _target.GetComponentInChildren<NavPoint>();
            if (point == null)
            {
                point = Object.Instantiate(_pointPrefab, _target.position, Quaternion.identity);
                point.AttachTo(_target);
                _navigator.SetCustomPath(point, true);
            }
        }

        private bool CloseThresholdReached()
            => (_pilot.position - _target.position).sqrMagnitude < _closeDistanceThreshold * _closeDistanceThreshold;

        private bool FarThresholdReached()
            => (_pilot.position - _target.position).sqrMagnitude > _farDistanceThreshold * _farDistanceThreshold;
    }
}
