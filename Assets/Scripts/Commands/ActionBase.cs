using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    public abstract class ActionBase : ScriptableObject, ICommand
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Slot { get; private set; }
        [field: SerializeField] public bool ActivateCommand { get; private set; }
        public abstract bool CanHandle(CommandContext context);
        public abstract void Handle(CommandContext context);
    }

}