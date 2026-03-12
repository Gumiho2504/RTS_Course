using Gumiho_Rts.Environment;
using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Gather Action", menuName = "Units/Commands/Gather", order = 105)]
    public class GatherCommand : ActionBase
    {
        [SerializeField] private UnitSO commandPostBuilding;
        public override bool CanHandle(CommandContext context)
        {
            // Debug.Log($"Hit {context.Hit.collider.gameObject.name}");
            return context.Commandable is Worker
              && context.Hit.collider != null
              && IsGatherOrCommandPost(context.Hit.collider);
        }
        private bool IsGatherOrCommandPost(Collider collider)
        {
            return collider.TryGetComponent(out GatherableSupply _) || IsCommandPost(collider);
        }

        private bool IsCommandPost(Collider collider)
        {
            return collider.TryGetComponent(out BaseBuilding building) && building.UnitSO.Equals(commandPostBuilding);
        }

        public override void Handle(CommandContext context)
        {

            Worker worker = context.Commandable as Worker;
            if (context.Hit.collider.TryGetComponent(out GatherableSupply gatherableSupply))
            {
                // Debug.Log("Gather Supplies");
                worker.Gather(gatherableSupply);
            }
            else if (IsCommandPost(context.Hit.collider) && worker.HasSupplies)
            {
                //Debug.Log("Return Supplies");
                worker.ReturnSupplies(context.Hit.collider.gameObject);
            }
            else
            {
                //  Debug.Log("Move");
                worker.Move(context.Hit.collider.transform.position);
            }



        }
    }

}