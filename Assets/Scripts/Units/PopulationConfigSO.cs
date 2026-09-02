using UnityEngine;

namespace Gumiho_Rts.Units
{
    [CreateAssetMenu(fileName = "Population Config", menuName = "Population Config",order = 1)]
    public class PopulationConfigSO : ScriptableObject
    {
        [field: SerializeField] public int PopulationCost { get; private set; }
        [field:SerializeField] public int PopulationSupply { get; private set; }
    }
}