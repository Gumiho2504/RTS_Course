namespace Gumiho_Rts.TechTree
{
    using System.Collections.Generic;
    using System.Linq;
    using Gumiho_Rts.Units;
    using UnityEngine;

    [CreateAssetMenu(menuName = "RTS_Course/UnlockableSO")]
    public abstract class UnlockableSO : ScriptableObject
    {
        [field: SerializeField] public string Name { get; private set; } = "Unit";
        [field:SerializeField] public bool IsOneTimeUnlock {get;private set;}
        [field: SerializeField] public float BuildTime { get; private set; } = 5;
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public SupplyCostSO Cost { get; private set; }
        [field:SerializeField] public TechTreeSO TechTree {get;private set;}
        [field:SerializeField] public List<UnlockableSO> unlockRequirements {get;private set;} = new();

        public IEnumerable<UnlockableSO> UnlockRequirements => unlockRequirements.ToList();
    }
}