using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SheetData
{
    public partial class PlayerLevel
    {
        public ItemInfo Reward;
        public ItemInfo PremiumReward;

        public override void Initialize()
        {
            base.Initialize();

            Reward = new ItemInfo(reward, amount);
            PremiumReward = new ItemInfo(premium_reward, premium_amount);
        }
    }
}