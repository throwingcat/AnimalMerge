using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server
{
    public class DBQuestInfo : DBBase
    {
        [Serializable]
        public class Attribute
        {
            public int point;
            public string day;
            public string questJson;
            [FormerlySerializedAs("receivedJson")] public string receivedRewardJson;
        }
        public override string DB_KEY()
        {
            return "quest_info";
        }

        public override void Save()
        {
            base.Save();

            Attribute attribute = new();
            attribute.point = QuestInfo.Instance.QuestPoint;
            attribute.day = QuestInfo.Instance.QuestDay.ToString(); 
            attribute.questJson =JsonConvert.SerializeObject(QuestInfo.Instance.QuestSlots);
            attribute.receivedRewardJson =JsonConvert.SerializeObject(QuestInfo.Instance.ReceiveReward);
            
            _Save(attribute);
        }

        public override void Load(Action onFinishDownload)
        {
            _Load((json) =>
            {
                if (json.IsNullOrEmpty())
                {
                    QuestInfo.Instance.QuestSlots = new List<QuestInfo.QuestSlot>();
                    QuestInfo.Instance.CreateQuestSlot();
                    QuestInfo.Instance.QuestPoint = 0;
                    QuestInfo.Instance.QuestDay = GameManager.GetTime();
                    QuestInfo.Instance.ReceiveReward = new List<bool>() {false, false, false};
                    SaveReserve(null);
                }
                else
                {
                    var attribute = JsonConvert.DeserializeObject<Attribute>(json);
                    
                    //날짜 변경확인
                    var currentDay = GameManager.GetTime().Day;
                    var prevDay = DateTime.Parse(attribute.day).Day;
                    if (currentDay != prevDay)
                    {
                        attribute.point = 0;
                        attribute.day = GameManager.GetTime().ToString();
                        attribute.receivedRewardJson = "";
                        QuestInfo.Instance.OnUpdate(attribute);   
                        SaveReserve(null);
                    }
                    else
                    {
                        QuestInfo.Instance.OnUpdate(attribute);
                    }
                }
            });
        }
    }
}