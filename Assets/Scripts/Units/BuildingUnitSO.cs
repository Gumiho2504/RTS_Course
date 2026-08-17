using UnityEngine;

namespace Gumiho_Rts.Units
{
    [CreateAssetMenu(fileName = "Building", menuName = "Buildings/Building")]
    public class BuildingUnitSO : UnitSO
    {
        [field: SerializeField] public Material BuildingGhostPlacement { get; private set; }
        public override object Clone()
        {
            BuildingUnitSO copy = base.Clone() as BuildingUnitSO;
            copy.SightConfig = SightConfig == null ? null : Instantiate(SightConfig);
            return copy;
        }
    }
}