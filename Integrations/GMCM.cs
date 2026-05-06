using NoThanksKeepIt.Integrations.GenericModConfigMenu;
using StardewModdingAPI;

namespace NoThanksKeepIt.Integrations;

internal static class GMCM
{
  public static void Register(
    IModHelper helper,
    IManifest manifest,
    Func<ModConfig> getConfig,
    Action reset,
    Action save
  )
  {
    var gmcm = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

    if (gmcm is null)
      return;

    gmcm.Register(manifest, reset, save);

    gmcm.AddBoolOption(
      manifest,
      () => getConfig().EnableMod,
      v => getConfig().EnableMod = v,
      () => I18n.Config_EnableMod_Name(),
      () => I18n.Config_EnableMod_Tooltip()
    );

    gmcm.AddBoolOption(
     manifest,
      () => getConfig().RejectHatedGifts,
      v => getConfig().RejectHatedGifts = v,
      () => I18n.Config_IncludeHatedGifts_Name(),
      () => I18n.Config_IncludeHatedGifts_Tooltip()
    );

    gmcm.AddBoolOption(
      manifest,
      () => getConfig().RejectDislikedGifts,
      v => getConfig().RejectDislikedGifts = v,
      () => I18n.Config_IncludeDislikedGifts_Name(),
      () => I18n.Config_IncludeDislikedGifts_Tooltip()
    );

    gmcm.AddParagraph(
      manifest,
      () => I18n.Config_GiftLimit_Disclaimer()
    );
  }
}
