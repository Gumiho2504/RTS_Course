using System;
using System.Collections;
using System.Collections.Generic;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using UnityEngine;
using UnityEngine.AI;

namespace Gumiho_Rts.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public int QueueSize => buildingQueue.Count;
        public UnitSO[] Queue => buildingQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnitSO BuildingUnit { get; private set; }
        [field: SerializeField] public BuildingProgress Progress { get; private set; } = new BuildingProgress(0, 0, BuildingProgress.BuildingState.Destroy);


        public delegate void QueueUpdatedEvent(UnitSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;

        public delegate void HealthUpdatedEvent(AbstractCommandable commandable, int lastHealth, int newHealth);
        public event HealthUpdatedEvent OnHealthUpdated;

        [field: SerializeField] public MeshRenderer MainMeshRenderer { get; private set; }

        private IBuildingBuilder unitBuildingThis;
        private List<UnitSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }
        [SerializeField] private NavMeshObstacle navMeshObstacle;
        [SerializeField] private Material primaryMaterial;
        protected void Awake()
        {
            BuildingSO = UnitSO as BuildingUnitSO;
            MaxHealth = BuildingSO.Health;
        }


        protected override void Start()
        {
            base.Start();
            if (MainMeshRenderer != null)
            {
                MainMeshRenderer.material = primaryMaterial;

            }
            Progress = new BuildingProgress(Progress.StartTime, 1, BuildingProgress.BuildingState.Completed);
            unitBuildingThis = null;
            Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
            Bus<BuildingSpawnEvent>.Raise(Owner, new BuildingSpawnEvent(Owner, this));
        }


        public void BuildUnit(UnitSO unit)
        {
            if (buildingQueue.Count == MAX_QUEUE_SIZE)
            {
                Debug.LogError("BuildUnit called when the queue was already full ! This is not supported!");
                return;
            }

            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, -unit.Cost.Minerals, unit.Cost.MineralsSO));
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, -unit.Cost.Gas, unit.Cost.GasSO));

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

            UnitSO unitSO = buildingQueue[index];
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, unitSO.Cost.Minerals, unitSO.Cost.MineralsSO));
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, unitSO.Cost.Gas, unitSO.Cost.GasSO));

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

                GameObject instance = Instantiate(BuildingUnit.Prefab, transform.position, Quaternion.identity);
                if (instance.TryGetComponent(out AbstractCommandable commandable))
                {
                    commandable.Owner = Owner;
                }
                buildingQueue.RemoveAt(0);
            }
            OnQueueUpdated?.Invoke(buildingQueue.ToArray());

        }

        public void StartBuilding(IBuildingBuilder buildingBuilder)
        {
            Awake();
            unitBuildingThis = buildingBuilder;
            Owner = buildingBuilder.Owner;
         //   Debug.Log("<color=green> BaseBuilding Start Build");



            Progress = new BuildingProgress(Time.time - BuildingSO.BuildTime * Progress.Progress, Progress.Progress, BuildingProgress.BuildingState.Building);
            if (Progress.Progress == 0)
            {
                Heal(1);
            }
            Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
            Bus<UnitDeathEvent>.OnEvent[Owner] += HandleUnitDeath;
        }

        private void HandleUnitDeath(UnitDeathEvent args)
        {
            if (args.Unit.TryGetComponent(out IBuildingBuilder builder) && builder == unitBuildingThis)
            {
                Progress = new BuildingProgress(Progress.StartTime, (Time.time - Progress.StartTime) / BuildingSO.BuildTime, BuildingProgress.BuildingState.Paused);
                Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
            }
        }

        public void ShowGhostVisual()
        {
            MainMeshRenderer.material = BuildingSO.BuildingGhostPlacement;
        }

        private void OnDestroy()
        {
            Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
        }

    }
}