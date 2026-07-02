using System.Linq;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        [field: SerializeField] public string Name { get; private set; } = " Command";
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField][field: Range(-1, 8)] public int Slot { get; private set; }
        [field: SerializeField] public bool RequiresClickToActivate { get; private set; } = true;
        [field: SerializeField] public bool IsSingleUnitCommand { get; private set; }
        [field: SerializeField] public GameObject GhostPrefab { get; private set; }
        [field: SerializeField] public BuildingRestrictionSO[] Restrictions { get; private set; }
        public abstract bool CanHandle(CommandContext context);
        public abstract void Handle(CommandContext context);
        public abstract bool IsLocked(CommandContext context);
        public bool AllRestrictionsPass(Vector3 point) => Restrictions.Length == 0 || Restrictions.All(restriction => restriction.CanPlace(point));
    }

}