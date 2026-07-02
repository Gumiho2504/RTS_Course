using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Load Unit", menuName = "Units/Commands/Load Unit", order = 106)]
    public class LoadUnitCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is ITransporter
                && context.Hit.collider != null
                && context.Hit.collider.TryGetComponent(out ITransportable _);
        }

        public override void Handle(CommandContext context)
        {
            ITransporter transporter = context.Commandable as ITransporter;
            ITransportable transportable = context.Hit.collider.GetComponent<ITransportable>();

            transporter.Load(transportable);
        }

        public override bool IsLocked(CommandContext context)
        {
            return false;
        }
    }
}