# Feature 1: Element data + AR table placement

## 1. Project
- Unity 6 (6000.x LTS), template **Universal 3D** (URP) or the **AR Mobile** template.
- Package Manager → install **AR Foundation**, **Google ARCore XR Plugin**, **Input System**, **TextMeshPro** (import TMP Essentials when asked).
- Build Settings → **Android** platform.
- Project Settings:
  - XR Plug-in Management → Android tab → tick **Google ARCore**.
  - Player → Other: Graphics API **Vulkan or OpenGLES3** (remove OpenGLES2 if listed), Minimum API **29**, Scripting Backend **IL2CPP**, Target Architectures **ARM64** only.
  - Player → Active Input Handling: **Input System Package (New)** or **Both**.
- In Unity Hub create the project **inside this repo folder** (or create it elsewhere and move Packages/ + ProjectSettings/ here) so `Assets/` merges with the scripts already in the repo.

## 2. Tile prefab (`Assets/Prefabs/ElementTile.prefab`)
1. Empty GameObject `ElementTile` + **BoxCollider** (size 1,1,1) + `ElementTile` script.
2. Child `Body`: 3D Cube, scale (1,1,1), a URP/Lit material (white — colour is set per tile at runtime). Remove its BoxCollider.
3. Child `Symbol`: **TextMeshPro (3D, not UI)**, font size ~6, centred, position (0, 0.05, -0.51), colour dark.
4. Child `Number`: TextMeshPro, size ~2.5, top-left aligned, position (-0.35, 0.38, -0.51).
5. Child `Name`: TextMeshPro, size ~1.6, centred, position (0, -0.36, -0.51).
   (Text sits slightly in front of the cube's front face, which faces -Z.)
6. Drag the children into the script's fields. Save as prefab.

## 3. Scene
1. Delete Main Camera. GameObject → XR → **XR Origin (Mobile AR)** and **AR Session**.
2. On XR Origin add **AR Plane Manager** (assign the default AR plane prefab: GameObject → XR → AR Default Plane, save as prefab), **AR Raycast Manager**, and `ARTablePlacer`.
3. Empty GameObject `PeriodicTable` + `PeriodicTableBuilder`, assign the tile prefab. Leave cellSize 0.03 (table ≈ 55 cm wide).
4. Optional reticle: a flat Quad with a ring texture, rotated (90,0,0), scale 0.1.
5. On `ARTablePlacer` assign `PeriodicTable` and the reticle.

## 4. Test
Build & Run to an ARCore phone. Move the phone slowly over a desk until planes appear → tap → the full table should stand on the desk facing you.

**Send back:** a screenshot of the placed table (and one close-up of a few tiles).
