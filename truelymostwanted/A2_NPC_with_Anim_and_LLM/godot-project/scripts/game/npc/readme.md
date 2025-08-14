# 🧠 NPC State Representation – Documentation

This document provides an overview of the `NpcState` struct used to represent the perception, memory, and behavior of a non-player character (NPC) in the **LabyrinthExplorer3D** project.  
It is intended for both developers and machine learning engineers working on behavior modeling or dataset generation.

---

## 📦 Struct Overview

| Name                      | Data Type        | Description                                                                 | Domain / Values                          |
|---------------------------|------------------|-----------------------------------------------------------------------------|-------------------------------------------|
| `Personality`             | `Personality`    | The personality trait of the NPC.                                           | `Neutral`, `Shy`, `Aggressive`            |
| `SeesPlayer`              | `bool`           | Whether the NPC currently sees the player.                                  | `true` / `false`                          |
| `HearsPlayer`             | `bool`           | Whether the NPC currently hears the player.                                 | `true` / `false`                          |
| `VisualDistanceToPlayer` | `float`          | Distance to player based on vision (meters). `-1` if not visible.           | `0.0+` or `-1`                            |
| `AuditoryDistanceToPlayer` | `float`        | Distance to player based on sound (meters). `-1` if not audible.            | `0.0+` or `-1`                            |
| `PlayerMovementState`     | `CharacterState` | Perceived movement state of the player.                                     | `Idle`, `Wave`, `Walk`, `Sprint`, `Crouch`, `PlayDead`, `Unknown` |
| `CanMoveForward`          | `bool`           | Whether the NPC has a clear path forward.                                   | `true` / `false`                          |
| `CanMoveBackward`         | `bool`           | Whether the NPC has a clear path backward.                                  | `true` / `false`                          |
| `CanMoveLeft`             | `bool`           | Whether the NPC has a clear path to the left.                               | `true` / `false`                          |
| `CanMoveRight`            | `bool`           | Whether the NPC has a clear path to the right.                              | `true` / `false`                          |
| `TimeSinceLastSeen`       | `float`          | Seconds since the player was last seen.                                     | `0.0+` seconds                            |
| `TimeSinceLastHeardPlayer`| `float`          | Seconds since the player was last heard.                                    | `0.0+` seconds                            |
| `AlertLevel`              | `float`          | Current awareness or threat perception (0 = calm, 1 = max alert).           | `0.0 – 1.0`                               |
| `CurrentAction`           | `CharacterState` | The current action the NPC is performing.                                   | Same as `CharacterState` enum             |
| `CurrentActionDuration`   | `float`          | Duration in seconds the current action has been active.                     | `0.0+` seconds                            |

---

## 🧮 Normalization Details

Some variables are normalized for input into machine learning models:

| Variable                 | Normalization Logic                                                  |
|--------------------------|-----------------------------------------------------------------------|
| `VisualDistanceToPlayer`| `distance / maxVisibleDistance`, or `1.1` if not seen                 |
| `AuditoryDistanceToPlayer`| `distance / maxHearingDistance`, or `1.1` if not heard             |
| `TimeSinceLastSeen`     | `time / 60.0f`, clamped to `[0, 1]`                                  |
| `TimeSinceLastHeardPlayer`| `time / 60.0f`, clamped to `[0, 1]`                               |
| `CurrentActionDuration` | `time / 30.0f`, clamped to `[0, 1]`                                  |
| `AlertLevel`            | Already normalized in `[0, 1]`                                       |
| Enums (`Personality`, `CharacterState`) | Mapped and normalized via `(value - min) / (max - min)`   |

---

## 📘 Enum Definitions

### Personality
```csharp
public enum Personality
{
    Neutral = 0,
    Shy = 1,
    Aggressive = 2
}
