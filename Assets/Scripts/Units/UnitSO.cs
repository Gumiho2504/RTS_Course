using Gumiho_Rts.TechTree;
using Gumiho_Rts.Units;
using UnityEngine;

// AbstractUnitSO
public abstract class UnitSO : UnlockableSO
{
    [field: SerializeField] public int Health { get; private set; } = 100;
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public UpgradeSO[] Upgrades { get; private set; }
    [field: SerializeField] public SightConfigSO SightConfig { get; protected set; }
}
