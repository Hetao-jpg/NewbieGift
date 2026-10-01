using Il2Cpp;

namespace NewbieGift;

public sealed class HonestMerchantPerk : CustomPerk
{
    public const string PerkId = "newbie_gift";

    public override string Id => PerkId;
    public override string DisplayName => Lang.T("良心商人", "The Honest Merchant");
    public override string Description => Lang.T(
        "下层区多了一位偶尔来光顾你生意的良心商人。",
        "A rare honest merchant has appeared in the Lower District and occasionally drops by your shop.");
    public override int Cost => 1;
    public override int MaxSlot => 0;

    public static bool IsActive
    {
        get
        {
            try { return StartingPerk.IsPerkActive(PerkId); }
            catch { return false; }
        }
    }
}