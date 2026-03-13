# UNI-Verse

A **narrative-driven interactive visual novel engine** built in **Unity 6.0** as a Bachelor's degree thesis project. UNI-Verse demonstrates a complete framework for choice-based storytelling with character animations, branching dialogue, and cinematic scene choreography.

## Overview

UNI-Verse is a visual novel that follows a character entering "UNI-Versity" with a rich narrative system powered by the **Ink scripting language**. The engine implements professional text rendering, character expression systems, and procedural event choreography for immersive interactive storytelling.

## Features

### 🎮 Core Systems
- **Ink Narrative Engine**: Professional story scripting with branching dialogue and player choices
- **Typewriter Text Effect**: Dynamic character-by-character text revelation (configurable timing)
- **Scene Choreography**: Code-driven event sequencing with synchronized animations, transitions, and audio
- **Character Expression System**: Multi-state emotional expressions for narrative depth

### 🎨 Visual Elements
- **Character Animations**: Fade-in, slide, and blend transitions
- **Expression System**: 50+ unique facial expressions across characters (Eyes Open, Eyes Closed Happy, Eyes Closed Emotional)
- **Background Imagery**: Professional visual novel backgrounds
- **2D Rendering**: Universal Render Pipeline (URP) optimized for 2D visuals

### 🔊 Audio System
- **Background Music**: Adaptive ambient music tracks
- **Sound Effects**: Character vocalizations (sighs, gasps, dialogue cues)
- **AudioSource Integration**: Dynamic audio playback control


## Project Structure

```
Assets/
├── Project/
│   ├── Scripts/
│   │   ├── TextCreator.cs           # Typewriter text rolling effect system
│   │   └── Scene01/
│   │       └── scene01_Events.cs    # Event choreography and scene flow
│   └── Story/
│       ├── Test.ink                 # Narrative script (choice-based)
│       └── Test.json                # Compiled Ink output
├── Characters/
│   ├── Char_Haruka/                 # Character sprites and expressions
│   └── Char_Kasumi/                 # Character sprites and expressions
├── Animations/                       # Animation controllers and clips
├── Audio/
│   ├── Music/                        # Background tracks
│   └── Effects/                      # Sound effects
├── BackgroupIMG/                     # Background images
├── Scenes/
│   └── SampleScene.unity             # Demo scene with narrative flow
└── Settings/                         # Render pipeline & project configuration
```

## Technologies & Dependencies

- **Engine**: Unity 6.0 (6000.3.11f1)
- **Narrative System**: Ink & Yarn Spinner integration
- **Rendering**: Universal Render Pipeline (URP) 17.3.0
- **UI/Text**: TextMesh Pro
- **Input**: Input System 1.19.0
- **2D**: Aseprite, 2D Animation, Sprite, Tilemap
- **Development**: Visual Studio, Rider IDE integration
- **Testing**: Unity Test Framework 1.6.0

## Getting Started

### Setup
1. Clone or open the project in Unity Hub (v6.0+)
2. Allow Unity to auto-import all packages from `Packages/manifest.json`
3. Navigate to `Assets/Scenes/SampleScene.unity` to view the demo
4. Press **Play** in the editor to experience the visual novel demo

### Playing the Story
- The demo scene (`SampleScene.unity`) launches with character introductions
- Make narrative choices when prompted
- Watch as the story branches based on your decisions
- Characters animate and react to dialogue with synchronized audio

## Key Implementation Details

### Text System (TextCreator.cs)
- **Typewriter Effect**: 0.03 seconds per character reveal
- **Configurable Timing**: Adjust text speed through component properties
- **TextMesh Pro Integration**: Professional font rendering and styling

### Scene Events (scene01_Events.cs)
- **Coroutine-Based Flow**: Orchestrates character animations, fade transitions, and dialogue timing
- **Sequential Events**: Choreographed entrance/exit animations with audio cues
- **Synchronization**: Precise timing between text, animation, and audio

### Ink Story System
- **Branching Narrative**: Player choices affect story progression
- **Compiled Output**: `.ink` files auto-compiled to `.json` for runtime
- **Dynamic Dialogue**: Full support for variable tracking and conditional branching


### Current Status
- Narrative engine fully integrated
- Character expression system implemented
- Scene choreography framework complete
- Audio system functional
- Story content in early development 

## Thesis Focus

This project demonstrates:
1. **Scalable Visual Novel Architecture** in Unity
2. **Integration of Professional Narrative Tools** (Ink) with game mechanics
3. **Dynamic Character Expression Systems** for emotional storytelling
4. **Procedural Scene Choreography** via code-driven event timing
5. **Text Rendering Enhancements** for immersive dialogue presentation


