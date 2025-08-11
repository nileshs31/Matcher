# Matcher(For Confidential hush hush!)
A simple yet fully functional **Matcher Game** built in Unity, featuring **persistent save/load**, **score tracking**, and **customizable grid sizes**.

---

## 📌 Table of Contents
- [Overview](#overview)
- [Features](#features)
- [How the Save/Load System Works](#how-the-saveload-system-works)
- [Gameplay Flow](#gameplay-flow)
- [Scripts Overview](#scripts-overview)
- [How to Run](#how-to-run)
- [Build and Working Video](#build-and-working-video)

---

## Overview
This is a **grid-based memory game** where players flip cards to find matching pairs.  
The project includes a **persistent save system** using JSON files in `Application.persistentDataPath`, ensuring the game state (card order, matched cards, score, and timer) is restored exactly when the player clicks **Continue**.

---

## Features
- **Dynamic Grid Layout** – Select rows and columns before starting the game.
- **Randomized Card Order** – Cards are shuffled at the start.
- **Persistent Save System** – Stores:
  - Grid size
  - Card order
  - Matched card indexes
  - Score
  - Elapsed time
- **Continue Game Support** – Resume exactly where you left off.
- **Game Over Cleanup** – Automatically deletes the save file and resets PlayerPrefs when the game ends.
- **Sound Effects** – Flip, match, mismatch, and game over sounds.
- **Scalable Design** – Easily change the number of sprites, grid layout, and match logic.

---

## How the Save/Load System Works
1. **On Shuffle** → Saves grid size and shuffled sprite order.
2. **On Match Found** → Updates the save file with matched card indexes.
3. **On Continue** → Loads from the save file:
   - Restores card positions
   - Turns on already matched cards
   - Restores score & timer
4. **On Game Over** → Deletes save file & related PlayerPrefs.

---

## Gameplay Flow
1. Player selects **rows** and **columns** in the main menu.
2. Game shuffles the cards and starts the timer.
3. Player flips cards:
   - Match → Cards stay visible.
   - Mismatch → Cards flip back.
4. Player can leave and later click **Continue** to resume.
5. Game ends when all pairs are matched.

---

## Scripts Overview
- **`CardsController.cs`** – Handles grid setup, shuffling, save/load logic, and match checking.
- **`Card.cs`** – Represents an individual card with flip animations and sprite handling.
- **`ScoreManager.cs`** – Manages score and timer.
- **`GameOverHandler.cs`** – Handles win/loss UI and save cleanup.
- **`MainMenuController.cs`** – Handles menu interactions and Continue button logic.
- **`SoundManager.cs`** – Plays all game sound effects.

---

## How to Run
1. Open the project in Unity **2021.3+**.
2. Set the main menu scene as the starting scene.
3. Press **Play**.
4. Adjust rows/columns → Start game → Play.
5. Quit mid-game → Press **Continue** to resume.

---

## Build and Working Video 

Android Build - https://drive.google.com/file/d/1tqSsRP1KMWgT1Me3ChWYsqsqLexWSKYH/view?usp=drive_link

Windows Build - https://drive.google.com/file/d/1d7FknJ59sAkXBGBMGdlPlYMw02oTGnlE/view?usp=drive_link

Gameplay Video (With all the requested Features) - https://drive.google.com/file/d/1YtzYETFf1mwjA8OmCkM7jke8tDXXrwnN/view?usp=drive_link


---

## Example Save File (`board_order.json`)
```json
{
    "rows": 4,
    "cols": 4,
    "names": ["sprite1", "sprite2", "sprite3", "..."],
    "matchedIndex": [0, 5, 8, 12],
    "score": 200,
    "elapsedTime": 45.6
}
