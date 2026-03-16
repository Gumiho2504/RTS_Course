using System.Collections;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public interface IBuildingBuilder
    {

        public GameObject Build(BuildingUnitSO building, Vector3 position);
    }
}