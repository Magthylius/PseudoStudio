using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    private void Awake()
    {
        //!Singleton
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    //!OnEnable and OnDisable is the central to photon functioning correctly
    //!these two methods are the only base methods that needs to be called in order to override
    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == 0) return;
        string path1 = string.Empty, path2 = string.Empty;
        path2 = "PlayerManager";
        if (scene.buildIndex == 1) { path1 = "PhotonPrefabs"; }
        else if (scene.buildIndex == 2) { path1 = "PhotonControlPrefabs"; }
        
        PhotonNetwork.Instantiate(Path.Combine(path1, path2), Vector3.zero, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
