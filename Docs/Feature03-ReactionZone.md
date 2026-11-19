# Feature 3: Reaction zone + compound recipes

A glowing ring sits in front of the table. Drag atoms into it and let go:
- The ring pulses green while it waits for you to stop adding atoms.
- **Match** → atoms pull together into the centre and become a 3D ball-and-stick molecule (with a vibration).
- **No match** → ring flashes red (Feature 4 will explain *why*).
- If the atoms could still become something bigger (e.g. H + H could become H₂O), it waits ~3 s instead of ~1.2 s, so you have time to add the O.

## Recipes (`Assets/Resources/compounds.json`) — 20 to start
Covalent: H₂, O₂, N₂, Cl₂, H₂O, CO₂, CO, CH₄, NH₃, HCl, HF, H₂S
Ionic: NaCl, KCl, LiF, MgO, CaO, MgCl₂, CaCl₂, Na₂O

Each recipe has atom positions (ångströms, real bond angles), bonds (1/2/3, or 0 = ionic with no stick), name, formula, bond type and a one-line real-world use. Composition is worked out from the atom list, so adding a compound is just adding one entry.

## Scripts
- `Reactions/CompoundData.cs` – data classes + `CompoundDatabase` (exact match, "part of a bigger recipe" check)
- `Reactions/ReactionZone.cs` – detects released atoms in the ring, settle timer, reaction animation, events `CompoundFormed` / `ReactionFailed`
- `Reactions/MoleculeFactory.cs` – builds ball-and-stick (CPK colours, double/triple sticks; ionic gets a glow halo instead of sticks)
- `Reactions/Molecule.cs` – pop-in, slow spin, float
- `Reactions/CpkColors.cs` – element colours/sizes for molecules

## Unity setup
1. Materials in `Assets/Materials/`:
   - `MolAtom` – URP/Lit, white, Smoothness ~0.7 (colour is set per atom in code)
   - `MolBond` – URP/Lit, light grey `#C8CCD2`
   - `IonHalo` – URP/Unlit, Transparent, white at ~20% alpha
   - `ZoneRing` – URP/Unlit, Transparent, white (colour is set in code)
2. Empty `MoleculeFactory` object + `MoleculeFactory` script → assign `MolAtom`, `MolBond`, `IonHalo`.
3. Under the `PeriodicTable` object, add an empty child `ReactionZone` at local position **(0, 0.03, -0.25)** (25 cm in front of the table, toward you).
   - Add `ReactionZone` script → assign `MoleculeFactory`.
   - Add a **LineRenderer** component (material `ZoneRing`, Use World Space off) and drag it into the script's `Ring` field.

## Test
- H + H + O → water. H + Cl → HCl. Na + Cl → salt (no sticks, glowing ions).
- C + O + O → CO₂. Try He + Ne → red flash.

**Send back:** a screenshot of water and table salt in the zone.
