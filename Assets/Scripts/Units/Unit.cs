using UnityEngine;

namespace Gumiho_Rts.Units
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit")]
    public class Unit : UnitSO
    {
        [field: SerializeField] public AttackConfigSO AttackConfig { get; private set; }
    }
}