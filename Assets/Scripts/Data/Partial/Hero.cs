using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheetData
{
    public partial class Hero 
    {
        public bool isUnlock
        {
            get
            {
                return true;
                if (unlock_condition.IsNullOrEmpty()) return true;
                return PlayerTracker.Instance.Contains(unlock_condition);
            }
        }
    }
}