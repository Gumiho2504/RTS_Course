using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = "Scriptable Objects/UnitSO")]
public class UnitSO : ScriptableObject
{
    [field: SerializeField] public int Health { get; private set; } = 100;
    [field: SerializeField] public GameObject Prefab { get; private set; }
    [field: SerializeField] public float BuildTime { get; private set; } = 5;
}
