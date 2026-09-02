using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.UI.Components;
using Gumiho_Rts.UI.Containers;
using Gumiho_Rts.Units;
using RTS_Course.Assets.Scripts.Events;
using UnityEngine;
namespace Gumiho_Rts.UI
{

    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private ActionUI actionUI;
        [SerializeField] private BuildingSelectedUI buildingSelectedUI;
        [SerializeField] private UnitIconUI unitIconUI;
        [SerializeField] private SingleUnitSelectedUI singleUnitSelectedUI;
        [SerializeField] private UnitTransportUI unitTransportUI;
        [SerializeField] private MultipleUnitsSelectedUI multipleUnitsSelectedUI;
        [SerializeField] private ControlGroupUI controlGroupUI;


        private HashSet<AbstractCommandable> selectableUnits = new(12);
        void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] += HandleUnitDeselected;
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] += HandleUnitDeath;
            Bus<SupplyEvent>.OnEvent[Owner.Player1] += HandleSupplyChangeEvent;
            Bus<UnitLoadEvent>.OnEvent[Owner.Player1] += HandleUnitLoadEvent;
            Bus<BuildingSpawnEvent>.OnEvent[Owner.Player1] += HandleBuildingSpawn;
            Bus<UpgradeResearchedEvent>.OnEvent[Owner.Player1] += HandleUpgradeResearched;
            Bus<BuildingDeathEvent>.OnEvent[Owner.Player1] += HandleBuildingDeath;
            // Bus<UnitUnloadEvent>.OnEvent += HandleUnitUnloadEvent;

        }



        void Start()
        {
            actionUI.Disable();
            unitIconUI.Disable();
            singleUnitSelectedUI.Disable();
            buildingSelectedUI.Disable();
            unitTransportUI.Disable();
            multipleUnitsSelectedUI.Disable();
          //  controlGroupUI.Disable();

        }

        void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent[Owner.Player1] -= HandleUnitDeselected;
            Bus<UnitDeathEvent>.OnEvent[Owner.Player1] -= HandleUnitDeath;
            Bus<SupplyEvent>.OnEvent[Owner.Player1] -= HandleSupplyChangeEvent;
            Bus<UnitLoadEvent>.OnEvent[Owner.Player1] -= HandleUnitLoadEvent;
            Bus<UnitUnloadEvent>.OnEvent[Owner.Player1] -= HandleUnitUnloadEvent;
            Bus<BuildingSpawnEvent>.OnEvent[Owner.Player1] -= HandleBuildingSpawn;
            Bus<UpgradeResearchedEvent>.OnEvent[Owner.Player1] -= HandleUpgradeResearched;
            Bus<BuildingDeathEvent>.OnEvent[Owner.Player1] -= HandleBuildingDeath;
        }


        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            if (args.Unit is AbstractCommandable unit)
            {
                selectableUnits.Add(unit);
                RefreshUI();
            }

        }
        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            if (args.Unit is AbstractCommandable commandable)
            {
                selectableUnits.Remove(commandable);
                RefreshUI();
            }
        }

        private void HandleUnitLoadEvent(UnitLoadEvent args)
        {
            if (selectableUnits.Count == 1 && selectableUnits.First() is ITransporter)
            {
                RefreshUI();
            }
            else if (args.Unit is AbstractCommandable commandable && selectableUnits.Contains(commandable))
            {
                commandable.Deselect(); // RefreshUI will be called because of UnitDeselectedEvent raised for this

            }
        }

        private void HandleUnitUnloadEvent(UnitUnloadEvent args)
        {
            if (selectableUnits.Count == 1 && selectableUnits.First() is ITransporter)
            {
                RefreshUI();
            }
        }

        private void HandleUpgradeResearched(UpgradeResearchedEvent args)
        {
            RefreshUI();
        }

        private void HandleBuildingSpawn(BuildingSpawnEvent args)
        {
            // if (selectableUnits.Count == 1 && selectableUnits.First() is Worker)
            // {
            actionUI.EnableFor(selectableUnits);
            // }
        }

        private void HandleBuildingDeath(BuildingDeathEvent args)
        {
            selectableUnits.Remove(args.Unit);
            RefreshUI();
        }



        private void RefreshUI()
        {
            controlGroupUI.EnableFor(selectableUnits);
            if (selectableUnits.Count > 0)
            {
                actionUI.EnableFor(selectableUnits);

                if (selectableUnits.Count == 1)
                {
                    multipleUnitsSelectedUI.Disable();
                    ResolveUnitSingleSelectedUI();
                }
                else
                {
                    unitIconUI.Disable();
                    singleUnitSelectedUI.Disable();
                    buildingSelectedUI.Disable();
                    unitTransportUI.Disable();
                    multipleUnitsSelectedUI.EnableFor(selectableUnits);
                }

            }
            else
            {
                DisableAllContainer();

            }
        }

        private void DisableAllContainer()
        {
            actionUI.Disable();
            buildingSelectedUI.Disable();
            unitIconUI.Disable();
            singleUnitSelectedUI.Disable();
            unitTransportUI.Disable();
             multipleUnitsSelectedUI.Disable();
        }

        private void ResolveUnitSingleSelectedUI()
        {
            AbstractCommandable commandable = selectableUnits.First();
            unitIconUI.EnableFor(commandable);

            if (commandable is BaseBuilding baseBuilding)
            {
                singleUnitSelectedUI.Disable();
                unitTransportUI.Disable();

                buildingSelectedUI.EnableFor(baseBuilding);

            }
            else if (commandable is ITransporter transporter && transporter.UsedCapacity > 0)
            {
                buildingSelectedUI.Disable();
                singleUnitSelectedUI.Disable();

                unitTransportUI.EnableFor(transporter);
            }
            else
            {
                buildingSelectedUI.Disable();
                unitTransportUI.Disable();

                singleUnitSelectedUI.EnableFor(commandable);
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