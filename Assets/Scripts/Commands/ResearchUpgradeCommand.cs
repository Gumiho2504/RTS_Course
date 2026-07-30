using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gumiho_Rts.Player;
using Gumiho_Rts.TechTree;
using Gumiho_Rts.Units;
using Unity.VisualScripting;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Research Upgrade", menuName = "Tech Tree/Research Upgrade Command", order = 140)]
    public class ResearchUpgradeCommand : BaseCommand
    {
        [field: SerializeField] public UpgradeSO Upgrade { get; private set; }
        private Dictionary<Owner, BaseBuilding.QueueUpdatedEvent> updateQueue = new();
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is BaseBuilding building && !building.IsQueueFull;
        }

        public override void Handle(CommandContext context)
        {
            if (!CanHandle(context) || IsLocked(context))
                return;

            BaseBuilding building = context.Commandable as BaseBuilding;
            if (HasEnoughSupply(context))
            {
                building.BuildUnlockable(Upgrade);
                if (updateQueue.TryAdd(context.Owner, GetQueueUpdatedFunction(context.Owner, building)))
                {
                    building.OnQueueUpdated += updateQueue[context.Owner];
                }
            }
        }



        private void HandleQueueUpdated(Owner owner, BaseBuilding baseBuilding, UnlockableSO[] unitsInQueue)
        {
            Debug.Log($"Handle Queue Update in {Name}");
            if (!unitsInQueue.Contains(Upgrade))
            {
                baseBuilding.OnQueueUpdated -= updateQueue[owner];
                updateQueue.Remove(owner);
            }
        }
        
        private BaseBuilding.QueueUpdatedEvent GetQueueUpdatedFunction(Owner owner, BaseBuilding baseBuilding)
        {
            return (unlockables) =>  HandleQueueUpdated(owner, baseBuilding, unlockables);
        }

        public override bool IsLocked(CommandContext context)
        {
            
            if (Upgrade == null || Upgrade.Cost == null || Upgrade.TechTree == null)
                return true;

            if (context.Commandable is BaseBuilding building)
            {
                if (building.IsQueueFull)
                    return true;

                if (Upgrade.IsOneTimeUnlock && updateQueue.ContainsKey(context.Owner))
                    return true;
            }

            return !HasEnoughSupply(context) || !Upgrade.TechTree.IsUnlocked(context.Owner, Upgrade);
        }

        public override bool IsAvailable(CommandContext context)
        {
            if (Upgrade == null || Upgrade.TechTree == null)
                return false;

            if (Upgrade.IsOneTimeUnlock && Upgrade.TechTree.IsResearched(context.Owner, Upgrade))
            {
                return false;
            }
            return Upgrade.TechTree.IsUnlocked(context.Owner, Upgrade);
        }

        private bool HasEnoughSupply(CommandContext context)
        {
            if (Upgrade == null || Upgrade.Cost == null)
                return false;

            return Upgrade.Cost.Minerals <= Supplies.Minerals[context.Owner] && Upgrade.Cost.Gas <= Supplies.Gas[context.Owner];
        }

    }
}