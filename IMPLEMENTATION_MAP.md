# UNI-Verse — Thesis ↔ Code Implementation Map

This document maps every technique described in the bachelor thesis
(*The UNI-Verse*, Draica Elena-Georgiana) to its implementation in the codebase.
Files marked **NEW** were added on the `thesis-techniques` branch; files marked
**UPDATED** were refactored from the `ui-setup` branch.

## Chapter 3 — Design and Methodology

| Thesis section | Technique | Implementation |
|---|---|---|
| §3.2 | Strict component isolation & content/presentation abstraction | `Managers/GameEvents.cs` **NEW** — subsystems communicate only over static delegate channels; `Narrative/StoryManager.cs` + `Narrative/DialogueUI.cs` split narrative logic from rendering |
| §3.3 | Single-singleton rule: GameManager is the sole `DontDestroyOnLoad` entity | `Managers/GameStateManager.cs` **UPDATED** — only persistent object (self-bootstraps before scene load); `PlayerData`, `StatSystem`, `TimeManager` **UPDATED** to scene-local facades holding no state |
| §3.3 | Scene-local StoryManager initializes Ink from scratch and reads values injected by the GameManager | `Narrative/StoryManager.cs` **NEW** — `InitializeStory()` + `InjectGlobalState()` |
| §3.4.1 | No-database paradigm | No DBMS anywhere; the whole state is one plain serializable object |
| §3.4.2 | JSON serialization layer: `[System.Serializable]` plain C# object → `JsonUtility.ToJson()` → local file I/O | `Managers/SaveData.cs` **NEW** + `Managers/SaveSystem.cs` **NEW** → `gamesave.json` in `Application.persistentDataPath` |
| §3.4.3 | Immutability policy: read-only fields, constructor-based new instances | `Managers/SaveData.cs` — private fields, no setters, `With*()` factories; `PlayerData.PlayerName` is getter-only (`PlayerData.TotalCredits += 10`-style writes are compile errors) |
| §3.5 | Character Customization use case | `Scene01/scene01_Events.cs` **UPDATED** — name input panel flow → immutable string in the data layer |
| §3.5 | Dialogue Selection use case (choice system) | `Narrative/DialogueUI.cs` **NEW** — Ink choices become buttons; picked index raised over `OnChoiceSelected` |
| §3.5 | Scene Interactivity use case | already present (`Scene02_Event` tree/house) |
| §3.5 | Chapter Retry & Progression Triggers use cases | `GameStateManager.BeginChapter()` / `EvaluateChapterEnd()` |
| §3.6 | Event-driven component architecture: static C# events, subscribe `OnEnable` / unsubscribe `OnDisable` | `Managers/GameEvents.cs` **NEW**; all new components + `UI/StatsDisplay.cs` **UPDATED** follow the pattern |
| §3.6 | Ink metadata tags → static events; double-entry sync of C# counters and Ink variable memory | `StoryManager.ProcessTags()` (tags → events) + `Managers/CreditSystem.cs` **NEW** (local counter, `OnInkSyncRequested` back into `story.variablesState`) |
| §3.8 | Security controls: try-catch loading pipeline, JSON schema validation, corrupt save → clean default profile | `SaveSystem.LoadOrDefault()` + `PassesSchemaValidation()` + `SaveData.IsValid()`; the healed default is re-written to disk |
| §3.9 | Privacy / GDPR alignment | all processing stays local; no telemetry, no network — preserved by design |

## Chapter 4 — Implementation and Results

