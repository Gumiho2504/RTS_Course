using Gumiho_Rts.TechTree;
using UnityEngine;

// AbstractUnitSO
public abstract class UnitSO : UnlockableSO
{
    [field: SerializeField] public int Health { get; private set; } = 100;
    [field: SerializeField] public GameObject Prefab { get; private set; }
}
