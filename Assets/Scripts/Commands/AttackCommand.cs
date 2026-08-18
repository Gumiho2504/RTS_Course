using Gumiho_Rts.Player;
using Gumiho_Rts.Units;
using UnityEngine;
namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Attack", menuName = "Units/Commands/Attack")]
    public class AttackCommand : BaseCommand
    {
        public override bool CanHandle(CommandContext context)
        {
            //Debug.Log($"Handle Attack Command IAttacker: {context.Commandable is IAttacker}| Hit:{context.Hit.collider != null} |{context.Hit.transform.name}|Damageable{context.Hit.collider.TryGetComponent<IDamageable>(out IDamageable _)}");
            return context.Commandable is IAttacker && context.Hit.collider != null;
            //  && context.Hit.collider.TryGetComponent<IDamageable>(out IDamageable _);
        }

        public override void Handle(CommandContext context)
        {
            IAttacker attacker = context.Commandable as IAttacker;
            if (context.Hit.collider.TryGetComponent<IDamageable>(out IDamageable damageable)
            && IsHitColliderVisible(context)
            )
            {
                attacker.Attack(damageable);
            }
            else
            {
                attacker.Attack(context.Hit.point);
            }

        }

        public override bool IsLocked(CommandContext context) => false;
    }
}
