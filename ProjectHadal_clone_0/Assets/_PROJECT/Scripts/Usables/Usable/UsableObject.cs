using System;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    public class UsableObject : MonoBehaviour, IUsable, IUnityServicer
    {
        [SerializeField] private UsableData data;
        public virtual UsableData Data { get => data; set => data = value; }
        public event Action<UsableObject> OnFire;
        public event Action<UsableObject> OnRestock;
        public event Action<UsableObject, bool> OnSwitch;
        protected Camera PCamera { get; set; } = null;
        protected bool IsActive { get; set; } = false;

        #region Unity Lifecycle

        protected virtual void Awake() { }
        private void Update() => DoUpdate(DeltaTime);

        #endregion

        #region Command Methods

        public void Inject(Camera camera) => PCamera = camera;
        public void Activate()
        {
            IsActive = true;
            OnSwitch?.Invoke(this, IsActive);
        }
        public void Deactivate()
        {
            IsActive = false;
            OnSwitch?.Invoke(this, IsActive);
        }

        #endregion

        #region Use Method

        public virtual bool Use(UsableHandlerInfo info)
        {
            if (!IsActive) return false;
            OnFire?.Invoke(this);
            LaunchToDestination(info);
            return true;
        }
        protected virtual void LaunchToDestination(UsableHandlerInfo info)
            => Data.DoEffect(info.WithCamera(PCamera));

        #endregion

        public void OnRestockInvoke() => OnRestock?.Invoke(this);
        public virtual void DoUpdate(in float deltaTime) { }
        public float ElapsedTime => Time.time;
        public float DeltaTime => Time.deltaTime;
    }
}
