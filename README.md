
# Subnautica-Echelon
A mod that adds the Echelon submersible to the Subnautica game

## Requirements
- Tobey's BepInEx Pack for Subnautica (https://www.nexusmods.com/subnautica/mods/1108)
- Vehicle Framework (https://www.nexusmods.com/subnautica/mods/859)
- Unity Editor version 2019.4.36f with Mac build support
- Visual Studio 2019+ (2022 is current at the time of writing) Community+
- .NET 4.7.2 developer pack (https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)
- KriptoFX (https://assetstore.unity.com/packages/vfx/particles/fire-explosions/realistic-explosions-pack-54783)
If you do not want to license this, you will need to revamp the Explosion/Explosion prefab to produce convincing visuals. One of its shaders is also used to the drive exhaust so you will have to also fix that.
The project expects those to be located in the KriptoFX subdirectory.

## Project Composition
The project is split in two: There is a unity project in the [clone]\EchelonUnity subdirectory. If should be opened with the correct Unity editor. The second directory, [clone]\Plugin, contains the actual plugin which is loaded by BepInEx.
While the Unity project should build (and run) once you fixed the explosion issue (see requirements), the Plugin likely will not.
See dependencies below. Since the plugin references the DLL produced by Unity, you will need to build those at least once before fixing the dependency issues.

## Building via Unity
1) Build assets via Unity: Menu -> Assets -> Build AssetBundles
2) Build DLLs via Unity: Menu -> File -> Build Settings ... -> Build 
(Then pick a folder that is not in the clone directory, ideally 'BuildTarget' next to [clone].  This will be called [build] from here)

## Plugin/Subnautica Echelon Project Dependencies and Building
The plugin needs the following DLLs to be referenced in the Plugin/Subnautica Echelon project:
- [build]\Subnautica Echelon_Data\Managed\EchelonScripts.dll
- [Subnautica]\BepInEx\core\0Harmony.dll
- [Subnautica]\BepInEx\core\BepInEx.dll
- [Subnautica]\BepInEx\plugins\Nautilus\Nautilus.dll
- [Subnautica]\BepInEx\plugins\VehicleFramework\VehicleFramework.dll
- [Subnautica]\Subnautica_Data\Managed\Assembly-CSharp.dll
- [Subnautica]\Subnautica_Data\Managed\Assembly-CSharp-firstpass.dll
- [Subnautica]\Subnautica_Data\Managed\FMODUnity.dll
- [Subnautica]\Subnautica_Data\Managed\UnityEngine.dll
- [Subnautica]\Subnautica_Data\Managed\UnityEngine.AssetBundleModule.dll
- [Subnautica]\Subnautica_Data\Managed\UnityEngine.AudioModule.dll
- [Subnautica]\Subnautica_Data\Managed\UnityEngine.CoreModule.dll
- [Subnautica]\Subnautica_Data\Managed\UnityEngine.InputLegacyModule.dll
- [Subnautica]\Subnautica_Data\Managed\UnityEngine.PhysicsModule.dll

Once set up, the project should build.
Compile Subnautica Echelon project for **release**. It cannot be run outside Subnautica. That should produce the DLL we need in [clone]\Plugin\bin\Release\net4.7.2\Subnautica Echelon.dll

## Assembly
The target mod directory should be in [Subnautica]\BepInEx\plugins\Echelon.
In order to run the mod, you need to copy the following files directly into that directory (no subdirectories):
1) [clone]\EchelonUnity\Assets\AssetBundles\OSX\echelon -> (rename to) echelon.osx
2) [clone]\EchelonUnity\Assets\AssetBundles\Windows\echelon
3) [clone]\Plugin\bin\Release\net4.7.2\Subnautica Echelon.dll
4) [build]\Subnautica Echelon_Data\Managed\EchelonScripts.dll

Also copy these entire directories:
1) [clone]\images
2) [clone]\Localization

If you intend to frequently change things, you should probably adapt the scripts in [clone]\Scripts to your needs.

## Notes about the recipes
The current version of vehicle framework (1.6.1) does not register changes in the build recipe of the craft.
A workaround has been implemented by the plugin itself which deletes Echelon recipes on launch if it recognizes them.
Otherwise, you may have to manually delete [Subnautica]\BepInEx\plugins\VehicleFramework\recipes\Echelon_recipe.json before the next game start

## Start
Once copied, the game should pick up the mod automatically. To check if everything went fine, check 
[Subnautica]\BepInEx\LogOutput.log
It should contain the following messages:
- [Info   :VehicleFramework] The Echelon is beginning Registration.
- [Info   :VehicleFramework] Finished Echelon registration.

It should be possible to build the craft via a Mobile Vehicle Bay. Alternatively 'spawn echelon' should also do the trick.


