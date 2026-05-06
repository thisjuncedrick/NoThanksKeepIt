using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
 
using NoThanksKeepIt.Integrations;

namespace NoThanksKeepIt
{
  public class ModEntry : Mod
  {
    private static ModConfig Config = null!;

    public override void Entry(IModHelper helper)
    {
      I18n.Init(helper.Translation);
      Config = helper.ReadConfig<ModConfig>();

      var harmony = new Harmony(ModManifest.UniqueID);
      harmony.Patch(
          original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
          prefix: new HarmonyMethod(typeof(ModEntry), nameof(Prefix_TryToReceiveActiveObject)),
          postfix: new HarmonyMethod(typeof(ModEntry), nameof(Postfix_TryToReceiveActiveObject))
      );

      helper.Events.GameLoop.GameLaunched += OnGameLaunched;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
      GMCM.Register(
          helper: Helper,
          manifest: ModManifest,
          getConfig: () => Config,
          reset: () => Config = new ModConfig(),
          save: () => Helper.WriteConfig(Config)
      );
    }

    private static bool CanInterceptGift(NPC npc, Farmer who, StardewValley.Object item)
    {
      // NPC cannot receive gifts
      if (!npc.CanReceiveGifts()) return false;

      // Item is not a giftable item, or is a quest item
      if (!item.canBeGivenAsGift() || item.questItem.Value) return false;

      // Get friendship data
      who.friendshipData.TryGetValue(npc.Name, out var f);

      // Check if we divorced this NPC
      if (f?.IsDivorced() == true) return false;

      // Checks whether the NPC is involved in the group 10 heart event, any NPC involved refuses gift.
      // According to https://stardewvalleywiki.com/Modding:Dialogue
      // +--------------------+----------+----------------------------------------------------+
      // | dumped_Guys        | 7 days   | Set after the corresponding ten-heart group event  |
      // | dumped_Girls       |          |                                                    |
      // +--------------------+----------+----------------------------------------------------+
      if (who.activeDialogueEvents.Keys.Any(k => k.Contains("dumped") && npc.Dialogue.ContainsKey(k))) return false;

      // Gifting is blocked entirely during Green Rain in Year 1 unless the NPC is your spouse
      if (Game1.IsGreenRainingHere() && Game1.year == 1 && !npc.isMarried()) return false;

      bool isStardropTea = item.QualifiedItemId == "(O)StardropTea";

      // Checks for weekly limits
      // - We have no friendship data, treat empty friendship data as 0 gifts given this week.
      // - We have not given the NPC 2 gifts this week.
      // - Today is NPC's birthday so we bypass the weekly limit.
      // - The active item is Stardrop Tea.
      bool withinWeeklyLimit = f == null ||
                               f.GiftsThisWeek < NPC.maxGiftsPerWeek ||
                               who.spouse == npc.Name ||
                               npc.isBirthday() ||
                               isStardropTea;

      // Check for daily limits
      // - We have no friendship data, treat empty friendship data as 0 gifts given today.
      // - We have not given a gift to the NPC today.
      // - The item is Stardrop Tea.
      bool withinDailyLimit = (f == null || f.GiftsToday < 1) || isStardropTea;

      return withinWeeklyLimit && withinDailyLimit;
    }

    public static void Prefix_TryToReceiveActiveObject(NPC __instance, Farmer who, bool probe, out Item? __state)
    {
      __state = null;

      if (!Config.EnableMod) return;
      if (probe || who.ActiveObject == null) return;
      if (!CanInterceptGift(__instance, who, who.ActiveObject)) return;

      int taste = __instance.getGiftTasteForThisItem(who.ActiveObject);
      bool isUnwanted = (taste == NPC.gift_taste_dislike && Config.RejectDislikedGifts) ||
                        (taste == NPC.gift_taste_hate && Config.RejectHatedGifts);

      if (isUnwanted)
        __state = who.ActiveObject.getOne();
    }

    public static void Postfix_TryToReceiveActiveObject(Farmer who, Item? __state)
    {
      if (__state == null) return;
      who.addItemToInventoryBool(__state);
    }
  }
}