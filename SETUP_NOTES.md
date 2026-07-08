# UNI-Verse — Setup Notes for the `thesis-techniques` Branch

Everything code-side is done; the steps below are the Unity-editor wiring that
cannot be automated from outside the editor. Total setup time: ~10 minutes.

## 0. First open

1. Pull the branch (or apply the patch) and open the project in Unity
   (the repo is on **6000.3.x**; the thesis references 2021.3 — both work with
   this code).
2. Unity will generate `.meta` files for the new scripts and the Ink plugin
   will auto-compile `Assets/Project/Story/Chapter1.ink` →
   **`Chapter1.json`** (same folder). Wait for the compile spinner to finish.
3. Console should be clean. (`Scene02_Event` previously imported
   `UnityEditorInternal`, which broke player builds — fixed.)

## 1. The new chapter scene ("Scene03" — the Ink-driven academic year)

This is the scene that demonstrates the decision loop, passometer, credits and
the retry mechanic. The components auto-build their UI, so the minimal version
is genuinely three steps:

1. **File → New Scene**, save as `Assets/Scenes/Scene03.unity`.
2. Create an empty GameObject `Narrative` and add these components:
   - **StoryManager** → drag `Assets/Project/Story/Chapter1.json` into
     *Ink Json Asset*;
   - **DialogueUI** (leave references empty → it builds the dialogue panel,
     continue button and choice list at runtime);
   - **CreditSystem** (auto-builds a top-left credits label);
   - **WillIPassometer**;
   - **PassometerDisplay** (auto-builds the top-right gauge).
3. **File → Build Settings → Add Open Scenes** so `Scene03` sits after
   `Scene02` (build index 4). `Scene02` already loads `"Scene03"` by name.

Optional polish: add a background sprite and two `AudioSource` objects, then
subscribe to `GameEvents.OnBackgroundCue` / `OnAudioCue` (cues used by the
story: `campus`, `lab`, `library`, `exam_hall` / `day`, `night`).

To watch the **retry mechanic (§2.7)**: play badly (skip the lecture, go for
coffee, take the easy week) — the year fails, credits roll back to the chapter
snapshot and the scene reloads with a fresh Ink story.

## 2. Scene01 — character name input (§3.5)

`Scene01_Event` already has the serialized fields; build the panel and assign:

1. Under the scene's Canvas create `NameInputPanel` (an `Image` panel):
   - a **TMP_InputField** (placeholder: "Type your name…");
   - a **Button** ("Confirm").
2. On the `Scene01` events object assign: *Name Input Panel* → the panel,
   *Name Input Field* → the input field, *Name Input Button* → the button.
   The confirm click is wired **in code** — no OnClick hookup needed.
3. The panel appears during the third dialogue beat; the confirmed name is
   written immutably to `gamesave.json` and used by Scene02's
   "I wonder where {name} is?" line and later by the Ink story
   (`player_name`).

If the panel is left unassigned the scene still runs (falls back to the saved
name), so nothing breaks meanwhile.

## 3. Main menu — MainMenu.cs + VolumeSettings.cs (§4.4.1–§4.4.2)

1. In `MainMenu.unity`, add the **MainMenu** component next to (or instead of)
   `StartPageUI` and assign: New Game / Continue / Settings / Quit buttons, an
   optional `saveInfoText` (shows "Name — Year N, X credits"), and a
   `settingsPanel` overlay with a Back button.
2. Inside the settings panel add three **Sliders** (Master / Music / SFX) and
   the **VolumeSettings** component; assign the sliders. Listeners are wired
   in code.
3. **AudioMixer (recommended):** Project window → Create → Audio Mixer, name
   it `MainMixer`; add `Music` and `SFX` groups; right-click each group's
   Volume in the Inspector → *Expose parameter*, then in the mixer's Exposed
   Parameters rename them to **`MasterVolume`**, **`MusicVolume`**,
   **`SFXVolume`**. Assign the mixer on VolumeSettings and route your
   `AudioSource`s to the groups. Without a mixer the Master slider still works
   through `AudioListener.volume`.

## 4. Save file & security demo (§3.4.2, §3.8)

- The save lives at `Application.persistentDataPath/gamesave.json`:
  - **Windows:** `%userprofile%\AppData\LocalLow\<company>\<product>\gamesave.json`
  - **macOS:** `~/Library/Application Support/<company>/<product>/gamesave.json`
- Tamper demo for the defense: break the JSON by hand, relaunch — the console
  logs the discarded corrupt save and a clean default profile is generated
  (and written back) instead of crashing.
- Old `PlayerPrefs` saves migrate to JSON automatically on first launch.

## 5. Notes & known follow-ups

- **HUD in old scenes:** you can drop `WillIPassometer` + `PassometerDisplay`
  + `CreditSystem` into Scene01/Scene02 too — they're scene-local and
  self-building.
- **Scene02 choice TODO:** the `// TODO: Add choice system here` beat can now
  be done two ways: quick C# (two buttons calling a public method) or — the
  thesis-aligned way — migrating Scene02's dialogue into an `.ink` file and
  reusing `StoryManager` + `DialogueUI`. Recommended as future work.
- **StartPageUI "LoadingScreen":** `StartPageUI` references a `LoadingScreen`
  scene that isn't in the build list (pre-existing). `MainMenu.cs` routes
  directly and doesn't need it.
- **GameStateManager tuning:** `creditsPerChapter` (default 60) and the
  pass/fail routing flags are Inspector fields on the component. Note the
  manager self-bootstraps before the first scene, so tune the defaults in
  `GameStateManager.cs` if you don't keep a scene-placed copy.
