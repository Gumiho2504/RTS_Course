using UnityEngine;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Override Commands", menuName = "Units/Commands/Override Commands", order = 110)]
    public class OverrideCommandsCommand : ActionBase
    {
        [field: SerializeField] public ActionBase[] commands { get; private set; }
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable != null;
        }

        public override void Handle(CommandContext context)
        {
            context.Commandable.SetCommandOverride(commands);
        }
    }
}