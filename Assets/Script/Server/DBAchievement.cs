using System;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using UnityEngine;

namespace Server
{
    public class DBAchievement : DBBase
    {
        public override string DB_KEY()
        {
            return "achievement";
        }

        public override void DoUpdate()
        {
            base.DoUpdate();

            var json = JsonConvert.SerializeObject(AchievementInfo.Instance.Achievements);

            var param = new Param();
            param.Add("Json", json);

            if (InDate.IsNullOrEmpty())
            {
                SendQueue.Enqueue(Backend.GameData.Insert,DB_KEY(),param, bro =>
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
        }

        public override void Download(Action onFinishDownload)
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, DB_KEY(),
                new Where(), 1, bro =>
                {
                    if (bro.IsSuccess() == false)
                    {
                        Debug.Log(bro);
                    }
                    else
                    {
                        var rows = bro.Rows();
                        if (rows != null)
                        {
                            foreach (JsonData row in rows)
                            {
                                var inDate = row["inDate"]["S"].ToString();
                                InDate = inDate;
                                
                                var json = row["Json"]["S"].ToString();
                                AchievementInfo.Instance.OnUpdate(json);
                            }
                        }
                    }
                    
                    onFinishDownload?.Invoke();
                });
        }
    }
}