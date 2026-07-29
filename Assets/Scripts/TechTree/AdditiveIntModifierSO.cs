using System;
using System.Reflection;
using UnityEngine;

namespace Gumiho_Rts.TechTree
{
    [CreateAssetMenu(fileName = "Additive Int Modifier", menuName = "Tech Tree/Modifier/Additive Int Modifier", order = 160)]
    public class AdditiveIntModifierSO : UpgradeSO
    {
        [field: SerializeField] public int Amount { get; private set; }

        public override void Apply(UnitSO unit)
        {
            Debug.Log($"{Name} is applying {Amount} to {PropertyPath}.");
            // PropertyPath should be "AttackConfig/Damage" (property names, not type names)
            try
            {
                int currentValue = GetProperty<int>(unit, out object target, out PropertyInfo attributeField);
                Debug.Log($"Adding {Amount} to {PropertyPath} which is currently {currentValue}");
                currentValue += Amount; ;
                attributeField.SetValue(target, currentValue);
                Debug.Log($"Updated value to: {attributeField.GetValue(target)}");
            }
            catch (InvalidCastException) { }
        }
    }
}
