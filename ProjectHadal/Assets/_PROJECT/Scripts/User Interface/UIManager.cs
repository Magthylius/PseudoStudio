using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hadal.Inputs;
using Magthylius.LerpFunctions;
using Hadal.Networking;
using Hadal.Locomotion;

//Created by Jet, Edited by Jon 
namespace Hadal.UI
{
    public delegate void OnHealthChange();
    public delegate void OnPauseMenuAction();

    public enum TrackerType
    {
        PLAYER_NAME = -1,
        SONIC_DART = 0,
    }

    public class UIManager : MonoBehaviourDebug
    {
        public static UIManager Instance; 

        public string debugKey;

        NetworkEventManager neManager;

        [Header("External References")] 
        public UIShootTracer ShootTracer;
        public UIScreenDataHandler ScreenDataHandler;
        public UIContextHandler ContextHandler;
        public UIEffectsHandler EffectsHandler;
        public UIHydrophoneBehaviour HydrophoneBehaviour;
        public UICockpitCamera CockpitCamera;
        public UIClassInfoHandler ClassInfoHandler;
        public Camera PlayerCamera;

        [Header("Reticle Settings")]
        [SerializeField] RectTransform reticleGroup;
        [SerializeField] RectTransform reticleDirectors;
        //[SerializeField] MagthyliusUILineRenderer reticleLineRenderer;
        [SerializeField] float maxDirectorRadius = 10f;
        [SerializeField] float directorReactionSpeed = 5f;
        [SerializeField] float directorInputCamp = 5f;

        [Header("Reticle Line Settings")]
        [SerializeField] Image reticleLineImage;
        [SerializeField] float minPixelsPerUnit;
        [SerializeField] float maxPixelsPerUnit;

        [Header("Reticle Rotation Settings")]
        [SerializeField, Min(0f)] float maxReticleRotation = 20f;

        FlexibleRect reticleDirectorsFR;

        [Header("Player Settings")]
        [SerializeField] RectTransform allUIParent;
        [SerializeField, Min(0f)] float uiDisplacement;
        [SerializeField, Min(0.1f)] float maxMovementInfluence;
        [SerializeField, Min(0.1f)] float uiLerpReactionSpeed;

        public static event OnHealthChange OnHealthChange;

        FlexibleRect allUIParentFR;

        Rotator playerRotator;
        IRotationInput playerRotationInput;
        Transform playerTransform;
        Rigidbody playerRigidbody;

        private Transform aiTransform;

        [Header("Torpedo Settings")] 
        public GameObject torpedoFillerPrefab;
        public Transform torpedoFillerParent;
        public GameObject torpedoEmptyText;

        public int torpCount;
        public Image torpLoader;
        public List<Image> reloadProgressors;
        public GameObject floodText;
        public GameObject reloadText;
        
        List<UIFillerBehaviour> torpedoFillers;
        private bool torpIsEmpty = false;

        [Header("Harpoon Settings")]
        public GameObject harpoonFillerPrefab;
        public Transform harpoonFillerParent;
        public Image harpLoader;

        private List<UIFillerBehaviour> harpoonFillers;
        
        
        [Header("VFX Settings")]
        public ParticleSystem torpedoReloadedVFX;
        public ParticleSystem torpedoEmptyVFX;

        [Header("Module Settings")]
        [SerializeField] UITrackerHandler trackerHandler;

        [Header("Utilities Settings")]
        [SerializeField] UIUtilitiesHandler utilitiesHandler;

        [Header("Pause Menu Settings")]
        [SerializeField] Menu pauseMenu;
        StandardUseableInput playerInput;
        public event OnPauseMenuAction PauseMenuOpened;
        public event OnPauseMenuAction PauseMenuClosed;

        bool pauseMenuOpen = false;
        //! blur out the screen

        //! Debug
        int sl_UI;

        #region Unity Lifecycle
        void Start()
        {
            neManager = NetworkEventManager.Instance;

            reticleDirectorsFR = new FlexibleRect(reticleDirectors);
            allUIParentFR = new FlexibleRect(allUIParent);
            

            DoDebugEnabling(debugKey);

            SetupReticle();
            SetupPauseMenu();
            PNTR_Resume();

            //Initialize(3);
            //sl_UI = DebugManager.Instance.CreateScreenLogger();
            //Debug.LogWarning("w: " + Screen.width + " | h: " + Screen.height);
            //Debug.LogWarning(Screen.currentResolution);
            //Debug.LogWarning("screen scale: " + (Screen.width / Screen.currentResolution.width));

        }

        void Update()
        {
            if (playerTransform == null) return;

#if UNITY_EDITOR
            if (playerInput.TabKeyDown) TriggerPauseMenu();
#else
            if (playerInput.EscKeyDown) TriggerPauseMenu();
#endif
        }

