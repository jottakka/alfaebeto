# AlfaEBeto

AlfaEBeto is an educational 2D top-down space shooter game designed to help users learn and practice aspects of various languages. The game is built using the Godot Engine with C#.

![Print]([https://s7.ezgif.com/tmp/ezgif-7827bf2d6e1ff1.gif](https://freeimage.host/i/3LyLx2e))
## 🎮 Gameplay

Players navigate a spacecraft and engage with enemies that represent linguistic challenges. The core gameplay loop involves shooting targets related to words, letters, grammar rules, and spelling, tailored to the selected language.

### Key Educational Mechanics:
* **Language Focus:** The game currently supports learning modules for German, English, Portuguese, and Japanese. Ideas for Spanish have also been considered.
* **Word & Grammar Challenges:**
    * **Diacritics (Accents):** Players might need to identify or shoot words with correct accentuation (e.g., `EnemyWord.cs` which uses `DiactricalMarkWordResource`).
    * **Spelling Rules:** Challenges involving correct spelling, such as choosing between "s" or "z", or "x" or "ch" (e.g., `MeteorWordTarget.cs` using `SpellingRuleWordResource`).
    * **German Grammar:** Specific challenges for German include identifying correct noun genders (der, die, das) and articles in different cases (e.g., `GuessArticleBlockEnemy.cs`, `MeteorGuessTarget.cs` utilizing `GermanPrepositionPhrase` for dative/accusative prepositions).
    * **Japanese Kana:** Players practice identifying Hiragana or Katakana characters (e.g., `GuessBlockEnemy.cs` using `HiraganaResourceProvider`).
    * **Verb Forms & More:** Ideas for future challenges include verb conjugations (past forms), identifying misspelled words, and understanding word meanings in context.
* **Enemy Variety:**
    * **Letter Blocks/Words:** Enemies that are themselves words or letters which players must interact with correctly.
    * **Answer Meteors:** Players choose the correct answer from multiple meteor-like options.
    * **Special Linguistic Enemies:** Bosses and enemies with unique mechanics tied to language rules, such as a "Kanji cyborg" or enemies that test knowledge of German gender.
    * **Regular Shooter Enemies:** Traditional space shooter enemies supplement the educational ones.
* **Player Progression & Learning:**
    * **Unlockable Rules:** Players can unlock and learn specific grammatical rules through a dedicated UI, potentially using in-game currency (gems).
    * **Adaptive Difficulty:** The game design includes considerations for adjusting difficulty based on player performance and suggesting areas for improvement.
    * **Performance Tracking:** The system tracks player accuracy with words and rules, storing this data for progression.

### Core Game Features:
* **Space Shooter Action:** Classic top-down shooter movement, firing projectiles (lasers), and evading enemy attacks.
* **Player Systems:** Health, shield, weapon cooldowns, and collection of in-game items.
* **Collectibles:** Coins for score/currency, gems (red and green) likely for unlocking content, health pickups, and shield boosters.
* **Diverse Enemy Types:** Including standard shooter enemies, word-based challenges, and meteor showers.
* **Dynamic Spawning System:** Enemies are spawned dynamically, with configurations for different languages and enemy types.
* **User Interface:** Includes a main menu, in-game HUD (health, shield, money, gems), pause menu, game over screen, and interfaces for viewing and unlocking educational rules.

## 🛠️ Technology

* **Game Engine:** Godot Engine (version not specified, but export presets suggest Godot 4.x).
* **Programming Language:** C#

## 💡 Ideas & Future Development (from `FileName.txt`)

* **Advanced Enemy Mechanics:**
    * Bosses with shields requiring specific linguistic input to disable.
    * Enemies with multiple destructible parts that must be targeted in a specific order (e.g., forming words, counting).
    * "Match two" type challenges with options in columns.
    * Swarm enemies where only misspelled/incorrect ones should be shot.
* **Trap Mechanics:** Path choices based on linguistic correctness (e.g., choosing the correct past tense of a verb).
* **Language-Specific Deep Dives:**
    * **German:** Dative conjunctions, verb + conjunction declensions.
    * **English:** Past tense of verbs.
    * **Spanish:** Gender picking, past tense of verbs, false friends for Portuguese speakers.
* **Feature Enhancements:**
    * Dynamic difficulty scaling.
    * Player progression paths.
    * Incremental improvements/rewards based on experience.
    * System for suggesting content based on player difficulties.
* **Gameplay Balancing:**
    * Limiting enemy spawn rates.
    * Allowing only one special enemy at a time.
    * Adjusting meteor spawn intervals.
    * Using visual cues (colors) that fade as the player improves.
