using System;
using UnityEngine;

//Created by Jet
namespace Hadal.Usables
{
    public class UsableLauncherObject : MonoBehaviour, IUsable, IUnityServicer
    {
        [SerializeField] private UsableLauncherData data;
        public virtual UsableLauncherData Data { get => data; set => data = value; }
        
        /// <summary> Event is called when <see cref="Use"/> is called succesfully. Can be used to cue sfx or animations. </summary>
        public event Action<UsableLauncherObject> OnFire;

        /// <summary> Event is called when any ammunition/charges are restored (if any). </summary>
        public event Action<UsableLauncherObject> OnRestock;

        /// <summary> Event is called everytime when the usable object is equipped or dequipped. 
        /// <br/>
        /// The boolean returns true if the usable object has been <strong>equipped</strong>. </summary>
        public event Action<UsableLauncherObject, bool> OnSwitch;
        public bool HasToggleAmmo { get; private set; } = false;
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
        public virtual void ToggleAmmo()
        {
            Data.ToggleProjectile(HasToggleAmmo);
        }

        #endregion

        #region Use Method

        /// <summary> Method that does what the usable launcher object should do. Can be overriden by subclasses for custom behaviour. </summary>
        /// <param name="info">Information that any usable launcher object may need to perform its task.</param>
        public virtual bool Use(UsableHandlerInfo info)
        {
            if (!IsActive) return false;
            OnFire?.Invoke(this);
            LaunchToDestination(info);
            return true;
        }

        /// <summary> Calls <see cref="UsableLauncherObject.Data"/>'s DoEffect. </summary>
        protected virtual void LaunchToDestination(UsableHandlerInfo info)
            => Data.DoEffect(info);

        #endregion

        #region Interface Implementations

        public void OnRestockInvoke() => OnRestock?.Invoke(this);
        public virtual void DoUpdate(in float deltaTime) { }
        public float ElapsedTime => Time.time;
        public float DeltaTime => Time.deltaTime;

        #endregion
    }
}
