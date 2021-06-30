using Hadal.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//! C: Jon
/// <summary>
/// Handles object pooling of trackers
/// </summary>
public class UITrackerHandler : MonoBehaviour
{
    [System.Serializable]
    struct TrackerPool
    {
        public GameObject trackerPrefab;
        public Transform spawnParent;
        public TrackerType trackerType;
        public int poolCount;

        public TrackerPool(TrackerPool other)
        {
            trackerPrefab = other.trackerPrefab;
            spawnParent = other.spawnParent;
            trackerType = other.trackerType;
            poolCount = other.poolCount;
        }
    }

    [SerializeField] Camera playerCamera;
    [SerializeField] Transform playerTransform;
    [SerializeField] List<TrackerPool> trackerPoolList;

    List<UITrackerBehaviour> trackerList;

    void Start()
    {
        trackerList = new List<UITrackerBehaviour>();
        foreach (TrackerPool tracker in trackerPoolList)
        {
            StartCoroutine(InstantiateTrackers(tracker));
        }
        //print(scaleFactor);
    }

    void Update()
    {
        
    }

    IEnumerator InstantiateTrackers(TrackerPool tracker)
    {
        int count = 0;
        while (count < tracker.poolCount)
        {
            var track = Instantiate(tracker.trackerPrefab, tracker.spawnParent);
            trackerList.Add(track.GetComponent<UITrackerBehaviour>());
            track.GetComponent<UITrackerBehaviour>().InjectDependencies(playerCamera, playerTransform);

            yield return new WaitForEndOfFrame();
            count++;
        }
    }

    public UITrackerBehaviour Scoop(TrackerType type)
    {
        foreach (UITrackerBehaviour tracker in trackerList)
            if (tracker.Type == type && !tracker.isActiveAndEnabled) return tracker;

        foreach (TrackerPool tracker in trackerPoolList)
        {
            if (tracker.trackerType == type) 
            {
                var track = Instantiate(tracker.trackerPrefab, tracker.spawnParent);
                trackerList.Add(track.GetComponent<UITrackerBehaviour>());
                return track.GetComponent<UITrackerBehaviour>();
            }
        }

        Debug.LogWarning("Tracker type not found!");
        return null;
    }

    public void Dump(UITrackerBehaviour tracker)
    {
        tracker.Untrack();
        tracker.Disable();
    }

    public void Dump(Transform trackerTransform)
    {
        foreach (UITrackerBehaviour tracker in trackerList)
        {
            if (tracker.TrackingTransform == trackerTransform)
            {
                Dump(tracker);
                return;
            }
        }

        Debug.LogWarning("Tracker not found, cannot dump!");
    }
}
