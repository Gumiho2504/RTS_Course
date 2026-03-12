

using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "StopCommand", menuName = "Units/Commands/Stop", order = 101)]
    public class StopCommand : ActionBase
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            var unit = context.Commandable as AbstractUnit;
            unit.Stop();
        }
    }


}