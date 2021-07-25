using Define;
using MessagePack;

[MessagePackObject]
public class ItemInfo
{
    [Key(0)] public string Key = "";
    [Key(1)] public eItemType Type = eItemType.Currency;
    [Key(2)] public int Amount = 0;
}