namespace NoThanksKeepIt;

internal sealed class ModConfig
{
  public bool EnableMod { get; set; } = true;
  public bool RejectHatedGifts { get; set; } = true;
  public bool RejectDislikedGifts { get; set; } = false;
}
