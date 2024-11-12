# Voxel Thing

Work-in-progress C# port of a currently barebones clone of a really popular block game.

For contributors: currently not accepting new features until all of the bugs of the port have been ironed out and performance is on par with the Java version.

Yeah, I'm releasing this early because I'm tired and I want to see if anyone else can find bugs in the code.

## Controls
- WASD: Fly around
- Move mouse: Look around
- Space: Jump
- Double Space: Fly (Permanently enabled atm, collision is buggy)
- Shift: Fly down
- 1-9: Select block
- Left click: Place block
- Right click: Break block
- E: Inventory
- Q: No-clip (Permanently enabled atm, collision is buggy)
- R: Teleport to a random position
- F1: Toggle GUI
- F3: Debug menu
- F4: Toggle profiler display
- F5: Toggle third person
- Escape: Open pause menu

## Play
Compiled binaries for the C# port of Voxel Thing are currently unavailable. Once the initial port has been finalized, CI will be set up. .NET 8+ runtime is required.

## Compiling
To compile Voxel Thing, simply run `dotnet build`. .NET 8+ SDK is required. The resulting executable can be found in `VoxelThing.Client/bin/Debug/net8.0`.
