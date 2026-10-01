namespace NewbieGift;

internal static class SaveKeys
{
    public static string FirstVisitDay(int slot)
        => $"first_visit_day_slot{slot}";

    public static string LastVisitDay(int slot)
        => $"last_visit_day_slot{slot}";

    // 礼金是否已发放（幂等标记）
    public static string CashGranted(int slot)
        => $"gift_cash_granted_slot{slot}";

    // 每日派发是否已处理（幂等标记）
    public static string GrantedDay(int slot)
        => $"granted_day_slot{slot}";
}