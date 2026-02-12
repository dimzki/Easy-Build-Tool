# Easy Build Tool

A Unity Editor build tool package with platform selection, scene management, semantic versioning, and ZIP output support.

## Requirements

- Unity 6000.3+

## Installation

### Via Git URL

In Unity, open **Window > Package Manager**, click **+** > **Add package from git URL**, and enter:

```
https://github.com/dimzki/Easy-Build-Tool.git
```

### Via manifest.json

Add this line to your `Packages/manifest.json` under `dependencies`:

```json
"com.alzaki.easybuildtool": "https://github.com/dimzki/Easy-Build-Tool.git"
```

## Usage

Open the tool from the menu: **Build > Build Window**

### Platform Target

Select a single build target:
- **Windows** (.exe)
- **MacOS** (.app)
- **Android APK** (.apk)
- **Android AAB** (.aab)
- **iOS** (Xcode project)

### Scripting Backend

Choose between **Mono** or **IL2CPP**.

### Development Build

Toggle development build on or off.

### Scene Selection

Manage which scenes are included in the build:
- Add scenes from anywhere in the project via the **Add Scene** popup
- Enable/disable individual scenes with checkboxes
- Reorder scenes using the up/down arrow buttons
- Remove scenes with the X button
- Build index is assigned automatically based on enabled scene order

### Version Control

Semantic versioning (Major.Minor.Hotfix) with the following rules:
- **Last Version** displays the version of the last successful build
- **New Version** displays the version being configured
- Build buttons are disabled when Last Version equals New Version
- New Version cannot be lower than Last Version
- Use **+**, **-**, and **reset** buttons to adjust each version component
- Version is stored in `Assets/Resources/BuildConfig.ini`

### Build Output

Build files are output to `[ProjectRoot]/Builds/[Platform]/` with the naming format:

```
[ProductName]_[Version]_[CommitHash]_[dd-MM-yyyy]
```

- **CommitHash** is the short git commit hash (included only if the project uses git)
- **Build Normally** runs a standard Unity build
- **Build as ZIP** builds and compresses the output into a `.zip` file
- Explorer opens to the build folder after a successful build

## Package Structure

```
com.dimzki.easybuildtool/
├── package.json
├── README.md
├── Runtime/
│   ├── SemanticVersion.cs       # Version struct with parsing and comparison
│   └── ConfigHandler.cs         # INI file read/write for version persistence
└── Editor/
    ├── BuildToolWindow.cs       # Main editor window
    ├── BuildExecutor.cs         # Build pipeline and ZIP compression
    ├── BuildSettingsData.cs     # Settings data and platform mappings
    ├── VersionManager.cs        # Version increment/decrement/reset logic
    ├── SceneSelectionPopup.cs   # Scene picker popup window
    ├── ConfirmBuildPopup.cs     # Build confirmation dialog
    └── GitHelper.cs             # Git commit hash detection
```

## License

MIT
