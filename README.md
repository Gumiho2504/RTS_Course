# RTS_Course

Unity RTS course project: units, buildings, commands, tech tree upgrades, owner-scoped events, and runtime UI.

Also includes **Gumiho Editor Tool** — a custom Unity inspector with tabs, title groups, buttons, and collection drawers. Inherit `GumihoBehaviour` and use attributes from the `GumihoEditorTool` namespace.

---

## Script architecture

Runtime game code lives under `Assets/Scripts/`. Systems talk through **ScriptableObject data**, the **Command pattern**, and an **owner-scoped Event Bus**.

### Folder roles

| Folder | Role |
|--------|------|
| **Units** | Selectable / damageable entities, buildings, workers, military, transport; unit & building data SOs |
| **Commands** | `BaseCommand` ScriptableObject actions executed via `CommandContext` |
| **TechTree** | Unlockables, upgrades / modifiers, per-owner tech tree state |
| **EventBus** | Generic `Bus<T>` keyed by `Owner` |
| **Events** | Event payloads (`UnitSelectedEvent`, `SupplyEvent`, …) |
| **Player** | `PlayerInput` (selection, commands, camera), `Supplies` economy |
| **UI** | `RuntimeUI` + containers / components bound to selection & commands |
| **Behavior** | Unity Behavior Graph actions / conditions (move, attack, gather, build) |
| **Environment** | Gatherables / supply sources |
| **Utilities** | Comparers, animation constants |

---

### High-level system flow

```mermaid
flowchart LR
  PI[PlayerInput] -->|select / world click| AC[AbstractCommandable]
  AC -->|UnitSelectedEvent| UI[RuntimeUI / ActionUI]
  UI -->|CommandSelectedEvent| PI
  PI -->|Handle CommandContext| CMD[BaseCommand]
  CMD -->|Move / Attack / Gather| AU[AbstractUnit + BehaviorGraph]
  CMD -->|queue unlockable| BB[BaseBuilding]
  BB -->|BuildUnlockable| TT[TechTreeSO / UpgradeSO]
  BB -->|UpgradeResearchedEvent| AC
  BB -->|SupplyEvent| SUP[Supplies]
  AU -->|UnitSpawn / UnitDeath| PI
  AU -->|UnitSpawn / UnitDeath| UI
```

1. **PlayerInput** tracks selection and issues the active command.
2. **UI** shows the intersection of `AvailableCommands` by slot; a click raises `CommandSelectedEvent`.
3. **Commands** drive units (Behavior Graph) or buildings (build / research queue).
4. **TechTree** unlocks from buildings / researched upgrades; commands check `IsUnlocked` / `IsResearched`.
5. **Upgrades** apply modifiers onto `UnitSO` data when researched (and again on spawn if already researched).

---

### Data inheritance (ScriptableObjects)

```mermaid
classDiagram
  direction TB
  class UnlockableSO {
    Name
    Cost
    TechTree
    UnlockRequirements
  }
  class UnitSO {
    Health
    Prefab
    Upgrades
  }
  class Unit {
    AttackConfig
    TransportConfig
  }
  class BuildingUnitSO
  class UpgradeSO {
    PropertyPath
    Apply(UnitSO)
  }
  class AdditiveIntModifierSO {
    Amount
  }
  class AdditiveFloatModifierSO {
    Amount
  }
  class IModifier

  UnlockableSO <|-- UnitSO
  UnlockableSO <|-- UpgradeSO
  UnitSO <|-- Unit
  UnitSO <|-- BuildingUnitSO
  UpgradeSO <|-- AdditiveIntModifierSO
  UpgradeSO <|-- AdditiveFloatModifierSO
  IModifier <|.. UpgradeSO
```

Related assets: `AttackConfigSO`, `TransportConfigSO`, `SupplyCostSO`, `TechTreeSO`.

---

### Runtime unit inheritance

```mermaid
classDiagram
  direction TB
  class AbstractCommandable {
    UnitSO
    Owner
    AvailableCommands
    Select / Deselect
    TakeDamage
  }
  class BaseBuilding {
    Build queue
    BuildUnlockable
  }
  class AbstractUnit {
    NavMeshAgent
    BehaviorGraphAgent
  }
  class Worker
  class BaseMilitaryUnit
  class Grenadier
  class AirTransport

  AbstractCommandable <|-- BaseBuilding
  AbstractCommandable <|-- AbstractUnit
  AbstractUnit <|-- Worker
  AbstractUnit <|-- BaseMilitaryUnit
  AbstractUnit <|-- AirTransport
  BaseMilitaryUnit <|-- Grenadier

  ISelectable <|.. AbstractCommandable
  IDamageable <|.. AbstractCommandable
  IMoveable <|.. AbstractUnit
  IAttacker <|.. AbstractUnit
  IBuildingBuilder <|.. Worker
  ITransportable <|.. Worker
  ITransportable <|.. BaseMilitaryUnit
  ITransporter <|.. AirTransport
```

---

### Command pattern

