using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Server;
using SheetData;
using Violet;

public class PlayerInfo : PlayerDataBase
{
    public Elements elements = new Elements();

    public List<PlayerLevelRewardInfo> LevelRewardInfos = new List<PlayerLevelRewardInfo>();

    public override void OnUpdate(string json)
    {
        elements = JsonConvert.DeserializeObject<Elements>(json);
        
        LevelRewardInfos = JsonConvert.DeserializeObject<List<PlayerLevelRewardInfo>>(elements.LevelRewardsJson);
        if (LevelRewardInfos == null)
            LevelRewardInfos = new List<PlayerLevelRewardInfo>();
    }

    #region Client
    public override void Download(Action onFinish)
    {
        var db = AnimalMergeServer.Instance.GetDB<DBPlayerInfo>();
        OnUpdate(db.PlayerInfo.ToJson());
        onFinish?.Invoke();
    }
    #endregion

    #region Utility
    public PlayerLevel GetLevelSheet()
    {
        var sheet = TableManager.Instance.GetTable<PlayerLevel>();
        foreach (var row in sheet)
        {
            if (row.Value is PlayerLevel data && data.level == elements.Level)
                return data;
        }

        return null;
    }

    public bool isMaxLevel()
    {
        var sheet = GetLevelSheet();
        if (sheet != null)
            if (0 < sheet.exp)
                return false;

        return true;
    }
    
    public bool HasReward(string key)
    {
        var dictionary = new Dictionary<string, PlayerLevelRewardInfo>();

        foreach (var info in LevelRewardInfos)
            dictionary.Add(info.Key, info);

        if (dictionary.ContainsKey(key) == false)
            return true;

        if (dictionary[key] == null)
            return true;

        if (dictionary[key].isReceivedPassReward == false)
            return true;

        if (elements.isPurchasePremium && dictionary[key].isReceivedPremiumReward == false)
            return true;
        return false;
    }
    #endregion
    
    #region Server Method 
    
    public string ToJson()
    {
        return JsonConvert.SerializeObject(elements);
    }
    
    public List<ItemInfo> ReceiveReward(string key)
    {
        var rewards = new List<ItemInfo>();

        foreach (var info in LevelRewardInfos)
            if (info.Key == key)
            {
                if (info.isReceivedPassReward == false)
                {
                    rewards.Add(info.PassReward);
                    info.isReceivedPassReward = true;
                }

                if (info.isReceivedPremiumReward == false && elements.isPurchasePremium)
                {
                    rewards.Add(info.PremiumReward);
                    info.isReceivedPremiumReward = true;
                }
            }

        return rewards;
    }
    
    public void GetExp(int exp)
    {
        if (isMaxLevel()) return;

        elements.Exp += exp;
        var sheet = GetLevelSheet();
        if (sheet.exp <= elements.Exp)
        {
            elements.Level++;
            elements.Exp -= sheet.exp;
        }
    }
    #endregion

    public class Elements
    {
        public int ChestPoint;
        public int Exp;
        public string GUID;
        public bool isPurchasePremium;
        public int Level;
        public string Nickname;
        public int RankScore;
        public string SelectHero;
        public string LevelRewardsJson;
    }

    public class PlayerLevelRewardInfo
    {
        public bool isReceivedPassReward;
        public bool isReceivedPremiumReward;
        public string Key;

        [JsonIgnore] public PlayerLevel Sheet => Key.ToTableData<PlayerLevel>();
        [JsonIgnore] public ItemInfo PassReward => Sheet?.Reward;
        [JsonIgnore] public ItemInfo PremiumReward => Sheet?.PremiumReward;

        [JsonIgnore]
        public bool isReceiveComplete
        {
            get
            {
                var playerInfo = PlayerDataManager.Get<PlayerInfo>();
                if (playerInfo.elements.isPurchasePremium)
                    return isReceivedPassReward && isReceivedPremiumReward;
                return isReceivedPassReward;
            }
        }
    }
}