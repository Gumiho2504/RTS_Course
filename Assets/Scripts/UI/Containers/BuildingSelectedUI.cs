using System;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.UI;
using Gumiho_Rts.UI.Containers;
using Gumiho_Rts.Units;
using UnityEngine;

public class BuildingSelectedUI : MonoBehaviour, IUIElement<BaseBuilding>
{
    [SerializeField] private BuildingBuildingUI buildingBuildingUI;
    [SerializeField] private SingleUnitSelectedUI singleUnitSelectedUI;
    [SerializeField] private BuildingUnderConstructorUI buildingUnderConstructorUI;

    private BaseBuilding selectedBuilding;

    public void EnableFor(BaseBuilding building)
    {
        selectedBuilding = building;
        selectedBuilding.OnQueueUpdated -= OnBuildingQueueUpdated;
        selectedBuilding.OnQueueUpdated += OnBuildingQueueUpdated;
        if (building.Progress.State == BuildingProgress.BuildingState.Completed)
        {
            buildingBuildingUI.Disable();
            buildingUnderConstructorUI.Disable();
            OnBuildingQueueUpdated();

        }
        else
        {
            buildingUnderConstructorUI.EnableFor(building);
            singleUnitSelectedUI.Disable();
            buildingBuildingUI.Disable();

            Bus<BuildingSpawnEvent>.OnEvent += HandleBuildingSpawn;

        }
    }



    public void Disable()
    {
        singleUnitSelectedUI.Disable();
        buildingBuildingUI.Disable();
        buildingUnderConstructorUI.Disable();
        Bus<BuildingSpawnEvent>.OnEvent -= HandleBuildingSpawn;
        if (selectedBuilding != null)
        {
            selectedBuilding.OnQueueUpdated -= OnBuildingQueueUpdated;
            selectedBuilding = null;
        }
    }


    private void OnBuildingQueueUpdated(UnitSO[] _ = null)
    {
        if (selectedBuilding.QueueSize == 0)
        {
            singleUnitSelectedUI.EnableFor(selectedBuilding);
            buildingBuildingUI.Disable();
        }
        else
        {
            buildingBuildingUI.EnableFor(selectedBuilding);
            singleUnitSelectedUI.Disable();
        }
    }
    private void HandleBuildingSpawn(BuildingSpawnEvent args)
    {
        if (args.Unit == selectedBuilding)
        {
            buildingUnderConstructorUI.Disable();
           OnBuildingQueueUpdated();
            Bus<BuildingSpawnEvent>.OnEvent -= HandleBuildingSpawn;
        }
    }


}