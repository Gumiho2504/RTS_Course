using System.Collections;
using UnityEngine;

namespace Gumiho_Rts.Units
{
    public class BaseBuilding : AbstractCommandable
    {
        public void BuildUnit(UnitSO unit)
        {
            StartCoroutine(DoBuildUnit(unit));
        }

        IEnumerator DoBuildUnit(UnitSO unit)
        {
            yield return new WaitForSeconds(unit.BuildTime);
            Instantiate(unit.Prefab, transform.position, Quaternion.identity);
        }
    }
}