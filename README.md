# Driver-Model-Swap-Tool

A simple C# tool for replacing driver models in Assetto Corsa by swapping KN5 files throughout the game's driver folders.

## Features
- Automatically finds your Assetto Corsa installation using Steam, the Windows registry, or common install locations.
- Replaces driver models in:
  - `content/driver`
  - `content/cars/*/driver`
  - `content/cars/*/drivers`
- Choose a custom KN5 file using the standard Windows file picker.
- Updates car driver folders in the background so the UI stays responsive.
- Creates a `driver-swap-log.txt` file for troubleshooting.
- Available as a single self-contained executable.

## How It Works
1. Finds your Assetto Corsa `content` folder.
2. Copies itself into the `content` folder if it isn't already there.
3. Prompts you to select a custom driver KN5 file.
4. Replaces all supported driver KN5 files in:
   - `content/driver`
   - `content/cars/*/driver`
   - `content/cars/*/drivers`

## Requirements
- Windows
- Assetto Corsa (Steam version)
- No additional installation required when using the self-contained release

## Usage
1. Run the tool as Administrator (it will request elevation if needed).
2. Select the driver KN5 file you want to use.
3. Wait for the replacement process to finish.
4. Launch Assetto Corsa.

## Building from Source

```bash
git clone https://github.com/zr1z/Driver-Model-Swap-Tool.git
cd Driver-Model-Swap-Tool
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

## Contributing
Pull requests are welcome. If you find a bug, have an improvement, or want to add support for something else, feel free to open an issue or submit a PR. Please try and keep the same code style, this makes it easier to review and publish.
