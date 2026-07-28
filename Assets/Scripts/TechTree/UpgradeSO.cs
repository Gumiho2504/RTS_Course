

namespace Gumiho_Rts.TechTree
{
    using UnityEngine;
    public abstract class UpgradeSO : UnlockableSO, IModifier
    {
        [field:SerializeField]public string PropertyPath {get;private set;}

        public abstract void Apply(UnitSO unit);
    }
}