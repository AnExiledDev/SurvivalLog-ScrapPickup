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
    private const string Version = "0.1.4";
    private const string ToolkitGuid = "com.dev.survivallog.exiledtoolkit";

    public override void Load()
    {
        var isOn = Config.Bind("Scrap", "Enabled", ScrapSettings.Default.IsOn,
            "Turns Scrap Pickup on and off. When false, items from furniture you scrap or demolish drop like they do without the mod.\n" +
            "设为 true 时启用 Scrap Pickup。设为 false 时，回收或拆除家具得到的物品和没装本模组时一样掉在地上。");
        var useTrunk = Config.Bind("Scrap", "ToTrunk", ScrapSettings.Default.UseTrunk,
            "When true, items from furniture you scrap or demolish while exploring go to your trunk or shopping cart first, then your backpack. When false, or when you are at home, items go to your backpack. Items drop on the ground only when nothing has room.\n" +
            "设为 true 时，外出探索时回收或拆除家具得到的物品会先放进后备箱或购物车，再放进背包。设为 false 或在家时，物品会放进背包。只有都放不下时，物品才会掉在地上。");

        Log.LogInfo($"===== Scrap Pickup {Version} started");
        ScrapHooks.Apply(new PatchKit(new Harmony(Guid), Write), Log);
        LiveSettings.Watch(() => ScrapHooks.Use(new ScrapSettings(isOn.Value, useTrunk.Value)), isOn, useTrunk);
    }

    // A hook that could not go in is an error, so the Mod Manager can show the mod may need an update.
    private void Write(string message)
    {
        if (HookFailure.Is(message)) Log.LogError(message);
        else Log.LogInfo(message);
    }
}
