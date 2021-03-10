using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("Animator Settings")]
    public Animator loadingAnimator;

    void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }

    [Button("Stop")]
    public void StopPlayback() => loadingAnimator.StopPlayback();
    [Button("Play")]
    public void StartPlayback() => loadingAnimator.StartPlayback();

}
