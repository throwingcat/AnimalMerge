using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SheetData;
using Violet;

public class PlayerInfo
{
    #region Instance
    private static PlayerInfo _instance;
    public static PlayerInfo Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PlayerInfo();
            return _instance;
        }
    }
    #endregion
    
    public string GUID;
    public int Level;
    public int Exp;
    public string NickName;
    public int RankScore;
    public string SelectHero;
    public string RewardInfoJson;
    public bool isPurchasePremium;

    [JsonIgnore]
    private List<PlayerLevelRewardInfo> _playerLevelRewardInfos = new List<PlayerLevelRewardInfo>();

    public void Update(string json)
    {
        _instance = JsonConvert.DeserializeObject<PlayerInfo>(json);
    } 
    public void Refresh()
    {
        List<PlayerLevelRewardInfo> result = new List<PlayerLevelRewardInfo>();
        foreach (var info in _playerLevelRewardInfos)
        {
            if (info.isReceivedPassReward == false && info.isReceivedPremiumReward) continue;

            result.Add(info);
        }

        RewardInfoJson = JsonConvert.SerializeObject(result);
    }

    public void OnUpdate()
    {
        List<PlayerLevelRewardInfo> infos = new List<PlayerLevelRewardInfo>();
        if (RewardInfoJson.IsNullOrEmpty() == false)
            infos = JsonConvert.DeserializeObject<List<PlayerLevelRewardInfo>>(RewardInfoJson);

        var sheet = TableManager.Instance.GetTable<PlayerLevel>();
        foreach (var row in sheet)
        {
            bool isInsert = false;
            foreach (var info in infos)
            {
                if (info.Key == row.Key)
                {
                    _playerLevelRewardInfos.Add(info);
                    isInsert = true;
                    break;
                }
            }

            if (isInsert) continue;

            _playerLevelRewardInfos.Add(new PlayerLevelRewardInfo()
            {
                Key = row.Key,
                isReceivedPassReward = false,
                isReceivedPremiumReward = false,
            });
        }
    }

    public List<PlayerLevelRewardInfo> GetRewardInfos()
    {
        return _playerLevelRewardInfos;
    }

    public List<ItemInfo> ReceiveReward(string key)
    {
        var rewards = new List<ItemInfo>();

        foreach (var info in _playerLevelRewardInfos)
        {
            if (info.Key == key)
            {
                if (info.isReceivedPassReward == false)
                {
                    rewards.Add(info.PassReward);
                    info.isReceivedPassReward = true;
                }

                if (info.isReceivedPremiumReward == false && isPurchasePremium)
                {
                    rewards.Add(info.PremiumReward);
                    info.isReceivedPremiumReward = true;
                }
            }
        }

        return rewards;
    }

    public bool HasReward(string key)
    {
        Dictionary<string, PlayerLevelRewardInfo> dictionary = new Dictionary<string, PlayerLevelRewardInfo>();

        foreach (var info in _playerLevelRewardInfos)
            dictionary.Add(info.Key, info);

        if (dictionary.ContainsKey(key) == false)
            return true;

        if (dictionary[key] == null)
            return true;

        if (dictionary[key].isReceivedPassReward == false)
            return true;

        if (isPurchasePremium && dictionary[key].isReceivedPremiumReward == false)
            return true;
        return false;
    }

    public void GetExp(int exp)
    {
        if (isMaxLevel()) return;
        
        Exp += exp;
        var sheet = GetLevelSheet();
        if (sheet.exp <= Exp)
        {
            Level++;
            Exp -= sheet.exp;
        }
    }

    public PlayerLevel GetLevelSheet()
    {
        var sheet = TableManager.Instance.GetTable<PlayerLevel>();
        foreach (var row in sheet)
        {
            var data = row.Value as PlayerLevel;
            if (data.level == Level)
                return data;
        }

        return null;
    }

    public bool isMaxLevel()
    {
        var sheet = GetLevelSheet();
        if (sheet != null)
        {
            if (0 < sheet.exp)
                return false;
        }

        return true;
    }

    

    public class PlayerLevelRewardInfo
    {
        public string Key;
        public bool isReceivedPassReward;
        public bool isReceivedPremiumReward;

        [JsonIgnore] public PlayerLevel Sheet => Key.ToTableData<PlayerLevel>();
        [JsonIgnore] public ItemInfo PassReward => Sheet?.Reward;
        [JsonIgnore] public ItemInfo PremiumReward => Sheet?.PremiumReward;

        [JsonIgnore]
        public bool isReceiveComplete
        {
            get
            {
                if (Instance.isPurchasePremium)
                    return isReceivedPassReward && isReceivedPremiumReward;
                return isReceivedPassReward;
            }
        }
    }
}