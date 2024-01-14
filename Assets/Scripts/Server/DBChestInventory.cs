using System;
using Newtonsoft.Json;

namespace Server
{
    public class DBChestInventory : DBBase
    {
        public override string DB_KEY()
        {
            return "chest_inventory";
        }

        public override void Save()
        {
            _Save(ChestInventory.Instance.ChestSlots);
        }

        public override void Load(Action onFinishDownload)
        {
            _Load(json =>
            {
                ChestInventory.Instance.ChestSlots = JsonConvert.DeserializeObject<ChestInventory.ChestSlot[]>(json);
                Array.Sort(ChestInventory.Instance.ChestSlots,
                    (a, b) =>
                    {
                        if (a.index < b.index) return -1;
                        if (a.index > b.index) return 1;
                        return 0;
                    });
                onFinishDownload?.Invoke();
            });
        }
    }
}