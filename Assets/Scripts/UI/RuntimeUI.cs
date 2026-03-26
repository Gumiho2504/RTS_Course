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
        [SerializeField] private BuildingSelectedUI buildingSelectedUI;
        [SerializeField] private UnitIconUI unitIconUI;
        [SerializeField] private SingleUnitSelectedUI singleUnitSelectedUI;


        private HashSet<AbstractCommandable> selectableUnits = new(12);
        void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitDeathEvent>.OnEvent += HandleUnitDeath;
            Bus<SupplyEvent>.OnEvent += HandleSupplyChangeEvent;

        }


        void Start()
        {
            actionUI.Disable();
            unitIconUI.Disable();
            singleUnitSelectedUI.Disable();
            buildingSelectedUI.Disable();

        }

        void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitDeathEvent>.OnEvent -= HandleUnitDeath;
            Bus<SupplyEvent>.OnEvent -= HandleSupplyChangeEvent;
        }



        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            if (args.Unit is AbstractCommandable unit)
            {
                selectableUnits.Add(unit);
                // actionUI.EnableFor(selectableUnits);
                RefreshUI();
            }
            // if (selectableUnits.Count == 1 && args.Unit is BaseBuilding building)
            // {
            //     buildingBuildingUI.EnableFor(building);
            // }

        }
        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            if (args.Unit is AbstractCommandable commandable)
            {
                selectableUnits.Remove(commandable);
                RefreshUI();
            }
        }

        private void RefreshUI()
        {
            if (selectableUnits.Count > 0)
            {
                actionUI.EnableFor(selectableUnits);

                if (selectableUnits.Count == 1)
                {
                    AbstractCommandable commandable = selectableUnits.First();
                    unitIconUI.EnableFor(commandable);

                    if (commandable is BaseBuilding baseBuilding)
                    {
                        singleUnitSelectedUI.Disable();
                        buildingSelectedUI.EnableFor(baseBuilding);
                    }
                    else
                    {
                        buildingSelectedUI.Disable();
                        singleUnitSelectedUI.EnableFor(commandable);
                    }
                }
                else
                {
                    unitIconUI.Disable();
                    singleUnitSelectedUI.Disable();
                    buildingSelectedUI.Disable();
                }

                // if (selectableUnits.Count == 1 && selectableUnits.First() is BaseBuilding building)
                // {
                //     buildingSelectedUI.EnableFor(building);
                // }
                // else
                // {
                //     buildingSelectedUI.Disable();
                // }

            }
            else
            {
                actionUI.Disable();
                buildingSelectedUI.Disable();
                unitIconUI.Disable();
                singleUnitSelectedUI.Disable();

            }
        }
        private void HandleUnitDeath(UnitDeathEvent args)
        {
            selectableUnits.Remove(args.Unit);
            RefreshUI();
        }

        private void HandleSupplyChangeEvent(SupplyEvent args)
        {
            actionUI.EnableFor(selectableUnits);
        }
    }
}