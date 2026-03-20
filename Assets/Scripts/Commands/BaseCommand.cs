using System.Linq;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    public abstract class BaseCommand : ScriptableObject, ICommand
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Slot { get; private set; }
        [field: SerializeField] public bool RequiresClickToActivate { get; private set; } = true;
        [field: SerializeField] public GameObject GhostPrefab { get; private set; }
        [field: SerializeField] public BuildingRestrictionSO[] Restrictions { get; private set; }
        public abstract bool CanHandle(CommandContext context);
        public abstract void Handle(CommandContext context);
        public bool AllRestrictionsPass(Vector3 point) => Restrictions.Length == 0 || Restrictions.All(restriction => restriction.CanPlace(point));
    }

}