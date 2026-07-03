using System;
using System.Collections.Generic;
using System.Linq;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.UI.Components;
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
        [SerializeField] private UnitTransportUI unitTransportUI;


        private HashSet<AbstractCommandable> selectableUnits = new(12);
        void Awake()
        {
            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitDeathEvent>.OnEvent += HandleUnitDeath;
            Bus<SupplyEvent>.OnEvent += HandleSupplyChangeEvent;
            Bus<UnitLoadEvent>.OnEvent += HandleUnitLoadEvent;
            Bus<UnitUnloadEvent>.OnEvent += HandleUnitUnloadEvent;

        }



        void Start()
        {
            actionUI.Disable();
            unitIconUI.Disable();
            singleUnitSelectedUI.Disable();
            buildingSelectedUI.Disable();
            unitTransportUI.Disable();

        }

        void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitDeathEvent>.OnEvent -= HandleUnitDeath;
            Bus<SupplyEvent>.OnEvent -= HandleSupplyChangeEvent;
            Bus<UnitLoadEvent>.OnEvent -= HandleUnitLoadEvent;
            Bus<UnitUnloadEvent>.OnEvent -= HandleUnitUnloadEvent;
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


        private void RefreshUI()
        {
            if (selectableUnits.Count > 0)
            {
                actionUI.EnableFor(selectableUnits);

                if (selectableUnits.Count == 1)
                {
                    ResolveUnitSingleSelectedUI();
                }
                else
                {
                    unitIconUI.Disable();
                    singleUnitSelectedUI.Disable();
                    buildingSelectedUI.Disable();
                    unitTransportUI.Disable();
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