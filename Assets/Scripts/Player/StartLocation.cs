using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using Gumiho_Rts.Environment;
using Gumiho_Rts.EventBus;
using Gumiho_Rts.Events;
using Gumiho_Rts.Units;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StartLocation : MonoBehaviour
{
    [SerializeField] private Owner player;
    [SerializeField] private BuildingUnitSO startBuilding;
    [SerializeField] private UnitSpawnData[] startingUnits;
    [SerializeField] private StartingSupplyData[] startingSupplies;

    IEnumerator Start()
    {
        GameObject buildingSO = Instantiate(startBuilding.Prefab, transform.position, transform.rotation);
        BaseBuilding baseBuilding = buildingSO.GetComponent<BaseBuilding>();
        baseBuilding.Owner = player;
        baseBuilding.enabled = true;
        baseBuilding.Heal(startBuilding.Health);

        yield return null;

        Bounds bounds = new(transform.position, Vector3.one);
        if (baseBuilding.TryGetComponent(out Collider collider))
        {
            bounds = collider.bounds;
        }

        int totalUnits = startingUnits.Sum(item => item.NumberToSpawn);
        for (int i = 0; i < startingUnits.Length; i++)
        {
            for (int count = 0; count < startingUnits[i].NumberToSpawn; count++)
            {

                float offset = (i + count) / (float)totalUnits * bounds.size.x;
                Vector3 spawnPosition = new Vector3(
                    bounds.min.x + offset,
                    bounds.min.y,
                    bounds.min.z + offset
                );

                GameObject unitGO = Instantiate(startingUnits[i].UnitSO.Prefab, spawnPosition, Quaternion.Euler(0, Random.value * 180, 0));
                AbstractCommandable commandable = unitGO.GetComponent<AbstractCommandable>();
                commandable.Owner = player;

                Bus<PopulationEvent>.Raise(player, new PopulationEvent(
                        player,
                        commandable.UnitSO.PopulationConfig.PopulationCost,
                        commandable.UnitSO.PopulationConfig.PopulationSupply
                        ));

            }
        }

        foreach (var supply in startingSupplies)
        {
            Bus<SupplyEvent>.Raise(player, new SupplyEvent(player, supply.StartingAmount, supply.SupplySO));
        }


        Destroy(GetComponentInChildren<DecalProjector>());
        enabled = false;
    }




    [System.Serializable]
    private struct UnitSpawnData
    {
        [field: SerializeField] public UnitSO UnitSO { get; private set; }
        [field: SerializeField] public int NumberToSpawn { get; private set; }
    }

    [System.Serializable]
    private struct StartingSupplyData
    {
        [field: SerializeField] public SupplySO SupplySO { get; private set; }
        [field: SerializeField] public int StartingAmount { get; private set; }
    }
}
