# MapsExtended

Map editor mod for ROUNDS.

## Usage

You can access the editor from the game's main menu. Maps are saved to `path/to/ROUNDS/maps`. Maps are loaded from `path/to/ROUNDS/maps` and `path/to/ROUNDS/BepInEx/plugins`.

### Sharing maps

Since maps are loaded from the BepInEx plugin folder, you can easily share your maps via [Thunderstore](https://rounds.thunderstore.io/). See the [Thunderstore manifest docs](https://thunderstore.io/package/create/docs/) for instructions on how to create a valid manifest zip file.

Since custom maps must be loaded with this mod, you need to add it to your manifest's dependencies. Example `manifest.json`:

```json
{
    "name": "MyMaps",
    "version_number": "1.0.0",
    "website_url": "https://github.com/thunderstore-io",
    "description": "This is a description for a mod. 250 characters max",
    "dependencies": [
        "olavim-MapsExtended-<version>"
    ]
}
```

In the above example, you should change `"<version>"` with the latest version of MapsExtended.

## Development

Before building the project, copy and rename `Config.props.dist` to `Config.props` and change the `RoundsFolder` property inside it to match your ROUNDS installation path.

### Project structure

The mod has been split into two separate BepInEx plugins. The **MapsExtended** plugin consists of the core logic for loading custom maps. The **Editor** plugin adds the actual map editor to the game. Splitting the mod into two plugins allows for custom map mods to only depend on the core plugin: the editor is not needed if all you want to do is to play custom maps.

Folders:
- **MapsExtended**: The core plugin C# project
- **Editor**: The editor plugin C# project

## Custom map objects

See [the wiki](https://github.com/olavim/RoundsMapsExtended/wiki/Modding-quickstart) for a basic guide on how to add custom map objects in MapsExtended.
