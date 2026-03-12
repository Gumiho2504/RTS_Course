using UnityEngine;

namespace Gumiho_Rts.Units
{
    [CreateAssetMenu(fileName = "Building", menuName = "Buildings/Building")]
    public class BuildingUnitSO : UnitSO
    {
        [field: SerializeField] public Material BuildingGhostPlacement { get; private set; }
    }
}