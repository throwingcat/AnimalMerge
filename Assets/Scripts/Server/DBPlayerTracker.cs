using System;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using UnityEngine;

namespace Server
{
    public class DBPlayerTracker : DBBase
    {
        public override string DB_KEY()
        {
            return "player_tracker";
        }

        public override void DoUpdate()
        {
            base.DoUpdate();
            
            //Quest 갱신
            //Achievement 갱신
            
            var json = JsonConvert.SerializeObject(PlayerTracker.Instance.Tracker);
            var param = new Param();
            param.Add("Json", json);

            if (InDate.IsNullOrEmpty())
                SendQueue.Enqueue(Backend.GameData.Insert, DB_KEY(), param, bro =>
                {
                    InDate = bro.GetInDate();
                    _onFinishUpdate?.Invoke();
                    _onFinishUpdate = null;
                });
            else
                SendQueue.Enqueue(Backend.GameData.Update, DB_KEY(), InDate, param, bro =>
                {
                    _onFinishUpdate?.Invoke();
                    _onFinishUpdate = null;
                });
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
                        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
                        {
                        }
                        else
                        {
                            var rows = bro.Rows();
                            foreach (JsonData row in rows)
                            {
                                var inDate = row["inDate"]["S"].ToString();
                                var json = row["Json"]["S"].ToString();
                                InDate = inDate;
                                PlayerTracker.Instance.OnUpdate(json);
                            }
                        }
                    }

                    onFinishDownload?.Invoke();
                });
        }
    }
}