```mermaid
classDiagram
  direction TB
  class ICommand
  class BaseCommand {
    Slot
    Icon
    CanHandle
    Handle
    IsLocked
    IsAvailable
  }
  class CommandContext {
    Commandable
    Owner
    Hit
  }

  ICommand <|.. BaseCommand
  BaseCommand <|-- MoveCommand
  BaseCommand <|-- StopCommand
  BaseCommand <|-- AttackCommand
  BaseCommand <|-- GatherCommand
  BaseCommand <|-- BuildBuildingCommand
  BaseCommand <|-- BuildUnitCommand
  BaseCommand <|-- CancelBuildingCommand
  BaseCommand <|-- ResearchUpgradeCommand
  BaseCommand <|-- LoadUnitCommand
  BaseCommand <|-- LoadIntoCommand
  BaseCommand <|-- UnloadAllUnitsCommand
  BaseCommand <|-- OverrideCommandsCommand
  BaseCommand ..> CommandContext : uses
```

`AbstractCommandable.AvailableCommands` holds the SO command list the UI shows for the current selection.

---

### Event bus relationships

`Bus<T>` where `T : IEvents` — raise / subscribe per `Owner`.

```mermaid
flowchart TB
  subgraph Raise
    AC[AbstractCommandable]
    AU[AbstractUnit]
    BB[BaseBuilding]
    AT[AirTransport]
    UI[ActionUI]
  end

  subgraph Bus["Bus&lt;T&gt; by Owner"]
    USE[UnitSelected / Deselected]
    USP[UnitSpawn / Death]
    BS[BuildingSpawn]
    CS[CommandSelected]
    SE[SupplyEvent]
    UR[UpgradeResearched]
    UL[UnitLoad / Unload]
  end

  subgraph Listen
    PI[PlayerInput]
    RUI[RuntimeUI]
    TT[TechTreeSO]
    SUP[Supplies]
    AC2[AbstractCommandable Apply upgrades]
  end

  AC --> USE
  AU --> USP
  BB --> BS
  BB --> UR
  BB --> SE
  UI --> CS
  AT --> UL

  USE --> PI
  USE --> RUI
  USP --> PI
  USP --> RUI
  BS --> TT
  BS --> RUI
  CS --> PI
  SE --> SUP
  SE --> RUI
  UR --> TT
  UR --> AC2
  UR --> RUI
  UL --> RUI
```

Behavior Graph uses its own **EventChannels** (`GatherSuppliesEventChannel`, `BuildingEventChannel`, `LoadUnitEventChannel`) — separate from `Bus<T>`.

---

### Tech tree & upgrades

```mermaid
sequenceDiagram
  participant UI as ActionUI
  participant PI as PlayerInput
  participant CMD as ResearchUpgradeCommand
  participant BB as BaseBuilding
  participant TT as TechTreeSO
  participant U as AbstractCommandable

  UI->>PI: CommandSelectedEvent
  PI->>CMD: Handle(context)
  CMD->>BB: BuildUnlockable(Upgrade)
  Note over BB: Queue finishes research
  BB->>TT: UpgradeResearchedEvent
  TT->>TT: Mark researched / unlock deps
  BB->>U: UpgradeResearchedEvent
  U->>U: Upgrade.Apply(UnitSO)
```

- **UnlockableSO** is shared by trainable units and researchable upgrades.
- Buildings queue either type; completion either spawns a unit or raises `UpgradeResearchedEvent`.
- Additive modifiers use reflection on `PropertyPath` (e.g. `AttackConfig/Damage`).

> **Note:** `Apply` mutates shared ScriptableObject data. If every alive unit handles `UpgradeResearchedEvent` and runs `+=`, the bonus stacks by unit count. Prefer apply-once globally, or runtime bonuses per owner.

---

### UI structure

```mermaid
flowchart TB
  RuntimeUI --> ActionUI
  RuntimeUI --> SingleUnitSelectedUI
  RuntimeUI --> BuildingSelectedUI
  RuntimeUI --> BuildingBuildingUI
  RuntimeUI --> BuildingUnderConstructorUI
  RuntimeUI --> UnitTransportUI
  RuntimeUI --> UnitIconUI

  ActionUI --> UIActionButton
  BuildingBuildingUI --> UIBuildQueueButton
  UnitIconUI --> UIUnitButton
```

Containers implement `IUIElement<T…>` (`EnableFor` / `Disable`) and refresh from selection / bus events.

---

### Design patterns used

| Pattern | Where |
|---------|--------|
| **ScriptableObject data** | Units, buildings, costs, attacks, commands, upgrades, tech tree |
| **Command** | `ICommand` / `BaseCommand` + `CommandContext` |
| **Event bus** | Owner-scoped `Bus<T>` decoupling player, UI, tech, economy |
| **Unlockable pipeline** | Shared `UnlockableSO` for train + research queues |
| **Behavior Graph** | Movement / combat / gather / build via blackboard `UnitCommand` |
| **Capability interfaces** | `ISelectable`, `IDamageable`, `IMoveable`, `IAttacker`, `IBuildingBuilder`, `ITransportable`, `ITransporter`, `IModifier` |
| **Typed UI elements** | `IUIElement<T…>` for containers and buttons |
