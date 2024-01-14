using System;
using System.Collections.Generic;
using Google.MiniJSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Server
{
    public class DBBattlePassInfo : DBBase
    {
        public class Attribute
        {
            public string season;
            public int point;
            public List<BattlePassInfo.BattlePassRewardInfo> rewardInfo;
            public bool purchasePremiumPass;
        }
        
        public override string DB_KEY()
        {
            return "battlepass_info";
        }

        public override void Save()
        {
            var param = new Attribute();
            param.season = BattlePassInfo.Instance.JoinSeasonKey;
            param.point = BattlePassInfo.Instance.Point;
            param.rewardInfo = BattlePassInfo.Instance.RewardInfos;
            param.purchasePremiumPass = BattlePassInfo.Instance.isPurchasePremiumPass;
            
            _Save(param);
        }

        public override void Load(Action onFinishDownload)
        {
            _Load((json) =>
            {
                var lAttribute = JsonConvert.DeserializeObject(json) as Attribute;
                BattlePassInfo.Instance.OnUpdate(
                    lAttribute.season,
                    lAttribute.point,
                    lAttribute.rewardInfo,
                    lAttribute.purchasePremiumPass);

                //진행중인 시즌이 없음
                if (BattlePassInfo.CurrentSeason == null)
                {
                    if (BattlePassInfo.Instance.JoinSeason != null)
                    {
                        BattlePassInfo.Instance.OnUpdate("", 0, new(), false);
                        SaveReserve(onFinishDownload);
                    }
                    else
                    {
                        onFinishDownload?.Invoke();
                    }
                }
                else
                {
                    //현재 시즌과 참가 시즌이 다름
                    var season = BattlePassInfo.CurrentSeason;
                    if (BattlePassInfo.Instance.JoinSeason == null ||
                        BattlePassInfo.Instance.JoinSeason.key != season.key)
                    {
                        BattlePassInfo.Instance.OnUpdate(season.key, 0, new(), false);
                        SaveReserve(onFinishDownload);
                    }
                    else
                    {
                        onFinishDownload?.Invoke();
                    }
                }
            });
        }
    }
}