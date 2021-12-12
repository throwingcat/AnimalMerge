using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheetData
{
    public partial class Unit
    {
        public bool isBadBlock = false;
        public static List<Unit> BadBlocks = new List<Unit>();
        public Hero Master => master.ToTableData<Hero>();
        
        public static Dictionary<string,List<Unit>> Sorted = new Dictionary<string, List<Unit>>();
        
        public override void Initialize()
        {
            base.Initialize();

            if (master == "Rat")
            {
                isBadBlock = true;
                BadBlocks.Add(this);

                BadBlocks.Sort((a, b) =>
                {
                    if (a.score < b.score) return 1;
                    if (b.score < a.score) return -1;
                    return 0;
                });
            }

            if (Sorted.ContainsKey(master) == false)
                Sorted.Add(master,new List<Unit>());
            Sorted[master].Add(this);
            Sorted[master].Sort((a, b) =>
            {
                if (a.index < b.index) return -1;
                if (a.index > b.index) return 1;
                return 0;
            });
        }
    }
}