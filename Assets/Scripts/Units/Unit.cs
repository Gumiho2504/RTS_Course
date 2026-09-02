using UnityEngine;

namespace Gumiho_Rts.Units
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit")]
    public class Unit : UnitSO
    {
        [field: SerializeField] public AttackConfigSO AttackConfig { get; private set; }
        [field: SerializeField] public TransportConfigSO TransportConfig { get; private set; }
        
        public override object Clone()
        {
            Unit copy = base.Clone() as Unit;

            copy.AttackConfig = AttackConfig == null ? null : Instantiate(AttackConfig);
            copy.TransportConfig = TransportConfig == null ? null : Instantiate(TransportConfig);
            copy.SightConfig = SightConfig == null ? null : Instantiate(SightConfig);
            copy.PopulationConfig = PopulationConfig == null ? null : Instantiate(PopulationConfig);

            return copy;
        }

    }
}