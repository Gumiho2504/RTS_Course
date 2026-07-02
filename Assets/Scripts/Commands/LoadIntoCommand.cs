using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Load Unit into Transport", menuName = "Units/Commands/Load Unit Into", order = 107)]
    public class LoadIntoCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext context)
        {
            Debug.Log("Handle Load Unit Into Command");
            return context.Commandable is ITransportable
                && context.Hit.collider != null
                && context.Hit.collider.TryGetComponent(out ITransportable _);
        }

        public override void Handle(CommandContext context)
        {
            ITransportable transportable = (ITransportable)context.Commandable;
            ITransporter transporter = context.Hit.collider.GetComponent<ITransporter>();

            transportable.LoadInto(transporter);
        }

        public override bool IsLocked(CommandContext context) => false;
    }

}