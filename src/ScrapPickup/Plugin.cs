using System;
using System.IO;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExiledToolkit;
using ExiledToolkit.Game;
using HarmonyLib;
using ScrapPickup.Adapters;
using ScrapPickup.Core;

namespace ScrapPickup;

[BepInPlugin(Guid, "Scrap Pickup", Version)]
[BepInDependency(ToolkitGuid, ">=0.19.0")]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.scrappickup";
    private const string Version = "0.1.2";
    private const string ToolkitGuid = "com.dev.survivallog.exiledtoolkit";

    public override void Load()
    {
        // Read before the first Bind, which writes the file.
        var oldConfigText = ReadConfigText();

        var isOn = Config.Bind(OnSwitch.Section, OnSwitch.Key, OnSwitch.Default,
            "Turns Scrap Pickup on. When false, items from furniture you scrap or demolish drop the same way they do without the mod.\n" +
            "设为 true 时启用 Scrap Pickup。设为 false 时，回收或拆除家具得到的物品和没装本模组时一样掉落。");
        var useTrunk = Config.Bind("Scrap", "ToTrunk", true,
            "When true, items from furniture you scrap or demolish while exploring go to your trunk or shopping cart first, then your backpack. When false, or at home, they go to your backpack. They drop on the ground only when nothing has room.\n" +
            "设为 true 时，外出探索时回收或拆除家具得到的物品会先放进后备箱或购物车，再放进背包。设为 false 或在家时，物品会放进背包。只有都放不下时，物品才会掉在地上。");

        Log.LogInfo($"===== Scrap Pickup {Version} started");
        KeepOnAfterUpdate(oldConfigText, isOn);
        ScrapHooks.Apply(new PatchKit(new Harmony(Guid), Write), Log);
        LiveSettings.Watch(() => ScrapHooks.Use(new ScrapSettings(isOn.Value, useTrunk.Value)), isOn, useTrunk);
    }

    /// <summary>A player updating from a version without the on/off setting had the mod working, so it stays on.</summary>
    private void KeepOnAfterUpdate(string? oldConfigText, BepInEx.Configuration.ConfigEntry<bool> isOn)
    {
        if (!OnSwitch.IsUpdateFromOldVersion(oldConfigText)) return;

        isOn.Value = true;
        Log.LogInfo($"[{OnSwitch.Section}] {OnSwitch.Key} was not in the config file yet, so it is turned on for this update");
    }

    /// <summary>The config file as it was before this start, or null when there was none or it could not be read.</summary>
    private string? ReadConfigText()
    {
        try
        {
            return File.Exists(Config.ConfigFilePath) ? File.ReadAllText(Config.ConfigFilePath) : null;
        }
        catch (Exception ex)
        {
            Log.LogWarning($"could not read {Path.GetFileName(Config.ConfigFilePath)}, so [{OnSwitch.Section}] {OnSwitch.Key} keeps its default: {ex.Message}");

            return null;
        }
    }

    // A hook that could not go in is an error, so the Mod Manager can show the mod may need an update.
    private void Write(string message)
    {
        if (HookFailure.Is(message)) Log.LogError(message);
        else Log.LogInfo(message);
    }
}
