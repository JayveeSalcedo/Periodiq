# Periodiq

[![Build Android](https://github.com/JayveeSalcedo/periodiq/actions/workflows/build-android.yml/badge.svg)](https://github.com/JayveeSalcedo/periodiq/actions/workflows/build-android.yml)

A mobile AR chemistry learning app. Place a 3D periodic table in your room, grab elements out of it, and combine them to build molecules.

- **Engine:** Unity 6 + AR Foundation (ARCore)
- **Platform:** Android first, iOS later

## Roadmap

| # | Feature | Status |
|---|---------|--------|
| 1 | Element data + AR table placement | ✅ Code ready, needs on-device test |
| 2 | Grab a tile → 3D atom pops out | ✅ Code ready, needs on-device test |
| 3 | Reaction zone + compound recipes | ⏳ |
| 4 | Compound info card + wrong-combo explanations | ⏳ |
| 5 | Discovery Book | ⏳ |
| 6 | Bondable-element glow + atom inspect | ⏳ |
| 7 | Missions + hints | ⏳ |
| 8 | Bond animations, sound, UI polish | ⏳ |
| 9 | Non-AR fallback mode | ⏳ |

Setup steps per feature are in [`Docs/`](Docs/). CI build setup: [`Docs/CI-Build.md`](Docs/CI-Build.md).

## Data

`Assets/Resources/elements.json` is derived from [Periodic-Table-JSON](https://github.com/Bowserinator/Periodic-Table-JSON) (no license file in that repo; check before publishing to the Play Store).
