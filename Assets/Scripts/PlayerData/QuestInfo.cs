using System;
using System.Collections.Generic;
using Common;
using Newtonsoft.Json;
using Server;
using SheetData;
using Violet;

public class QuestInfo
{
    private static QuestInfo _instance;
    public DateTime QuestDay;

    //획득 퀘스트 포인트
    public int QuestPoint;
    public List<QuestSlot> QuestSlots = new List<QuestSlot>();
    public List<bool> ReceiveReward = new List<bool>();
    public List<int> RewardPoint = new List<int> {60, 150, 300};

    public static QuestInfo Instance
    {
        get
        {
            if (_instance == null)
                _instance = new QuestInfo();
            return _instance;
        }
    }

    public void OnUpdate(DBQuestInfo.Attribute attribute)
    {
        QuestPoint = attribute.point;
        QuestSlots = new List<QuestSlot>();
        QuestDay = DateTime.Parse(attribute.day);
        ReceiveReward = null;
        if (attribute.receivedRewardJson.IsNullOrEmpty() == false)
            ReceiveReward = JsonConvert.DeserializeObject<List<bool>>(attribute.receivedRewardJson);
        if (ReceiveReward == null)
            ReceiveReward = new List<bool> {false, false, false};

        if (attribute.questJson.IsNullOrEmpty() == false)
            QuestSlots = JsonConvert.DeserializeObject<List<QuestSlot>>(attribute.questJson);

        if (QuestSlots == null || QuestSlots.Count == 0)
        {
            CreateQuestSlot();
            AnimalMergeServer.Instance.UpdateDB<DBQuestInfo>(null);
        }
    }

    public void CreateQuestSlot()
    {
        QuestSlots = new List<QuestSlot>();
        for (var i = 0; i < 4; i++)
        {
            var quest = GetQuest();
            if (quest != null)
            {
                var slot = new QuestSlot();
                slot.Key = quest.key;
                slot.Index = i;
                slot.Count = 0;

                QuestSlots.Add(slot);
            }
        }
    }
    
    public bool HasReward(int index)
    {
        return ReceiveReward[index] == false;
    }

    public Quest GetQuest()
    {
        var table = TableManager.Instance.GetTable<Quest>();

        var activeList = new List<string>();
        foreach (var row in table)
        {
            var isContains = false;
            foreach (var slot in QuestSlots)
                if (slot.Key == row.Key)
                {
                    isContains = true;
                    break;
                }

            if (isContains == false)
                activeList.Add(row.Key);
        }

        if (0 < activeList.Count)
        {
            var result = Utils.RandomPickDefault(activeList);
            return result.ToTableData<Quest>();
        }

        return null;
    }

    public class QuestSlot
    {
        //진행 카운트
        public ulong Count;

        //퀘스트 슬롯 인덱스
        public int Index;

        //퀘스트 키
        public string Key;

        //새로운 퀘스트 갱신 시간
        public DateTime RefreshTime;

        //시작 카운트
        public ulong StartCount;

        //퀘스트 만료
        [JsonIgnore] public bool isExpire => RefreshTime == null ? false : GameManager.GetTime() < RefreshTime;
        [JsonIgnore] public Quest Sheet => Key.ToTableData<Quest>();
        [JsonIgnore] public bool isClear => Sheet != null && (ulong) Sheet.Count <= StartCount - Count;

        [JsonIgnore]
        public string DescriptionText
        {
            get
            {
                var desc = Sheet.Description.ToLocalization();
                desc = string.Format(desc, Sheet.Count);
                return desc;
            }
        }

        [JsonIgnore] public string ProgressText => string.Format("{0} / {1}", Count, Sheet.Count);
    }
}