# Unity Update System
Custom deterministic update manager for Unity that replaces `MonoBehaviour.Update` with a centralized pipeline. 
Handles execution order based on priority, fixed time or fixed frame updates, and per-entity timing via **ScriptableObject** update profiles.

## How to use
### 1. Add an Update Manager
Place ```UpdateManager``` prefab in your scene.

The manager is a singleton that automatically registers and handles all active ```UpdatableEntity``` instances.

### 2. Create an Updatable Entity
Inherit from ```UpdatableEntity```.

```csharp
using ThornDuck.UpdateSystem;
using UnityEngine;

public class Entity : UpdatableEntity
{
  protected override void OnStartEntity()
  {
    // Called once when registered
  }

  protected override void OnUpdateEntity()
  {
    // Custom update logic
  }
}
```

### 3. Create an Update Profile
Create and assign a new ```UpdateProfile``` asset.

If no profile is assigned, the system automatically uses the manager’s default profile.

## Update Modes
### Default
Updates every frame.

```csharp
UpdateMode.Default
```

Equivalent to a normal Unity ```Update()```.

### Fixed Time
Updates at fixed time intervals.

```csharp
UpdateMode.FixedTime
```

Useful if you have functions that do not require full FPS.

### Fixed Frame
Updates every X+1 frames.

```csharp
UpdateMode.FixedFrame
```

Useful if you want to execute two or more (heavy) functions in different frames.

### Execution Priority
Entities are automatically sorted by priority. Higher priority entities update first.

Changing priority at runtime automatically reinserts the entity in the correct order.

## Notes
> Use ```OnStartEntity()``` and ```OnUpdateEntity()``` instead of ```Start()``` and ```Update()```.

> ```DeltaTimeSeconds``` replaces ```Time.deltaTime```.
