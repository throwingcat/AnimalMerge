using System;

namespace Server
{
    public class DBInventory : DBBase
    {
        public override string DB_KEY()
        {
            return "inventory";
        }

        public override void Save()
        {
            _Save(Inventory.Instance.Items);
        }

        public override void Load(Action onFinishDownload)
        {
            _Load(json =>
            {
                Inventory.Instance.Update(json);
                onFinishDownload?.Invoke();
            });
        }
    }
}