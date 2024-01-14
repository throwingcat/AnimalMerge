using System;
using System.Collections.Generic;

namespace Server
{
    public class DBUnitInventory : DBBase
    {
        //Group , InDate
        public Dictionary<string, string> GroupIndate = new();

        public override string DB_KEY()
        {
            return "unit_inventory";
        }

        public override void Save()
        {
        }

        public override void Load(Action onFinishDownload)
        {
        }
    }
}