        void FixedUpdate()
        {
            if (playerTransform == null) return;

            UpdateReticle();
            UpdateUIDisplacement();
        }
        
        #endregion

        #region External calls

        public void Initialize(int totalTorpedoCount, int totalHarpoonCount)
        {
            torpedoFillers = new List<UIFillerBehaviour>();
            harpoonFillers = new List<UIFillerBehaviour>();
            //StartCoroutine(SpawnFillers(totalTorpedoCount - 1, true));
            
            torpedoEmptyText.SetActive(false);
            
            //! -1 because 1 is represented through the filler
            for (int i = 0; i < totalTorpedoCount - 1; i++)
            {
                GameObject go = Instantiate(torpedoFillerPrefab, torpedoFillerParent);
                UIFillerBehaviour filler = go.GetComponent<UIFillerBehaviour>();
                torpedoFillers.Add(filler);

                filler.ToFilled();
            }
            
            torpedoFillers.Reverse();

            for (int i = 0; i < totalHarpoonCount - 1; i++)
            {
                GameObject go = Instantiate(harpoonFillerPrefab, harpoonFillerParent);
                UIFillerBehaviour filler = go.GetComponent<UIFillerBehaviour>();
                harpoonFillers.Add(filler);
                
                filler.ToFilled();
            }
        }
        
        IEnumerator SpawnFillers(int totalTorpedoCount, bool startFilled)
        {
            for (int i = 0; i < totalTorpedoCount; i++)
            {
                GameObject go = Instantiate(torpedoFillerPrefab, torpedoFillerParent);
                UIFillerBehaviour filler = go.GetComponent<UIFillerBehaviour>();
                torpedoFillers.Add(filler);
                
                if (startFilled) filler.ToFilled();
                else filler.ToHollow();
                
                //! We can delay here to make an effect
                //yield return new WaitForEndOfFrame();
            }
            
            torpedoFillers.Reverse();
            yield return new WaitForEndOfFrame();
        }
        
        public void Activate()
        {
            gameObject.SetActive(true);
            if (IsNull) Instance = this;
        }
        #endregion

        #region Health

        public void InvokeOnHealthChange(int currentHealth)
        {
            OnHealthChange?.Invoke();
            UpdateHealthUI(currentHealth);
        }
        void UpdateHealthUI(int currentHealth)
        {
            ScreenDataHandler.UpdateTargetHealth(currentHealth);
        }
        #endregion

        #region Torpedoes
        public void UpdateTorpedoChamber(float progress, bool showFlooding)
        {
            torpLoader.fillAmount = progress;
            //floodText.SetActive(showFlooding);
            if (showFlooding) ShootTracer.ToBlue();
            else ShootTracer.ToRed();
        }

        public void UpdateTorpedoReserve(int torpedoCount)
        {
            torpCount = torpedoCount;

            //! TorpedoCount is -1 due to loader being represented thru other code
            int count = 0;
            foreach (UIFillerBehaviour filler in torpedoFillers)
            {
                if (count < torpCount - 1) filler.ToFilled();
                else filler.ToHollow();
                count++;
            }

            //print(torpedoCount);
            torpIsEmpty = torpCount == 0;
            torpedoEmptyText.SetActive(torpIsEmpty);

            if (torpIsEmpty)
            {
                ShootTracer.ToOrange();
            }
        }

        public void UpdateFiringVFX(bool emptyChamber)
        {
            if (emptyChamber) torpedoEmptyVFX.Emit(1);
        }

        public void UpdateReload(float progress, bool showReloading)
        {           
            reloadText.SetActive(showReloading);
            foreach (Image progressors in reloadProgressors)
            {
                progressors.fillAmount = progress;
            }
        }
        #endregion

        #region Harpoons

        public void UpdateHarpoonChamber(float progress)
        {
            harpLoader.fillAmount = progress;
        }

        public void UpdateHarpoonReserve(int harpoonCount)
        {
            int count = 0;
            foreach (UIFillerBehaviour filler in harpoonFillers)
            {
                if (count < harpoonCount - 1) filler.ToFilled();
                else filler.ToHollow();
                count++;
            }
        }

        #endregion

        #region Modules
        public void InjectPlayer(Transform Transform, Rotator Rotator, IRotationInput RotationInput)
        {
            playerTransform = Transform;
            playerRotator = Rotator;
            playerRotationInput = RotationInput;

            playerRigidbody = playerTransform.GetComponent<Rigidbody>();
            
            ShootTracer.InjectDependencies(PlayerCamera);
            ScreenDataHandler.InjectDependencies(this, playerTransform);
            EffectsHandler.InjectDependencies(playerRigidbody, ShootTracer);
            HydrophoneBehaviour.InjectPlayerDependencies(playerTransform);
            CockpitCamera.InjectDependencies(playerRotationInput);
        }

