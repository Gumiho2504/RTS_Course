namespace Gumiho_Rts.TechTree
{
    using UnityEngine;
    using System.Reflection;
    [CreateAssetMenu(fileName = "Additive Float Modifier", menuName = "Tech Tree/Modifier/Additive Float Modifier", order = 161)]
    public class AdditiveFloatModifierSO : UpgradeSO
    {
        [field: SerializeField] public float Amount { get; private set; }

        public override void Apply(UnitSO unit)
        {
            try
            {
                float currentValue = GetProperty<float>(unit, out object target, out PropertyInfo attributeField);
                Debug.Log($"Adding {Amount} to {PropertyPath} which is currently {currentValue}");
                currentValue += Amount;
                attributeField.SetValue(target, currentValue);
                Debug.Log($"Updated value to: {attributeField.GetValue(target)}");
            }
            catch (InvalidPathSpecifiedException) { }
        }
    }
}