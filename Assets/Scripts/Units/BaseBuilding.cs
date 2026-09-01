using System;
using System.Collections;
using System.Collections.Generic;
using Gumiho_Rts.Commands;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Player;
using Gumiho_Rts.TechTree;
using RTS_Course.Assets.Scripts.Events;
using UnityEngine;
using UnityEngine.AI;

namespace Gumiho_Rts.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public int QueueSize => buildingQueue.Count;
        public bool IsQueueFull => buildingQueue.Count >= MAX_QUEUE_SIZE;
        public UnlockableSO[] Queue => buildingQueue.ToArray();
        [field: SerializeField] public float CurrentQueueStartTime { get; private set; }
        [field: SerializeField] public UnlockableSO SOBeingBuilt { get; private set; }
        [field: SerializeField] public BuildingProgress Progress { get; private set; } = new BuildingProgress(0, 0, BuildingProgress.BuildingState.Destroy);
        [SerializeField] private new Collider collider;
        [SerializeField] private CancelBuildingCommand cancelBuildingCommand;


        public delegate void QueueUpdatedEvent(UnlockableSO[] unitsInQueue);
        public event QueueUpdatedEvent OnQueueUpdated;

        public delegate void HealthUpdatedEvent(AbstractCommandable commandable, int lastHealth, int newHealth);
        public event HealthUpdatedEvent OnHealthUpdated;

        [field: SerializeField] public MeshRenderer MainMeshRenderer { get; private set; }

        private Placeholder culledVisual;
        private IBuildingBuilder unitBuildingThis;
        private List<UnlockableSO> buildingQueue = new(MAX_QUEUE_SIZE);
        private const int MAX_QUEUE_SIZE = 5;
        [field: SerializeField] public BuildingUnitSO BuildingSO { get; private set; }
        [SerializeField] private NavMeshObstacle navMeshObstacle;
        [SerializeField] private Material primaryMaterial;
        protected override void Awake()
        {
            base.Awake();
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

            if (BuildingSO.Upgrades != null && BuildingSO.TechTree != null)
            {
                foreach (UpgradeSO upgrade in BuildingSO.Upgrades)
                {
                    if (upgrade != null && BuildingSO.TechTree.IsResearched(Owner, upgrade))
                    {
                        upgrade.Apply(BuildingSO);
                    }
                }
            }

            if (collider != null)
            {
                collider.enabled = true;
            }

        }


        public void BuildUnlockable(UnlockableSO unlockable)
        {
            if (IsQueueFull)
                return;

            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, -unlockable.Cost.Minerals, unlockable.Cost.MineralsSO));
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, -unlockable.Cost.Gas, unlockable.Cost.GasSO));

            buildingQueue.Add(unlockable);
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

            UnlockableSO unlockableSO = buildingQueue[index];
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, unlockableSO.Cost.Minerals, unlockableSO.Cost.MineralsSO));
            Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, unlockableSO.Cost.Gas, unlockableSO.Cost.GasSO));

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
                SOBeingBuilt = buildingQueue[0];
                CurrentQueueStartTime = Time.time;

                OnQueueUpdated?.Invoke(buildingQueue.ToArray());

                yield return new WaitForSeconds(SOBeingBuilt.BuildTime);

                if (SOBeingBuilt is UnitSO unitSO)
                {
                    GameObject instance = Instantiate(unitSO.Prefab, transform.position, Quaternion.identity);
                    if (instance.TryGetComponent(out AbstractCommandable commandable))
                    {
                        commandable.Owner = Owner;
                    }
                }
                else if (SOBeingBuilt is UpgradeSO upgrade)
                {
                    Bus<UpgradeResearchedEvent>.Raise(Owner, new UpgradeResearchedEvent(Owner, upgrade));
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
            SetCommandOverride(new BaseCommand[] { cancelBuildingCommand });



            Progress = new BuildingProgress(Time.time - BuildingSO.BuildTime * Progress.Progress, Progress.Progress, BuildingProgress.BuildingState.Building);
            if (Progress.Progress == 0)
            {
                Heal(1);
            }

            if (collider != null)
            {
                collider.enabled = true;
            }


            Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;
            Bus<UnitDeathEvent>.OnEvent[Owner] += HandleUnitDeath;
        }

        public void CancelBuilding()
        {
            if (unitBuildingThis != null)
            {
                unitBuildingThis.CancelBuilding();
            }
            else
            {
                Destroy(this);

                Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, Mathf.FloorToInt(UnitSO.Cost.Minerals * 0.75f), UnitSO.Cost.MineralsSO));
                Bus<SupplyEvent>.Raise(Owner, new SupplyEvent(Owner, Mathf.FloorToInt(UnitSO.Cost.Gas * 0.75f), UnitSO.Cost.GasSO));

            }
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Bus<UnitDeathEvent>.OnEvent[Owner] -= HandleUnitDeath;

            Bus<BuildingDeathEvent>.Raise(Owner, new BuildingDeathEvent(Owner, this));

        }

        protected override void OnGainVisibility()
        {
            base.OnGainVisibility();
            if (culledVisual != null)
            {
                culledVisual.gameObject.SetActive(false);
            }
        }

        protected override void OnLoseVisibility()
        {
            base.OnLoseVisibility();
            if (culledVisual == null)
            {
                Transform originalRendererTransform = MainMeshRenderer.transform;
                GameObject culledObject = new($"Culled {BuildingSO.Name} Visuals")
                {
                    layer = LayerMask.NameToLayer("Supplies"),
                    transform =
                    {
                        position = originalRendererTransform.position,
                        rotation = originalRendererTransform.rotation,
                        localScale = originalRendererTransform.localScale
                    }
                };
                culledVisual = culledObject.AddComponent<Placeholder>();
                culledVisual.Owner = Owner;
                culledVisual.ParentObject = gameObject;

                MeshFilter meshFilter = culledObject.AddComponent<MeshFilter>();
                meshFilter.mesh = MainMeshRenderer.GetComponent<MeshFilter>().mesh;
                MeshRenderer meshRenderer = culledObject.AddComponent<MeshRenderer>();
                meshRenderer.material = MainMeshRenderer.material;
            }
            else
            {
                culledVisual.gameObject.SetActive(true);
            }
        }

        public override void Deselect()
        {
            base.Deselect();
            
            if (Progress.State != BuildingProgress.BuildingState.Completed)
            {
                SetCommandOverride(new BaseCommand[] { cancelBuildingCommand });
            }

        }

    }
}