# Scrap Pickup

Items from furniture you scrap or demolish go into your trunk or backpack instead of falling on the ground.

In Survival Log, demolishing furniture drops its materials on the floor, and scrapping it while your backpack is full does the same. Scrap Pickup puts them away for you.

## Features

- **Trunk first while exploring**: with a shopping cart or car, materials go to its trunk first, then your backpack.
- **Backpack at home**: the trunk is not with you at home, so materials go to your backpack.
- **Ground only as a last resort**: items drop on the ground only when the trunk and the backpack have no room.
- **Both ways of removing furniture**: works for Scrap and for Demolish.

The backpack counts as full when it has no free slot or when the item would put you over your carry weight, the same check the game makes when you pick something up. Only furniture you remove yourself is affected.

Scrap Pickup adds nothing to your save and does not edit game files. You can remove it at any time.

## Requirements

- Survival Log 1.1.19611. Built for this version; a game update can break it until the mod is updated.
- The [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12), the BepInEx 6 (IL2CPP) build for the game.
- Optional: [Exiled's Toolkit](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit) 0.4.0 or newer. With it, settings changed on the Mod Manager's Mods screen take effect without restarting the game. Without it, they take effect at the next game start.

## Install

1. Install the [BepInEx Pack for Survival Log](https://www.nexusmods.com/survivallog/mods/12) (if no other mods were installed before, start the game once so BepInEx finishes setup, then quit).
2. Optional, to change settings without a restart: install Exiled's Toolkit. Download `ExiledToolkit-x.y.z.zip` from [its latest release](https://github.com/AnExiledDev/SurvivalLog-ExiledToolkit/releases/latest) or from [Nexus Mods](https://www.nexusmods.com/survivallog/mods/61), and extract it into the game folder (the folder with `SurvivalLog.exe`).
3. Download `ScrapPickup-x.y.z.zip` from the [latest release](https://github.com/AnExiledDev/SurvivalLog-ScrapPickup/releases/latest).
4. Extract the zip into the game folder (the folder with `SurvivalLog.exe`). It holds a `BepInEx` folder, so the DLL lands in the right place. When asked, merge the folders. The result should be:
   `C:\Program Files (x86)\Steam\steamapps\common\Survival Log\BepInEx\plugins\ScrapPickup.dll`

## Uninstall

Delete `ScrapPickup.dll` from the `BepInEx\plugins` folder. For a clean slate, also delete `BepInEx\config\com.dev.survivallog.scrappickup.cfg`.

## Configuration

Settings live in `BepInEx\config\com.dev.survivallog.scrappickup.cfg`, created the first time the game starts with the mod. Change them in game on the Mod Manager's Mods screen, where they take effect at once, or edit the file while the game is closed. See [CONFIG.md](CONFIG.md) for every setting. Without Exiled's Toolkit installed, changes made on the Mods screen take effect at the next game start instead.

## Troubleshooting

Built against BepInEx `6.0.0-be.788`.

Check `BepInEx\LogOutput.log` for `===== Scrap Pickup 0.1.0 started`, followed by lines starting with `hooked`. A line starting with `could not hook` means part of the mod does not work on this game version, and that part works as in the unmodded game.

## Known issues

- Items stored inside a piece of furniture (a cabinet's contents, for example) still drop on the ground when you demolish it. Only the materials the furniture breaks down into are put away.

## Feedback and bug reports

Found a bug, have an idea, or something feels off? Come say so on the Core Directive Discord: https://discord.gg/trTsyxUFqg . You can also post in the Posts tab of the mod's Nexus Mods page. When reporting a bug, attach `BepInEx\LogOutput.log`.

To reach me directly, message `exiled.dev` on Discord; you can find me in the [official Survival Log Discord](https://discord.gg/MZGg4gQAUe). I'm also on X as [@AnExiledDev](https://x.com/AnExiledDev).

A Survival Log wiki with detailed documentation for all of my mods is coming soon.

## Build

This is a BepInEx 6 IL2CPP plugin. It compiles against BepInEx and the game's IL2CPP interop assemblies, so a game install with BepInEx set up and started once is required. Those assemblies come from the game and are not part of this repo. The .NET 8 SDK is required.

```
dotnet build src/ScrapPickup -c Release -p:GameDir="C:\path\to\Survival Log"
```

`GameDir` is the game folder. The default in the project file is a local path you will almost certainly need to override. The output DLL is at `src\ScrapPickup\bin\Release\ScrapPickup.dll`.

The unit tests cover the pure routing rules and need nothing from the game:

```
dotnet test tests/ScrapPickup.Tests
```

## License

MIT. Copyright (c) 2026 AnExiledDev. See [LICENSE](LICENSE).
