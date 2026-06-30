using System.Collections;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public interface IBuildingBuilder
    {
        public bool IsBuilding { get; }
        public GameObject Build(BuildingUnitSO building, Vector3 position);
        public void ResumeBuilding(BaseBuilding building);
        public void CancelBuilding();
    }
}