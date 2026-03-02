using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.UI.Containers;
using Gumiho_Rts.Units;
using UnityEngine;
namespace Gumiho_Rts.UI
{

    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private ActionUI actionUI;
        [SerializeField] private BuildingBuildingUI buildingBuildingUI;
        private HashSet<AbstractCommandable> selectableUnits = new(12);
        void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;

        }

        void Start()
        {
            actionUI.Disable();
            buildingBuildingUI.Disable();
        }

        void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }

        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            if (args.Unit is AbstractCommandable unit)
            {
                selectableUnits.Add(unit);
                actionUI.EnableFor(selectableUnits);
            }
            if (selectableUnits.Count == 1 && args.Unit is BaseBuilding building)
            {
                buildingBuildingUI.EnableFor(building);
            }

        }
        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            if (args.Unit is AbstractCommandable unit)
            {
                selectableUnits.Remove(unit);
                print(selectableUnits.Count);
                if (selectableUnits.Count > 0)
                {
                    actionUI.EnableFor(selectableUnits);
                    if (selectableUnits.Count == 1 && selectableUnits.First() is BaseBuilding building)
                    {
                        buildingBuildingUI.EnableFor(building);
                    }
                    else
                    {
                        buildingBuildingUI.Disable();
                    }

                }
                else
                {
                    actionUI.Disable();
                    buildingBuildingUI.Disable();
                }
            }
        }


    }
}