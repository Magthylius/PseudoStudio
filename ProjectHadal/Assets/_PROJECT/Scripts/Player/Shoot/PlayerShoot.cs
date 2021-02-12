//created by Jin
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hadal.UI;
using Magthylius.DataFunctions;

public class PlayerShoot : MonoBehaviour
{
    UIManager uiManager;
    public bool debugEnabled = true;

    [Header("Torpedoes")]
    public int torpedoMaxCount = 4;
    public float floodDelay = 2f;
    public float reloadDelay = 2f;
    public bool startFullyLoaded = true;

    int torpedoCount;
    bool allowFlood;
    bool allowReload;
    Timer floodTimer;
    Timer reloadTimer;

    [Header("Utilities")]
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bullet;
    [SerializeField] float fireRate;
    [SerializeField] float force;

    #region Unity Lifecycle
    private void Start()
    {
        uiManager = UIManager.Instance;

        floodTimer = new Timer(floodDelay, true, true);
        reloadTimer = new Timer(reloadDelay);

        floodTimer.TargetTickedEvent.AddListener(FloodTorpedoes);
        reloadTimer.TargetTickedEvent.AddListener(ReloadTorpedoes);

        allowFlood = false;
        allowReload = true;

        if (startFullyLoaded)
        {
            torpedoCount = torpedoMaxCount;
            allowReload = false;
        }

#if UNITY_EDITOR
    if (debugEnabled) Debug.unityLogger.logEnabled = true;
    else Debug.unityLogger.logEnabled = false;
#else
    Debug.unityLogger.logEnabled = false;
#endif

        uiManager.UpdateTubes(torpedoCount);
    }

    private void Update()
    {
        if (allowFlood && torpedoCount > 0)
        {
            uiManager.UpdateFlooding(floodTimer.Progress);
            floodTimer.Tick(Time.deltaTime);

            //if (floodTimer.Progress >= 1f) allowFlood = false;
            //uiManager.UpdateFlooding(floodTimer.Progress);

            //Debug.Log("Flooding: " + floodTimer.Progress);
        }

        if (allowReload) reloadTimer.Tick(Time.deltaTime);

        // swap to input system
        if (Input.GetMouseButtonDown(0)) FireTorpedo();

        if(Input.GetMouseButtonDown(1))
        {
            GameObject projectile = Instantiate(bullet, firePoint.position, firePoint.rotation);
            projectile.GetComponent<Rigidbody>().AddForce(firePoint.forward * force);
        }
    }
    #endregion

    #region Torpedoes
    void ReloadTorpedoes()
    {
        if (torpedoCount < torpedoMaxCount)
        {
            torpedoCount++;
            uiManager.UpdateTubes(torpedoCount);
        }

        Debug.Log("Torpedo Loaded!");
    }

    void FloodTorpedoes()
    {
        allowFlood = false;
        uiManager.UpdateFlooding(1f);
        Debug.Log("Torpedo Flooded");
    }

    void FireTorpedo()
    {
        if (allowFlood || torpedoCount == 0) return;

        torpedoCount--;
        allowReload = true;

        if (torpedoCount > 0)
            allowFlood = true;
        
        floodTimer.Reset();
        uiManager.UpdateTubes(torpedoCount);
        Debug.Log("Torpedo Fired!");
    }
    #endregion
}
