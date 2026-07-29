using UnityEngine;
using GumihoEditorTool;
using System.Collections.Generic;

/// <summary>
/// Demo for Gumiho tabs / foldouts / buttons.
/// Must inherit <see cref="GumihoBehaviour"/> (not MonoBehaviour) for the custom inspector.
/// </summary>
public class ExampleComponent : GumihoBehaviour
{
    [TabGroup("General", order: 0)]
    [InfoBox("Core identity and references for this unit.")]
    [RequiredField("Assign the player root.")]
    public Transform player;

    [TabGroup("General")]
    [ReadOnlyField]
    public string runtimeId;

    [TabGroup("General")]
    public bool enableCombat = true;

    [TabGroup("Combat", order: 1)]
    [ShowIf(nameof(enableCombat))]
    [TitleGroup("Weapons")]
    public float damage = 10f;

    [TabGroup("Combat")]
    [ShowIf(nameof(enableCombat))]
    [TitleGroup("Weapons")]
    public float attackRange = 2.5f;

    [TabGroup("Combat")]
    [ShowIf(nameof(enableCombat))]
    [FoldoutGroup("Advanced")]
    public float critChance = 0.1f;

    [TabGroup("Movement", order: 2)]
    [TitleGroup("Speed")]
    public float walkSpeed = 3.5f;

    [TabGroup("Movement")]
    [TitleGroup("Speed")]
    public float runSpeed = 6f;

    [TabGroup("Movement")]
    [TitleGroup("Speed")]
    [ListDrawerSettings(ShowDropZone = true)]
    [SerializeField]
    private List<string> waypoints = new List<string>();

    [TabGroup("Movement")]
    [TitleGroup("Speed")]
    [ListDrawerSettings(ShowDropZone = true, ElementLabel = "Element {0}")]
    public Transform[] anchors;

    [TabGroup("Movement")]
    [EnableIf(nameof(enableCombat))]
    [InfoBox("Only editable while Combat is enabled.", InfoBoxType.Warning)]
    public bool chaseWhenAttacking;

    [TabGroup("General")]
    [Buttons("Reset Speeds")]
    private void ResetSpeeds()
    {
        walkSpeed = 3.5f;
        runSpeed = 6f;
        damage = 10f;
    }

    [TabGroup("Combat")]
    [Buttons("Log Combat Stats")]
    [ShowIf(nameof(enableCombat))]
    private void LogCombat()
    {
        Debug.Log($"DMG {damage}  Range {attackRange}  Crit {critChance}");
    }

    [Buttons("Ping Player")]
    private void LogPlayer()
    {
        Debug.Log(player != null ? player.name : "Missing player");
    }
}
