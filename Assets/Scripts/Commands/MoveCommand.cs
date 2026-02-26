using Gumiho_Rts.Units;
using UnityEngine;

namespace Gumiho_Rts.Commands
{
    [CreateAssetMenu(fileName = "Move Command", menuName = "AI/Action/Move", order = 100)]
    public class MoveCommand : ActionBase
    {

        public float radiusMultiplier = 3.5f;
        int unitsOnLayer = 0;
        int maxUnitsLayer = 1;
        float circleRadius = 0;
        float radiusOffset = 0;
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            var unit = context.Commandable as AbstractUnit;
            var hit = context.Hit;

            if (context.UnitIndex == 0)
            {
                unitsOnLayer = 0;
                maxUnitsLayer = 1;
                circleRadius = 0;
                radiusOffset = 0;
            }
            var targetPosition = new Vector3(
                        hit.point.x + Mathf.Cos(radiusOffset * unitsOnLayer) * circleRadius,
                        hit.point.y,
                        hit.point.z + Mathf.Sin(radiusOffset * unitsOnLayer) * circleRadius
                    );

            unit.Move(targetPosition);

            unitsOnLayer++;
            if (unitsOnLayer >= maxUnitsLayer)
            {
                unitsOnLayer = 0;
                circleRadius += unit.AgentRadius * 3.5f;
                // The 3.5f is a spacing factor to ensure units don't overlap. Adjust as necessary.     
                //2 * 3.14 * 3.5 * 0.5 / 0.5 * 3.5 `= 10
                maxUnitsLayer = Mathf.FloorToInt(2 * Mathf.PI * circleRadius / (unit.AgentRadius * radiusMultiplier));
                radiusOffset = 2 * Mathf.PI / maxUnitsLayer;
            }
        }
    }
}