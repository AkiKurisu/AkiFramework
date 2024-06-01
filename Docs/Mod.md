# Mod

Simple mod system based on Addressables. 

Editor part and runtime part can be separated, so you can build mod resources in another project.

## Runtime API

```C#
private async void LoadMod()
{
    List<ModInfo> modInfos = new();
    await new ModImporter(new ModSetting()).LoadAllModsAsync(modInfos); 
}
```
## Editor Export

Use Mod Exporter to create new addressable group and build only the mod group you edited.

You can inherit ``CustomBuilder`` and add it to export config to write mod additional meta data such as game assets sub catalog into `ModInfo` or make a pre-process such as looping the group's addressable entries.

## Build Notice

If you build mod in source project, you should add `DefaultBundleNamePatchBuilder` to export config for preventing bundle name conflict 

Reference https://docs.unity3d.com/Packages/com.unity.addressables@1.21/manual/MultiProject.html.