| Thesis section | Technique | Implementation |
|---|---|---|
| §4.2 | Event-driven UI update pipeline (no per-frame polling for UI state) | `UI/PassometerDisplay.cs` **NEW**, `CreditSystem`, `StatsDisplay` — all repaint only on events |
| §4.2 | Time-of-day driven audio/lighting backdrops | `Managers/TimeManager.cs` **UPDATED** (scene-local, offset survives scenes) + existing Scene02 switching; `OnBackgroundCue`/`OnAudioCue` events for tag-driven cues |
| §4.4.1 | Main menu lifecycle & scene initialization (`MainMenu.cs`) | `Assets/Project/Scripts/MainMenu.cs` **NEW** — onboarding, configuration overlay, persistent storage reads, routing |
| §4.4.2 | Audio layers & volume slider abstracting (`VolumeSettings.cs`) | `Assets/Project/Scripts/VolumeSettings.cs` **NEW** — sliders → global volume channels (AudioMixer in dB, fallback `AudioListener.volume`) |
| §4.4.3 | Character-by-character printing with progression gating | already present (`TextCreator`); reused by `DialogueUI` (with internal fallback at the same 0.03 s pacing) |
| §4.5.1 | Integer-driven FSM + coroutine sequencing | already present (`eventPos` switch in scene scripts) |
| §4.5.2 | Serious-game academic decision loop; will-I-passometer; support options at critical motivation | `Story/Chapter1.ink` **NEW** (decision loop) + `Managers/WillIPassometer.cs` **NEW** (weighted index, FSM states, support unlock) |
| §1.3.2 / §2.3 / §2.7 | Credit-gated chapter progression with rollback & retry | `GameStateManager` — immutable snapshot at chapter start, threshold gate, credit rollback, scene reload rebuilds the Ink story from scratch |
| Appendix D | `gamesave.json` schema `{ PlayerName, CompletedChapters, TotalCredits }` | `SaveData` fields serialize to exactly those keys (superset adds the simulation metrics) |

## The will-I-passometer model (§2.5)

```
passometer = 0.40 · academic/100
           + 0.25 · attendanceRatio          (lectures attended / lectures held)
           + 0.20 · motivation/100
           + 0.15 · (1 − stress/100)
```

FSM states: **OnTrack** ≥ 0.65 > **AtRisk** ≥ 0.40 > **Critical**.
Support options unlock when `motivation ≤ 25` (§4.5.2). All weights and
thresholds are `[SerializeField]` and tunable in the Inspector.

## Metadata tag glossary (used in `.ink` files)

| Tag | Effect |
|---|---|
| `#speaker: Name` | dialogue name flag |
| `#credits: +10` | award/deduct credits → `OnCreditsAwarded` |
| `#stat: academic +8` | adjust social/academic/health/stress/motivation |
| `#stress: +5` / `#motivation: -10` | shorthand stat tags |
| `#attend` / `#skip` | attendance counters for the passometer |
| `#background: id` / `#audio: id` | environmental cues → `OnBackgroundCue` / `OnAudioCue` |
| `#chapter_end` | run the credit-threshold gate on the next advance |

## Design decisions & deviations (worth knowing for the defense)

1. **Immutability vs. Unity serialization.** `JsonUtility` cannot populate C#
   `readonly` fields, so `SaveData` enforces immutability *structurally*
   (private fields, zero setters, `With*()` constructors) rather than with the
   `readonly` keyword. Externally the guarantee is identical: mutation is a
   compile error.
2. **Appendix D key casing.** Backing fields are named in PascalCase so the
   JSON on disk matches the appendix key-for-key.
3. **Single-singleton without breaking scenes.** `PlayerData`, `StatSystem`
   and `TimeManager` keep their class names and `instance` accessors (existing
   scenes keep working) but no longer persist or own state — they delegate to
   `GameStateManager.CurrentSave`. They also self-create on demand, so any
   scene can be played directly in the editor.
4. **Immutable snapshot = free rollback.** Because `SaveData` can never be
   mutated, the §2.7 chapter snapshot is just a kept reference — no deep copy,
   no corruption risk across retries.
5. **Chapter gate timing.** `#chapter_end` defers evaluation until the player
   advances past the final line, so the fail/retry scene reload never cuts off
   the closing text.
6. **Input System fix.** The project runs with *Active Input Handling = Input
   System Package*; the legacy `Input.GetKeyDown` call in `PauseMenuUI` threw
   at runtime and was replaced with the matching backend (conditionally
   compiled). Auto-created EventSystems get `InputSystemUIInputModule`.
7. **Verified narrative balance.** `Chapter1.ink` compiles clean and was
   simulated end-to-end: diligent path 70/60 → pass; slacker path 15/60 →
   fail → retry; struggling path reaches critical motivation → counselor
   support unlocks. 70 credits obtainable, 60 required.
