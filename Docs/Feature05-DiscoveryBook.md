# Feature 5: Discovery Book

A 3D book stands beside the periodic table. The cover shows progress (e.g. `7 / 20`).

- **Tap the closed book** → it opens into a two-page spread floating above it, turned toward you.
- **Compounds tab** – all 20 compounds are listed. Ones you've made are coloured (blue = covalent, purple = ionic); the rest are grey with "Not made yet".
- **Tap a found compound** → its molecule appears in the reaction zone and the book closes so you can see it.
- **Badges tab** – 8 badges, gold when earned.
- Progress bar + count in the header, Prev / Next to change pages, X to close.
- **Toasts** drop in from the top: "New discovery! Water added to your Discovery Book" and "Badge unlocked: …".
- Progress is **saved on the phone** (PlayerPrefs) and survives closing the app.

## Badges
| Badge | How to earn |
|---|---|
| First Bond | Make any compound |
| Opposites Attract | Make an ionic compound |
| Organic Starter | Make methane (CH₄) |
| Triple Threat | Make a triple-bond molecule (N₂ or CO) |
| Air Collector | Make H₂, O₂, N₂ and Cl₂ |
| Halfway There | Discover half of all compounds |
| Salt Master | Make every ionic compound |
| Master Chemist | Discover everything |

Add badges in `Discovery/Badges.cs`.

## Scripts
- `Discovery/DiscoveryStore.cs` – listens to `ReactionZone.CompoundFormed`, saves found compounds + badges, raises `NewDiscovery`, `BadgeEarned`, `Changed`. Right-click the component → **Reset progress** to clear saves while testing.
- `Discovery/Badges.cs` – badge list and rules
- `Discovery/DiscoveryBook.cs` – the 3D book (cover + world-space pages), all built in code
- `UI/Toast.cs` – top banners, queued
- `UI/UIBuild.cs` – UI building helpers
- `Interaction/ITappable.cs` – `GrabController` now calls `OnTap()` on tapped objects (used by the closed book)
- `ReactionZone.ShowMolecule()` – puts a molecule in the zone without counting as a new discovery

## Unity setup
1. Material `BookCover` – URP/Lit (colour set in code).
2. Empty `DiscoveryStore` object + `DiscoveryStore` script → assign `ReactionZone`.
3. Under `PeriodicTable`, empty child `DiscoveryBook` at local position **(0.42, 0, 0)** (right of the table) + `DiscoveryBook` script → assign `DiscoveryStore`, `ReactionZone`, `BookCover`.
4. Empty `Toast` object + `Toast` script → assign `DiscoveryStore`.
5. EventSystem from Feature 4 is required (world-space book buttons use it).

## Test
1. Make water → "New discovery!" + "Badge unlocked: First Bond" toasts.
2. Tap the book → water is coloured, others grey, progress 1 / 20.
3. Tap Water in the book → molecule appears in the zone, book closes.
4. Close and reopen the app → progress is still there.

**Send back:** screenshots of the closed book next to the table and the open book.
