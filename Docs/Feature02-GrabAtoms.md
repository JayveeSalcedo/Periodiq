# Feature 2: Grab a tile → 3D atom pops out

Touch any tile on the placed table → a Bohr-model atom pops out in front of it and follows your finger. Let go → it floats there with a gentle bob. Touch it again to move it. While holding, a second finger pinch pushes it nearer/farther. Max 12 atoms at once.

Scripts:
- `Atoms/AtomFactory.cs` – builds the atom procedurally (nucleus of protons + neutrons, one tilted ring per shell, orbiting electrons). Heavy atoms are capped at 40 visible nucleons for performance.
- `Atoms/Atom.cs` – the spawned atom: element data, pop-out animation, hover.
- `Interaction/GrabController.cs` – touch → raycast → spawn/pick up/drag/drop.

## Unity setup
1. Create 4 materials in `Assets/Materials/`:
   - `Proton` – URP/Lit, red `#FF5C5C`
   - `Neutron` – URP/Lit, grey `#B0B4BB`
   - `Electron` – URP/Lit, cyan `#3EE6FF`, tick **Emission** (same cyan)
   - `Shell` – URP/Unlit, Surface Type **Transparent**, white at ~35% alpha
2. Empty GameObject `AtomFactory` + `AtomFactory` script → assign the 4 materials.
3. Empty GameObject `GrabController` + `GrabController` script → assign `XR Origin` (for the ARTablePlacer field) and `AtomFactory`.
4. Make sure the tile prefab's **BoxCollider** is on the root (from Feature 1) — that's what the touch hits.

## Test
Place the table → tap and drag **H**, **O**, **Na**, **Fe** → check the shells look right (Na = 2, 8, 1).

**Send back:** a screenshot with 3–4 atoms floating in front of the table.