        public void InjectAIDependencies(Transform AITransform)
        {
            aiTransform = AITransform;
            HydrophoneBehaviour.InjectAIDependencies(aiTransform);
        }

        void UpdateUIDisplacement()
        {
            Vector2 velocity = playerTransform.InverseTransformDirection(playerRigidbody.velocity);
            velocity = -velocity / maxMovementInfluence * uiDisplacement;
            allUIParentFR.StartLerp(velocity);
            allUIParentFR.Step(uiLerpReactionSpeed * Time.deltaTime);
        }
        
        public void TrackPlayerName(Transform otherPlayer, string playerName)
        {
            print("tracking: " + playerName);
            StartCoroutine(TryTracking());

            IEnumerator TryTracking()
            {
                while (!trackerHandler.Initialized)
                {
                    //nameTracker = trackerHandler.Scoop(TrackerType.PLAYER_NAME);
                    yield return new WaitForEndOfFrame();
                }
                UITrackerBehaviour nameTracker = trackerHandler.Scoop(TrackerType.PLAYER_NAME);
                nameTracker.TrackTransform(otherPlayer);
                PlayerNameTrackerBehaviour pNameTracker = nameTracker as PlayerNameTrackerBehaviour;
                if (pNameTracker) pNameTracker.UpdateText(playerName);
            }
        }
        
        public void TrackProjectile(Transform projectileTransform, TrackerType projectileType)
        {
            UITrackerBehaviour tracker = trackerHandler.Scoop(projectileType);

            if (tracker)
            {
                tracker.TrackTransform(projectileTransform);
                return;
            }

            Debug.LogWarning("Failed to scoop projectile!");
        }

        public void UntrackProjectile(Transform projectileTransform)
        {
            trackerHandler.Dump(projectileTransform);
        }

        #endregion

        #region Reticles
        void SetupReticle()
        {
            //! Make 2 points
            List<Vector2> linePoints = new List<Vector2>();
            linePoints.Add(Vector2.zero);
            linePoints.Add(Vector2.zero);

            //reticleLineRenderer.UpdatePoints(linePoints);
        }

        void UpdateReticle()
        {
            Vector3 rotInput = Vector3.zero;
            if (!pauseMenuOpen) rotInput = playerRotationInput.AllInput;

            Vector2 destination = (Vector2)rotInput * maxDirectorRadius;
            if (destination.sqrMagnitude >= maxDirectorRadius * maxDirectorRadius) destination = destination.normalized * maxDirectorRadius;
            reticleDirectorsFR.StartLerp(destination);
            reticleDirectorsFR.Step(directorReactionSpeed * Time.deltaTime);

            float rdFRDist = reticleDirectorsFR.DistanceFromOrigin;
            float linePPU = Mathf.Lerp(minPixelsPerUnit, maxPixelsPerUnit, rdFRDist / maxDirectorRadius);
            reticleLineImage.pixelsPerUnitMultiplier = linePPU;
            reticleLineImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, reticleDirectorsFR.AngleFromOriginDeg);
            reticleLineImage.rectTransform.offsetMax = new Vector2(rdFRDist, reticleLineImage.rectTransform.offsetMax.y);
            //reticleLineRenderer.SetPoint(1, reticleDirectorsFR.center);

            Quaternion targetQT = Quaternion.Euler(reticleGroup.localRotation.x, reticleGroup.localRotation.y, rotInput.z * maxReticleRotation);
            reticleGroup.localRotation = Quaternion.Lerp(reticleGroup.localRotation, targetQT, 10f * Time.deltaTime);
            
        }
        #endregion

        #region Utilities
        public void UpdateCurrentUtility(string utilityName)
        {
            utilitiesHandler.UpdateCurrentUtilities(utilityName);
        }
        #endregion

        #region Pause menu
        void SetupPauseMenu()
        {
            playerInput = new StandardUseableInput();
            pauseMenuOpen = false;
            pauseMenu.Close();
        }

        void TriggerPauseMenu()
        {
            pauseMenuOpen = !pauseMenuOpen;

            if (pauseMenuOpen) PNTR_Pause();
            else PNTR_Resume();
        }

        public void PNTR_Debug()
        {
            DebugLog("Pointer entered");
        }

        public void PNTR_Resume()
        {
            pauseMenu.Close();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            //Cursor.lockState = CursorLockMode.Confined;
            if (PauseMenuClosed != null) PauseMenuClosed.Invoke();

            pauseMenuOpen = false;
        }

        public void PNTR_Pause()
        {
            pauseMenu.Open();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            if (PauseMenuOpened != null) PauseMenuOpened.Invoke();

            pauseMenuOpen = true;
        }

        public void PNTR_Disconnect()
        {
            neManager.LeaveRoom(true,true);
        }
        #endregion

        #region Accessors

        public bool IsOpen => pauseMenuOpen;
        public static bool IsNull => Instance == null;
        #endregion
    }
}
