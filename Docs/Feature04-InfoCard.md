# Feature 4: Compound info card + wrong-combo explanations

When the reaction zone forms a molecule, a card slides up from the bottom:
- Name, big formula, **COVALENT** / **IONIC** chip
- One-line real-world use
- A generated explanation:
  - Covalent → each atom's outer electrons and how many pairs it shares
  - Ionic → which atom gives electrons, which takes them, the ions formed (Na⁺, Cl⁻), and why the ratio is what it is (Na₂O: 2×1 = 1×2)

When atoms *don't* react, the card explains why, in this order:
1. **Noble gas** in the mix → "its outer shell is already full"
2. **Close to a real recipe** → "Almost! Add 1 H to make Water", "Too many atoms. Remove 1 Na…", "Close! Swap…"
3. **Only metals** → "nothing here wants to take electrons"
4. **Metal + nonmetal, wrong ratio** → "Na gives 1 electron but O needs 2. Try 2 Na + 1 O"
5. Otherwise → each atom's outer electrons and how many it still needs

"Almost there" cases (e.g. H + O sitting in the zone) get a yellow **HINT** card and no red flash.

## Scripts
- `Reactions/ChemistryExplainer.cs` – all explanation text (uses TMP `<sub>`/`<sup>` so any font works)
- `UI/InfoCardUI.cs` – bottom-sheet card built in code, slide animation, X to close
- `UI/ReactionFeedback.cs` – connects `ReactionZone` events to the card
- `GrabController` now ignores touches on UI, so tapping the card doesn't grab atoms behind it

## Unity setup
1. Make sure the scene has an **EventSystem** (GameObject → UI → Event System). If Unity asks, click **Replace with InputSystemUIInputModule**.
2. Empty GameObject `InfoCard` + `InfoCardUI` script (no prefab needed; it builds its own canvas).
3. Empty GameObject `ReactionFeedback` + `ReactionFeedback` script → assign `ReactionZone` and `InfoCard`.

## Test
- H + H + O → Water card (covalent). Na + Cl → Salt card (ionic).
- H + O → after ~3 s, yellow "Add 1 H" hint.
- Ne + Cl → noble-gas explanation. Na + O → "Add 1 Na to make Sodium oxide". Fe + Cu → metals-only explanation.

**Send back:** screenshots of one success card and one "why" card.
