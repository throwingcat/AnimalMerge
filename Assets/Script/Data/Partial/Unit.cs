using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheetData
{
    public partial class Unit
    {
        public static List<Unit> BadBlocks = new List<Unit>();
        public override void Initialize()
        {
            base.Initialize();
            
            if(group == "Rat")
                BadBlocks.Add(this);
            
            BadBlocks.Sort((a, b) =>
            {
                if (a.score < b.score) return 1;
                if (b.score < a.score) return -1;
                return 0;
            });
        }
    }
}