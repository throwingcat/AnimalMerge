using System;
using System.Collections;
using System.Collections.Generic;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using SheetData;
using UnityEngine;
using Violet;

namespace Server
{
    public class DBBattlePassInfo : DBBase
    {
        public override string DB_KEY()
        {
            return "battlepass_info";
        }

        public override void DoUpdate()
        {
            base.DoUpdate();

            var param = new Param();
            param.Add("Season", BattlePassInfo.Instance.JoinSeason.key);
            param.Add("Point", BattlePassInfo.Instance.Point);
            param.Add("RewardInfo", JsonConvert.SerializeObject(BattlePassInfo.Instance.RewardInfos));
            param.Add("PurchasePremiumPass",BattlePassInfo.Instance.isPurchasePremiumPass);
            
            if (InDate.IsNullOrEmpty())
            {
                SendQueue.Enqueue(Backend.GameData.Insert, DB_KEY(), param, bro =>
                {
                    InDate = bro.GetInDate();
                    _onFinishUpdate?.Invoke();
                    _onFinishUpdate = null;
                });
            }
            else
            {
                SendQueue.Enqueue(Backend.GameData.Update, DB_KEY(), InDate, param, bro =>
                {
                    _onFinishUpdate?.Invoke();
                    _onFinishUpdate = null;
                });
            }
            
            isReservedUpdate = false;
        }

        public override void Download(Action onFinishDownload)
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, DB_KEY(),
                new Where(), 1, bro =>
                {
                    var rows = bro.Rows();

                    string seasonKey = "";
                    int point = 0;
                    bool purchase = false;
                    string rewardInfoJson = "";
                    if (rows != null)
                    {
                        foreach (JsonData row in rows)
                        {
                            var inDate = row["inDate"]["S"].ToString();
                            InDate = inDate;

                            seasonKey = row["Season"]["S"].ToString();
                            point = int.Parse(row["Point"]["N"].ToString());
                            purchase = bool.Parse(row["PurchasePremiumPass"]["BOOL"].ToString());
                            rewardInfoJson = row["RewardInfo"]["S"].ToString();
                        }
                    }

                    //데이터 한번 업데이트 하고
                    BattlePassInfo.Instance.OnUpdate(seasonKey, point, rewardInfoJson, purchase);
                    
                    //진행중인 시즌이 없음
                    if (BattlePassInfo.CurrentSeason == null)
                    {
                        if (BattlePassInfo.Instance.JoinSeason != null)
                        {
                            BattlePassInfo.Instance.OnUpdate("",0,"",false);
                            Update(onFinishDownload);
                        }
                    }
                    else
                    {
                        //현재 시즌과 참가 시즌이 다름
                        var season = BattlePassInfo.CurrentSeason;
                        if (BattlePassInfo.Instance.JoinSeason == null || BattlePassInfo.Instance.JoinSeason.key != season.key)
                        {
                            BattlePassInfo.Instance.OnUpdate(season.key,0,"",false);
                            Update(onFinishDownload);
                        }
                        else
                            onFinishDownload?.Invoke();
                    }
                    
                    
                });
        }
    }
}