using Gumiho_Rts.Environment;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Gather Action", menuName = "AI/Commands/Gather", order = 105)]
    public class GatherCommand : ActionBase
    {
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is Worker
              && context.Hit.collider
              && context.Hit.collider.TryGetComponent(out GatherableSupply _);
        }

        public override void Handle(CommandContext context)
        {
            Worker worker = context.Commandable as Worker;
            GatherableSupply gatherableSupply = context.Hit.collider.GetComponent<GatherableSupply>();

            worker.Gather(gatherableSupply);
        }
    }

}