using Gumiho_Rts.Units;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
namespace Gumiho_Rts.Behavoir
{

    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Build Building", story: "[Self] builds [BuildingSO] at [TargetLocation]", category: "Action/Units", id: "aef011bdaa0b93ede5b17dc948b5573e")]

    public partial class BuildBuildingAction : Action
    {
        [SerializeReference] public BlackboardVariable<GameObject> Self;
        [SerializeReference] public BlackboardVariable<BuildingUnitSO> BuildingSO;
        [SerializeReference] public BlackboardVariable<Vector3> TargetLocation;
        [SerializeReference] public BlackboardVariable<BaseBuilding> BuildingUnderConstruction;
        private float startBuildTime;
        private BaseBuilding completedBuilding;
        private Renderer buildingRenderer;
        private Vector3 startPosition;

        protected override Status OnStart()
        {
            if (!HasValidInput())
            {
                Debug.Log($"Building Failed");
                return Status.Failure;
            }
            if(BuildingUnderConstruction.Value == null)
            {
                GameObject building = GameObject.Instantiate(BuildingSO.Value.Prefab, TargetLocation.Value, Quaternion.identity);
                if (!building.TryGetComponent(out completedBuilding) || completedBuilding.MainMeshRenderer == null) return Status.Failure;
            }
            else
            {
                completedBuilding = BuildingUnderConstruction.Value;
            }
            completedBuilding.StartBuilding(Self.Value.GetComponent<IBuildingBuilder>());
            startBuildTime = completedBuilding.Progress.StartTime;
          
            //   GameObject building = GameObject.Instantiate(BuildingSO.Value.Prefab);
           

            buildingRenderer = completedBuilding.MainMeshRenderer;
            BuildingUnderConstruction.Value = completedBuilding;

            startPosition = TargetLocation.Value - Vector3.up * buildingRenderer.bounds.size.y;
            buildingRenderer.transform.position = startPosition;

            return OnUpdate();
        }



        protected override Status OnUpdate()
        {

            float normalizedTime = (Time.time - startBuildTime) / BuildingSO.Value.BuildTime;
            buildingRenderer.transform.position = Vector3.Lerp(startPosition, TargetLocation.Value, normalizedTime);
            return normalizedTime >= 1 ? Status.Success : Status.Running;
        }

        protected override void OnEnd()
        {
            if (CurrentStatus == Status.Success)
            {
                completedBuilding.enabled = true;
            }
        }

        private bool HasValidInput()
        {
            return Self.Value != null && BuildingSO.Value != null && BuildingSO.Value.Prefab != null;
        }
    }


}