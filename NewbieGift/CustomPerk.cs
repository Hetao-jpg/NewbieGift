using System;
using Il2Cpp;

namespace NewbieGift;

public abstract class CustomPerk
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract string Description { get; }
    public virtual int Cost => 1;
    public virtual int MaxSlot => 0;
    public virtual string[] IncompatibleIds => Array.Empty<string>();

    public virtual void OnNewGame() { }
    public virtual void OnNewDay() { }

    public StartingPerk Create()
    {
        var perk = new StartingPerk
        {
            id = Id,
            cost = Cost,
            maxSlot = MaxSlot
        };

        if (perk.incompatiblePerks != null)
        {
            foreach (var id in IncompatibleIds)
                if (!string.IsNullOrEmpty(id))
                    perk.incompatiblePerks.Add(id);
        }

        return perk;
    }
}