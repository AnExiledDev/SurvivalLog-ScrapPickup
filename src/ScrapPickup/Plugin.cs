using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ScrapPickup.Adapters;
using ScrapPickup.Core;

namespace ScrapPickup;

[BepInPlugin(Guid, "Scrap Pickup", Version)]
[BepInDependency(OptionalLive.ToolkitGuid, BepInDependency.DependencyFlags.SoftDependency)]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.scrappickup";
    private const string Version = "0.1.0";

    public override void Load()
    {
        if (!OptionalLive.IsToolkitInstalled) Log.LogInfo(OptionalLive.MissingNote);

        var useTrunk = Config.Bind("Scrap", "ToTrunk", true,
            "When true, items from furniture you scrap or demolish while exploring go to your trunk or shopping cart first, then your backpack. When false, or at home, they go to your backpack. They drop on the ground only when nothing has room.\n" +
            "设为 true 时，外出探索时回收或拆除家具得到的物品会先放进后备箱或购物车，再放进背包。设为 false 或在家时，物品会放进背包。只有都放不下时，物品才会掉在地上。");

        Log.LogInfo($"===== Scrap Pickup {Version} started");
        ScrapHooks.Apply(new Harmony(Guid), Log);
        OptionalLive.Watch(() => ScrapHooks.Use(new ScrapSettings(useTrunk.Value)), useTrunk);
    }
}
