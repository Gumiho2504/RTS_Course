

namespace Gumiho_Rts.TechTree
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;
    using System.Reflection;
    using System;
    public abstract class UpgradeSO : UnlockableSO, IModifier
    {
        [field: SerializeField] public string PropertyPath { get; private set; }

        public abstract void Apply(UnitSO unit);

        protected T GetProperty<T>(UnitSO unit,out object target ,out PropertyInfo propertyInfo)
        {
            string[] attributes = PropertyPath.Split('/');
            if (attributes.Length == 0 || string.IsNullOrEmpty(PropertyPath))
            {
                Debug.LogError($"Unable to apply modifier {Name}: PropertyPath is empty!");
                throw new InvalidPathSpecifiedException(PropertyPath);
            }

            Type type = unit.GetType();
             target = unit;

            // Walk to the parent of the final property (e.g. Unit -> AttackConfig)
            for (int i = 0; i < attributes.Length - 1; i++)
            {
                 propertyInfo = type.GetProperty(attributes[i]);
                if (propertyInfo == null)
                {
                    Debug.LogError($"Unable to apply modifier {Name} to attribute {PropertyPath} because {attributes[i]} does not exist on {type.Name}!");
                    throw new InvalidPathSpecifiedException(PropertyPath);
                }

                target = propertyInfo.GetValue(target);
                if (target == null)
                {
                    Debug.LogError($"Unable to apply modifier {Name} to attribute {PropertyPath} because {attributes[i]} is null on {unit.Name}!");
                    throw new InvalidPathSpecifiedException(PropertyPath);
                }

                type = target.GetType();
            }

            // Apply amount to the final int property (e.g. Damage)
            string finalAttribute = attributes[^1];
             propertyInfo = type.GetProperty(finalAttribute);
            if (propertyInfo == null)
            {
                Debug.LogError($"Unable to apply modifier {Name} to attribute {PropertyPath} because {finalAttribute} does not exist on {type.Name}!");
                throw new InvalidPathSpecifiedException(PropertyPath);
            }

            T returnValue = default;
            try
            {
                returnValue = (T)propertyInfo.GetValue(target);

            }
            catch (InvalidCastException)
            {
                Debug.LogError($"Unable to apply modifier {Name} to attribute {PropertyPath} because it is not an int!");
            }
            
            return returnValue;

        }
    }
}