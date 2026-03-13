using System.Collections;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public interface IBuildingBuilder
    {

        public void Build(BuildingUnitSO building, Vector3 position);
    }
}