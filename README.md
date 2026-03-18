# Spherical Hex Grid Navigation System

> Standalone C# system for navigating a hex tile graph built at runtime from any isoSphere mesh.

<img height="350" alt="Hex sphere navigation demo" src="https://github.com/user-attachments/assets/7c2a4c81-d794-4c51-ba66-15260d0c38b1" /><img height="350" alt="Camera-relative navigation gizmos" src="https://github.com/user-attachments/assets/fe27f9de-f9a9-4704-aac2-6fd0e2e7ec69" />

<img height="350" alt="Hex sphere tile graph" src="https://github.com/user-attachments/assets/b2aec161-b3d3-4c2f-9f77-981b69469270" />

---

## What it does

At runtime, reads a sphere mesh, groups triangles into hex faces, builds a neighbor graph, and lets a cursor navigate between tiles using camera-relative directional input (gamepad left stick).

---

## File overview

```
Code/
├── Systems/
│   ├── PlanetGraph.cs       – MonoBehaviour: mesh → tile graph at Awake
│   └── PlanetTile.cs        – Data class: center, normal, neighbors, occupancy
├── Gameplay/
│   └── PlanetGridSelector.cs – Moves a cursor between tiles based on stick input
└── Input/
    ├── IPlayerController.cs     – Interface: exposes Move (Vector2)
    ├── BasePlayerController.cs  – Abstract base, wires lifecycle to InputManager
    ├── BuildController.cs       – Concrete controller for the PlanetBuilder action map
    └── InputManager.cs          – Singleton: owns PlayerInputsystem, switches action maps
Settings/
└── PlayerInputsystem.inputactions  – Unity Input System bindings (gamepad)
```

---

## How it works

### 1. Mesh → Graph (`PlanetGraph`)
On `Awake`, groups mesh triangles by comparing averaged normals (`dot > 0.95`) — one group = one hex face. Computes each tile's `LocalCenter` and `LocalNormal`, sorts top-to-bottom, then auto-calculates a connection threshold from the closest tile pair to link neighbors.

### 2. Camera-relative navigation (`PlanetGridSelector`)
Each frame:
1. Projects the camera's right vector onto the tile's surface plane → `surfaceRight`
2. Derives `surfaceForward` via cross product
3. Combines stick input into a world-space `inputDir`
4. Scores each neighbor by `dot(inputDir, dirToNeighbor)`, moves to the best match

"Push right on the stick" always moves camera-right regardless of where on the sphere the cursor is.

### 3. Input (`BuildController` / `InputManager`)
`InputManager` is a singleton owning the generated `PlayerInputsystem` and exposes `SetInputMode(InputMode)` to enable one action map at a time. `BuildController` subscribes to the `PlanetBuilder` map and surfaces `Move`, `Look`, and `ZoomInput` as properties.
