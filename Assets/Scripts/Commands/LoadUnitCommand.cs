using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Load Unit", menuName = "Units/Commands/Load Unit", order = 106)]
    public class LoadUnitCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext context)
        {
            Debug.Log("<color=blue> can handle load unit command</color>");
            return context.Commandable is ITransporter transporter
                && context.Hit.collider != null
                && context.Hit.collider.TryGetComponent(out ITransportable transportable)
                && transportable.Owner == transporter.Owner;
        }

        public override void Handle(CommandContext context)
        {
            ITransporter transporter = context.Commandable as ITransporter;
            ITransportable transportable = context.Hit.collider.GetComponent<ITransportable>();

            transporter.Load(transportable);
        }

        public override bool IsLocked(CommandContext context)
        {
            if (context.Commandable is ITransporter transporter)
            {
                return transporter.UsedCapacity >= transporter.Capacity;
            }
            return true;
        }
    }
}