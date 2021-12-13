using Define;
using MessagePack;
using SheetData;

[MessagePackObject]
public class ItemInfo
{
    [Key(0)] public string Key = "";
    [Key(1)] public eItemType Type = eItemType.Currency;
    [Key(2)] public int Amount = 0;
    
    public ItemInfo(){}

    public ItemInfo(string item, int amount)
    {
        var row = item.ToTableData<Item>();
        Key = row.key;
        Type = Utils.EnumParse<eItemType>(row.Type);
        Amount = amount;
    }
}