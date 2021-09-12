using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Packet;
using Server;
using SheetData;
using Violet;

public class QuestInfo
{
    private static QuestInfo _instance;

    //획득 퀘스트 포인트
    public int QuestPoint = 0;
    public List<bool> ReceiveReward = new List<bool>();
    public List<int> RewardPoint = new List<int>() {60, 150, 300};
    public DateTime QuestDay = new DateTime();
    public List<QuestSlot> QuestSlots = new List<QuestSlot>();

    public static QuestInfo Instance
    {
        get
        {
            if (_instance == null)
                _instance = new QuestInfo();
            return _instance;
        }
    }

    public void OnUpdate(int point, string day, string questJson, string receviedRewardJson)
    {
        QuestPoint = point;
        QuestSlots = new List<QuestSlot>();
        QuestDay = DateTime.Parse(day);
        ReceiveReward = null;
        if (receviedRewardJson.IsNullOrEmpty() == false)
            ReceiveReward = JsonConvert.DeserializeObject<List<bool>>(receviedRewardJson);
        if (ReceiveReward == null)
            ReceiveReward = new List<bool>() {false, false, false};

        if (questJson.IsNullOrEmpty() == false)
            QuestSlots = JsonConvert.DeserializeObject<List<QuestSlot>>(questJson);

        if (QuestSlots == null || QuestSlots.Count == 0)
        {
            CreateQuestSlot();
            AnimalMergeServer.Instance.UpdateDB<DBQuestInfo>(null);
        }
    }

    public void CreateQuestSlot()
    {
        QuestSlots = new List<QuestSlot>();
        for (int i = 0; i < 4; i++)
        {
            var quest = GetQuest();
            if (quest != null)
            {
                QuestSlot slot = new QuestSlot();
                slot.Key = quest.key;
                slot.Index = i;
                slot.Count = 0;

                QuestSlots.Add(slot);
            }
        }
    }

    public void Complete(int index)
    {
        if (index < QuestSlots.Count)
        {
            if (QuestSlots[index].isClear)
            {
                var packet = new PacketBase();
                packet.PacketType = ePACKET_TYPE.QUEST_COMPLETE;
                packet.hash = new Hashtable();
                packet.hash.Add("index", index);
                NetworkManager.Instance.Request(packet, (res) =>
                {
                    var packet = res as PacketReward;
                });
            }
        }
    }

    public bool HasReward(int index)
    {
        return ReceiveReward[index] == false;
    }

    public class QuestSlot
    {
        //퀘스트 슬롯 인덱스
        public int Index;

        //진행 카운트
        public int Count;

        //퀘스트 키
        public string Key;


        //새로운 퀘스트 갱신 시간
        public DateTime RefreshTime;
        
        //퀘스트 만료
        [JsonIgnore] public bool isExpire => RefreshTime == null ? false : GameManager.GetTime() < RefreshTime;
        [JsonIgnore] public Quest Sheet => Key.ToTableData<Quest>();
        [JsonIgnore] public bool isClear => Sheet == null ? false : Sheet.Count <= Count;

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

        [JsonIgnore]
        public string ProgressText
        {
            get { return string.Format("{0} / {1}", Count, Sheet.Count); }
        }
    }

    public Quest GetQuest()
    {
        var table = TableManager.Instance.GetTable<Quest>();

        List<string> activeList = new List<string>();
        foreach (var row in table)
        {
            bool isContains = false;
            foreach (var slot in QuestSlots)
            {
                if (slot.Key == row.Key)
                {
                    isContains = true;
                    break;
                }
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
}