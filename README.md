# Unity Project Collection

This repository contains multiple Unity projects that are kept isolated from each other.

## Projects

### Solar System Project

The original root-level Unity project is an interactive solar system visualization.

- Open from the repository root in Unity Hub
- Main scene: `Assets/Scenes/SampleScene.unity`

### Mountain Goat Project

The new game project is stored as a separate Unity project under `projects/mountain-goat/`.

- Open `projects/mountain-goat/` in Unity Hub as its own project
- Main scenes: `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/GamePlay.unity`, `Assets/Scenes/GameOver.unity`

## Isolation Strategy

The two Unity projects are not merged into one editor project.

- `Solar System Project` stays at the repository root
- `Mountain Goat Project` lives under `projects/mountain-goat/`
- Each project keeps its own `Assets`, `Packages`, and `ProjectSettings`
- Generated Unity folders such as `Library` and `Temp` are ignored

## Notes

- Use Unity Hub to open the correct project folder depending on which project you want to work on
- Do not move assets between the two projects unless you intend to share them manually
