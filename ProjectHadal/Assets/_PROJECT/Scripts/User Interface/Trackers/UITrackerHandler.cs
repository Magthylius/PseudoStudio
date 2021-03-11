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
        public int poolCount;
    }

    [SerializeField] Camera playerCamera;
    [SerializeField] List<TrackerPool> trackerPoolList;

    List<UITrackerBehaviour> trackerList;

    void Start()
    {
        trackerList = new List<UITrackerBehaviour>();
        foreach (TrackerPool tracker in trackerPoolList)
        {
            StartCoroutine(InstantiateTrackers(tracker));
        }
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

            yield return new WaitForEndOfFrame();
            count++;
        }
    }

    public UITrackerBehaviour Scoop(TrackerType type)
    {
        foreach (UITrackerBehaviour tracker in trackerList)
            if (tracker.Type == type && !tracker.isActiveAndEnabled) return tracker;

        return null;
    }

    public void Dump(UITrackerBehaviour tracker)
    {
        tracker.Untrack();
        tracker.Disable();
    }
}
