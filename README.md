# Kinect Violin

A Windows desktop application that lets you play a virtual violin using **Kinect v2** body tracking, custom hand-hover buttons, and **MIDI** output.

The project extends the [Kinect v2 WPF Custom Button](https://github.com/dehariapankaj/WPFKinectV2CustomButton) demo pattern: buttons respond to hand pointer enter/leave events instead of mouse clicks. Combined with Visual Gesture Builder (VGB) gesture detection, hovering over a string/fret button and performing a bowing gesture sends MIDI notes to a virtual instrument.

![Kinect Violin running interface](Kinect_Violin/Images/app-screenshot.jpg)

*The virtual violin UI: hover a hand over note buttons (G/D/A/E strings) to select pitch, choose an instrument at the top right, then perform a bowing gesture to play via MIDI.*

## What It Does

1. **Hand-hover UI** — 16 fret buttons (G/D/A/E strings) and 4 instrument selectors (Violin, Ensemble, Cello, Bass) inside a `KinectRegion`.
2. **Gesture recognition** — Detects the custom `PlayString` bowing gesture from a VGB database (`.gba` files in `Database/`).
3. **MIDI output** — Sends `ProgramChange`, `NoteOn`, and `NoteOff` messages via [RtMidi.Core](https://www.nuget.org/packages/RtMidi.Core) to the first available MIDI output device.
4. **Body visualization** — Draws tracked skeleton joints in the UI (`KinectBodyView`).

### Typical Interaction Flow

```
Leave a button once (arms the session)
  → Hover over an instrument button to switch sound (Violin / Cello / …)
  → Hover over a note button (G0–E3)
  → Perform the PlayString bowing gesture
  → MIDI note plays; gesture ends or hand leaves → note stops
```

## Technical Foundations

This project sits at the intersection of **computer vision**, **human–computer interaction**, and **digital music synthesis**. The app does not implement low-level sensor drivers; instead it composes several mathematical pipelines provided by the Kinect SDK into a playable virtual instrument.

### 1. Depth Sensing & Coordinate Projection

Kinect v2 fuses an **IR time-of-flight depth camera** with a color camera. Each tracked body joint is reported in **camera space** — a right-handed 3D coordinate system where the sensor is the origin, \(Z\) points toward the user, and units are meters:

\[
\mathbf{p}_{\text{camera}} = (X,\ Y,\ Z)
\]

To draw the skeleton on screen, `KinectBodyView` projects each joint into **depth image space** (2D pixel coordinates) via the SDK's `CoordinateMapper`:

\[
(x_d,\ y_d) = \Pi\!\left(\mathbf{p}_{\text{camera}}\right)
\]

where \(\Pi\) encapsulates the Kinect's calibrated **pinhole camera model** (focal length, principal point, lens distortion). Inferred joints occasionally report \(Z < 0\); the renderer clamps \(Z \leftarrow \max(Z,\ 0.1\ \text{m})\) to avoid singular projections that would map to \((-\infty,\ -\infty)\).

### 2. Skeletal Kinematics

The SDK tracks up to six bodies, each modeled as a **kinematic tree** of 25 joints connected by rigid **bones**. The visualization treats the skeleton as a graph \(G = (V, E)\):

- **Vertices** \(V\): joint positions in depth space
- **Edges** \(E\): anatomical bone pairs (e.g. `ShoulderRight → ElbowRight → WristRight`)

Each joint carries a **tracking state** \(\sigma \in \{\text{Tracked},\ \text{Inferred},\ \text{NotTracked}\}\). A bone is drawn as a solid segment only when both endpoints are Tracked; otherwise it is rendered as an inferred (dashed) connection — a simple confidence heuristic over the kinematic chain.

### 3. Hand-Pointer Projection & Hit Testing

Hover detection does not use raw depth pixels. The Kinect WPF input stack provides a **hand pointer** in normalized screen coordinates:

\[
\mathbf{u} = (u_x,\ u_y) \in [0, 1]^2
\]

`KinectV2CustomButtonController` maps this to pixel space and performs an **axis-aligned bounding box (AABB)** hit test against each button:

\[
\begin{aligned}
\mathbf{p}_{\text{region}} &= (u_x \cdot W_{\text{region}},\ u_y \cdot H_{\text{region}}) \\
\mathbf{p}_{\text{button}} &= T_{\text{region}\rightarrow\text{button}}(\mathbf{p}_{\text{region}}) \\
\text{inside} &\Leftrightarrow 0 \le p_x < W_{\text{button}} \ \land\ 0 \le p_y < H_{\text{button}}
\end{aligned}
\]

where \(T\) is the WPF affine coordinate transform (`TranslatePoint`). Enter/leave events fire on **state transitions** of the `inside` predicate, giving hysteresis-free hover semantics without mouse hardware.

### 4. Gesture Recognition (Visual Gesture Builder)

The `PlayString` bowing gesture is not hard-coded. It is a **discrete classifier** trained offline in Visual Gesture Builder and shipped as `Database/PlayString.gba`. At runtime, VGB evaluates the live skeleton against an ensemble of **AdaBoost decision stumps** trained on skeletal feature vectors (joint angles, relative displacements, and temporal derivatives across frames).

Each frame yields a detection flag and a **confidence** \(c \in [0, 1]\):

\[
\text{PlayString}(t) = \mathbb{1}\left[\sum_k \alpha_k h_k(\mathbf{x}_t) \ge \theta\right], \quad c = \sigma(\cdot)
\]

where \(\mathbf{x}_t\) is the skeletal state at time \(t\), \(h_k\) are weak learners, and \(\alpha_k\) are learned weights. `GestureDetector` forwards \((\text{detected},\ c)\) to `ViolinSessionController`, which treats gesture onset/offset as a **temporal gate** for MIDI output.

### 5. Music Theory & MIDI Pitch Mapping

Note buttons map to **MIDI note numbers** under **12-tone equal temperament**, where each semitone corresponds to a frequency ratio of \(2^{1/12}\):

\[
f(n) = 440\ \text{Hz} \cdot 2^{(n - 69)/12}
\]

The four violin strings (G, D, A, E) are laid out as ascending semitone grids in `MusicCatalog`:

| String | Open string (MIDI) | Frets G0–G3 / D0–D3 / … |
|--------|-------------------|-------------------------|
| G | 55 (G₃, ≈ 196 Hz) | +2 semitones per fret |
| D | 62 (D₄, ≈ 293 Hz) | |
| A | 69 (A₄, 440 Hz) | |
| E | 76 (E₅, ≈ 659 Hz) | |

Instrument presets apply a **semitone transposition** via pitch offset before `NoteOn` / `NoteOff`:

\[
n_{\text{out}} = n_{\text{base}} + \Delta_{\text{pitch}}
\]

For example, Cello uses \(\Delta = -20\) (just under two octaves down), emulating a lower-register timbre on the same physical gesture. `ProgramChange` selects the General MIDI instrument patch independently of pitch.

### 6. Session State Machine

Whether a note actually sounds is governed by a small **finite state machine** in `ViolinSessionController`, not by gesture detection alone. All of the following must hold:

\[
\text{play} \Leftrightarrow \underbrace{\text{armed}}_{\text{left a button once}} \ \land\ \underbrace{\text{hover}(\text{note})}_{\text{AABB hit}} \ \land\ \underbrace{\text{PlayString}}_{\text{VGB gate}}
\]

Leaving a button **arms** the session; hovering selects pitch; the bowing gesture acts as a **continuous controller** that opens the MIDI gate. Gesture end or hand leave sends `NoteOff`, completing the attack–release envelope at the synthesis layer.

Together, these layers transform raw depth frames into musical output: **3D sensing → 2D projection → kinematic tracking → learned gesture classification → tempered pitch mapping → MIDI**.

## Requirements

### Hardware

| Item | Notes |
|------|-------|
| **Kinect for Windows v2** | Sensor + power hub + USB 3.0 port |
| **Windows PC** | 64-bit Windows 10/11 recommended |
| **MIDI output device** | Recommended: [loopMIDI](https://www.tobias-erichsen.de/software/loopmidi.html). The app launches without one, but produces no sound |

### Software

| Item | Version |
|------|---------|
| **.NET Framework** | 4.7.2 |
| **Visual Studio** | 2019 or 2022 (with .NET desktop development workload) |
| **Kinect for Windows SDK** | 2.0 (v2.0_1409) — **required for runtime** |

> **Important:** Kinect managed DLLs in this project are **x86 (32-bit)**. Always build with the **x86** platform. Building as x64 will cause a `BadImageFormatException` at startup.

## Dependencies

### NuGet Packages (restored automatically)

| Package | Version | Purpose |
|---------|---------|---------|
| [Microsoft.Kinect.VisualGestureBuilder](https://www.nuget.org/packages/Microsoft.Kinect.VisualGestureBuilder) | 2.0.1410.19000 | Custom gesture recognition |
| [RtMidi.Core](https://www.nuget.org/packages/RtMidi.Core) | 1.0.50 | MIDI output |

### Bundled References (`Kinect_Violin/References/`)

| DLL | Purpose |
|-----|---------|
| `Microsoft.Kinect.dll` | Kinect body tracking API |
| `Microsoft.Kinect.Wpf.Controls.dll` | `KinectRegion`, hand pointer controls |

### External Downloads (install manually)

| Software | Download |
|----------|----------|
| **Kinect for Windows SDK 2.0** | [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=44561) |
| **loopMIDI** (recommended for sound) | [tobias-erichsen.de/software/loopmidi.html](https://www.tobias-erichsen.de/software/loopmidi.html) |
| **NuGet CLI** (if `nuget restore` is needed) | [dist.nuget.org/win-x86-commandline/latest/nuget.exe](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe) |

Install the Kinect SDK **before** running the app, with the sensor **unplugged**. Plug in the Kinect after installation completes.

## Getting Started

### 1. Clone the repository

```powershell
git clone https://github.com/markkuo1999/Kinect-V2-Virtual-Violin.git
cd Kinect-V2-Virtual-Violin/Kinect_Violin
```

### 2. Restore NuGet packages

If the `packages/` folder is missing:

```powershell
# Download nuget.exe to the project folder if you don't have it
Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile "nuget.exe"

.\nuget.exe restore WpfKinectV2CustomButton.sln
```

### 3. Install Kinect SDK 2.0

Download and run `KinectSDK-v2.0_1409-Setup.exe` from the [Microsoft Download Center](https://www.microsoft.com/en-us/download/details.aspx?id=44561).

### 4. Build (x86 only)

**Command line:**

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" WpfKinectV2CustomButton.sln /p:Configuration=Debug /p:Platform=x86
```

Adjust the MSBuild path if you use a different Visual Studio edition.

**Visual Studio:**

1. Open `Kinect_Violin/WpfKinectV2CustomButton.sln`
2. Set configuration to **Debug | x86**
3. Build → Build Solution (`Ctrl+Shift+B`)

### 5. Run

```powershell
.\bin\x86\Debug\WpfKinectV2CustomButton.exe
```

Or press **F5** in Visual Studio with **Debug | x86** selected.

## Architecture

The app is organized into three layers:

```
Kinect / Hand Pointer events
        ↓
MainWindow (UI wiring only)
        ↓
ViolinSessionController  ← decides when to play / stop
        ↓
MidiService            ← sends NoteOn / NoteOff / ProgramChange
        ↓
MusicCatalog           ← note & instrument lookup tables
```

| Layer | Key files | Responsibility |
|-------|-----------|----------------|
| **UI** | `MainWindow.xaml.cs`, `KinectV2CustomButton*` | Display buttons, forward Kinect events |
| **Services** | `ViolinSessionController`, `MidiService`, `KinectTrackingService` | Session state, MIDI I/O, body/gesture tracking |
| **Models** | `MusicCatalog`, `InstrumentConfig`, `NoteButtonDefinition` | Note mappings, instrument presets |

### How MIDI Is Triggered

1. `KinectTrackingService` detects the `PlayString` gesture via `GestureDetector` → `GestureResultView`.
2. `MainWindow` forwards hand-hover and gesture events to `ViolinSessionController`.
3. `ViolinSessionController` calls `MidiService.PlayNote()` / `StopNote()` when:
   - the session is **armed** (user has left a button at least once),
   - a **note button** is hovered, and
   - the **bowing gesture** is active.
4. Instrument buttons call `MidiService.SetInstrument()` to switch MIDI program and pitch offset.

To change note mappings, edit `Models/MusicCatalog.cs`. To change play conditions, edit `Services/ViolinSessionController.cs`.

## Project Structure

```
Kinect_Violin/
├── Models/
│   ├── MusicCatalog.cs             # Note & instrument lookup tables
│   ├── InstrumentConfig.cs
│   ├── NoteButtonDefinition.cs
│   └── GestureStateChangedEventArgs.cs
├── Services/
│   ├── ViolinSessionController.cs  # Play/stop decision logic
│   ├── MidiService.cs              # MIDI device I/O
│   ├── KinectTrackingService.cs    # Body frames + gesture detectors
│   └── ButtonBrushFactory.cs       # Cached button image brushes
├── MainWindow.xaml / .cs           # UI wiring and button creation
├── KinectV2CustomButton.cs         # Custom Kinect-aware button control
├── KinectV2CustomButtonController.cs  # Hand pointer enter/leave detection
├── GestureDetector.cs              # VGB gesture detection per body
├── GestureResultView.cs            # Gesture state display & events
├── KinectBodyView.cs               # Skeleton rendering
├── Database/                       # VGB gesture databases (.gba, .gbd)
├── Images/                         # UI assets (violin icons, backgrounds)
├── References/                     # Kinect SDK managed DLLs
└── packages/                       # NuGet packages (after restore)
```

## Button Layout

### String / Fret Buttons (16)

| String | Buttons | MIDI Keys |
|--------|---------|-----------|
| G | G0–G3 | 55–60 |
| D | D0–D3 | 62–67 |
| A | A0–A3 | 69–74 |
| E | E0–E3 | 76–81 |

### Instrument Selectors (4)

| Button | MIDI Program | Pitch Offset |
|--------|--------------|--------------|
| Violin | 40 | 0 |
| Ensem | 49 | 0 |
| Cello | 42 | −20 |
| Bass | 43 | −32 |

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `BadImageFormatException` on startup | Build with **x86**, not x64 or Any CPU |
| `VisualGestureBuilder` not found | Run `nuget restore`; ensure `packages/` exists |
| App crashes when window loads | Ensure `Images/` is copied to output (handled by post-build step) |
| App opens but no sound | Install loopMIDI (or another virtual MIDI port); route MIDI to a synth or DAW |
| App shows "No MIDI output device found" | Expected without loopMIDI — install it to enable sound |
| Kinect not detected | Install Kinect SDK 2.0; use USB 3.0; check Device Manager for "KinectSensor Device" |
| Gesture not recognized | Stand in sensor range; ensure `Database/PlayString.gba` is present in output folder |

## License

This project is based on Microsoft Kinect SDK sample patterns. Refer to the [Kinect for Windows SDK 2.0 license terms](https://www.microsoft.com/en-us/download/details.aspx?id=44561) for SDK usage.
