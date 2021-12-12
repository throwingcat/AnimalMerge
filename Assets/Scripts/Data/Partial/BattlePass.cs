using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheetData
{
    public partial class BattlePass
    {
        public ItemInfo PassReward;
        public List<ItemInfo> PremiumRewards = new List<ItemInfo>();
        
        public override void Initialize()
        {
            base.Initialize();
            
            PassReward = new ItemInfo(pass_reward,pass_amount);
            PremiumRewards.Add(new ItemInfo(premium_reward1,premium_amount1));
            PremiumRewards.Add(new ItemInfo(premium_reward2,premium_amount2));
        }
    }
}