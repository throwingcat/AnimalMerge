using System;
using System.Collections.Generic;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using UnityEngine;

namespace Server
{
    public class DBQuestInfo : DBBase
    {
        public override string DB_KEY()
        {
            return "quest_info";
        }

        public override void DoUpdate()
        {
            base.DoUpdate();

            var point = QuestInfo.Instance.QuestPoint;
            var day = QuestInfo.Instance.QuestDay.ToString();
            var questJson = JsonConvert.SerializeObject(QuestInfo.Instance.QuestSlots);
            var receivedJson = JsonConvert.SerializeObject(QuestInfo.Instance.ReceiveReward);
            var param = new Param();
            param.Add("Point", point);
            param.Add("QuestDay", day);
            param.Add("ReceivedJson", receivedJson);
            param.Add("Json", questJson);

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
                        //초기화
                        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
                        {
                            QuestInfo.Instance.QuestSlots = new List<QuestInfo.QuestSlot>();
                            QuestInfo.Instance.CreateQuestSlot();
                            QuestInfo.Instance.QuestPoint = 0;
                            QuestInfo.Instance.QuestDay = GameManager.GetTime();
                            QuestInfo.Instance.ReceiveReward = new List<bool>() {false, false, false};
                            Update(null);
                        }
                        else
                        {
                            var rows = bro.Rows();
                            foreach (JsonData row in rows)
                            {
                                var inDate = row["inDate"]["S"].ToString();
                                var questJson = row["Json"]["S"].ToString();
                                var point = int.Parse(row["Point"]["N"].ToString());
                                var day = row["QuestDay"]["S"].ToString();
                                var received = row["ReceivedJson"]["S"].ToString();

                                InDate = inDate;

                                //날짜 변경확인
                                var currentDay = GameManager.GetTime().Day;
                                var prevDay = DateTime.Parse(day).Day;
                                if (currentDay != prevDay)
                                {
                                    point = 0;
                                    day = GameManager.GetTime().ToString();
                                    received = "";
                                    QuestInfo.Instance.OnUpdate(point, day, questJson, received);   
                                    Update(null);
                                }
                                else
                                {
                                    QuestInfo.Instance.OnUpdate(point, day, questJson, received);
                                }
                            }
                        }
                    }

                    onFinishDownload?.Invoke();
                });
        }
    }
}