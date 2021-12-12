using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UnityEngine;

namespace SheetData
{
    public partial class Achievement
    {
        public List<ItemInfo> Rewards = new List<ItemInfo>();

        public AchievementInfo.AchievementData Info
        {
            get
            {
                var info = AchievementInfo.Instance.Get(key);
                return info;
            }
        }
        public string DescriptionText
        {
            get
            {
                ulong endValue = (ulong)Grow;
                if (Info != null)
                    endValue = Info.EndValue;
                
                var desc = Description.ToLocalization();
                desc = string.Format(desc, endValue);
                return desc;
            }
        }
        public string ProgressText
        {
            get
            {
                ulong value = 0;
                ulong endValue = (ulong)Grow;
                if (Info != null)
                {
                    value = Info.Value;
                    endValue = Info.EndValue;
                }
                return string.Format("{0} / {1}", value, endValue); 
            }
        }
        public override void Initialize()
        {
            base.Initialize();

            if (Reward1.IsNullOrEmpty() == false)
                Rewards.Add(new ItemInfo(Reward1, Amount1));
            if (Reward2.IsNullOrEmpty() == false)
                Rewards.Add(new ItemInfo(Reward2, Amount2));
        }
    }
}