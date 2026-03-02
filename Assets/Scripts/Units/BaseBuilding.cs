using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public int QueueSize => buildingQueue.Count;
        public UnitSO[] Queue => buildingQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO BuildingUnit { get; private set; }

        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;


        private List<UnitSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;

        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count == MAX_QUEUE_SIZE)
            {
                Debug.LogError("BuildUnit called when the queue was already full ! This is not supported!");
                return;
            }
            buildingQueue.Add(unit);
            if (buildingQueue.Count == 1)
                StartCoroutine(DoBuildUnits());
            else
                OnQueueUpdated?.Invoke(buildingQueue.ToArray());

        }
        public void CancelBuildUnit(int index)
        {
            if (index < 0 || index >= buildingQueue.Count)
            {
                Debug.LogError("CancelBuildUnit called with an invalid index");
                return;
            }
            buildingQueue.RemoveAt(index);
            if (index == 0)
            {
                StopAllCoroutines();
                if (buildingQueue.Count > 0) StartCoroutine(DoBuildUnits());
                else OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }
            else
            {

                OnQueueUpdated?.Invoke(buildingQueue.ToArray());
            }

        }

        IEnumerator DoBuildUnits()
        {
            while (buildingQueue.Count > 0)
            {
                BuildingUnit = buildingQueue[0];
                CurrentQueueStartTime = Time.time;

                OnQueueUpdated?.Invoke(buildingQueue.ToArray());

                yield return new WaitForSeconds(BuildingUnit.BuildTime);

                Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                buildingQueue.RemoveAt(0);
            }
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());

        }
    }
}