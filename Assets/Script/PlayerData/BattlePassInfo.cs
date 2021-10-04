using System.Collections.Generic;
using Newtonsoft.Json;
using SheetData;
using Violet;

public class BattlePassInfo
{
    private static BattlePassInfo _instance;

    public static BattlePassInfo Instance
    {
        get
        {
            if (_instance == null)
                _instance = new BattlePassInfo();
            return _instance;
        }
    }

    #region Value

    public string JoinSeasonKey;
    public int Point;
    public List<BattlePassRewardInfo> RewardInfos = new List<BattlePassRewardInfo>();
    public bool isPurchasePremiumPass;

    #endregion

    #region Cache Value

    public List<BattlePass> SeasonPassItems = new List<BattlePass>();

    public Dictionary<string, BattlePassRewardInfo> SeasonPassRewardInfo =
        new Dictionary<string, BattlePassRewardInfo>();

    public BattlePassSeason JoinSeason => JoinSeasonKey.ToTableData<BattlePassSeason>();

    public bool isActiveSeason => JoinSeason == null
        ? false
        : JoinSeason.StartTime <= GameManager.GetTime() && GameManager.GetTime() < JoinSeason.EndTime;

    public double SeasonReaminTime => (JoinSeason.EndTime - GameManager.GetTime()).TotalSeconds;

    public static BattlePassSeason CurrentSeason
    {
        get
        {
            var table = TableManager.Instance.GetTable<BattlePassSeason>();
            foreach (var row in table)
            {
                var season = row.Value as BattlePassSeason;
                if (season.StartTime <= GameManager.GetTime() && GameManager.GetTime() < season.EndTime)
                    return season;
            }

            return null;
        }
    }

    #endregion

    public void OnUpdate(string season, int point, string rewardInfoJson, bool purchase)
    {
        JoinSeasonKey = season;
        Point = point;
        RewardInfos = JsonConvert.DeserializeObject<List<BattlePassRewardInfo>>(rewardInfoJson);
        if (RewardInfos == null)
            RewardInfos = new List<BattlePassRewardInfo>();

        isPurchasePremiumPass = purchase;

        SeasonPassItems.Clear();
        SeasonPassRewardInfo.Clear();

        var sheet = TableManager.Instance.GetTable<BattlePass>();
        foreach (var row in sheet)
        {
            BattlePass pass = row.Value as BattlePass;
            if (pass.Season == JoinSeasonKey)
            {
                SeasonPassItems.Add(pass);
                SeasonPassRewardInfo.Add(pass.key, null);
            }
        }

        SeasonPassItems.Sort((a, b) =>
        {
            if (a.point < b.point) return -1;
            if (a.point > b.point) return 1;
            return 0;
        });

        foreach (var info in RewardInfos)
        {
            if (SeasonPassRewardInfo.ContainsKey(info.Key))
                SeasonPassRewardInfo[info.Key] = info;
        }
    }

    public List<ItemInfo> ReceiveReward(string key)
    {
        var rewards = new List<ItemInfo>();
        if (SeasonPassRewardInfo.ContainsKey(key))
        {
            var info = SeasonPassRewardInfo[key];
            if (info == null)
            {
                info = new BattlePassRewardInfo()
                {
                    Key = key,
                    isReceivedPassReward = false,
                    isReceivedPremiumReward = false,
                };
                RewardInfos.Add(info);
            }

            if (info.isReceivedPassReward == false)
            {
                info.isReceivedPassReward = true;
                rewards.Add(info.PassReward);
            }

            if (isPurchasePremiumPass)
            {
                if (info.isReceivedPremiumReward == false)
                {
                    info.isReceivedPremiumReward = true;
                    rewards.AddRange(info.PremiumReward);
                }
            }
            SeasonPassRewardInfo[key] = info;
        }

        for (int i = 0; i < RewardInfos.Count; i++)
        {
            if (RewardInfos[i].Key == key)
            {
                RewardInfos[i] = SeasonPassRewardInfo[key];
                break;
            }
        }

        return rewards;
    }

    public bool HasReward(string key)
    {
        if (SeasonPassRewardInfo.ContainsKey(key) == false)
            return true;
        
        if (SeasonPassRewardInfo[key].isReceivedPassReward == false)
            return true;

        if (isPurchasePremiumPass && SeasonPassRewardInfo[key].isReceivedPremiumReward == false)
            return true;
        return false;
    }
    public class BattlePassRewardInfo
    {
        public bool isReceivedPassReward;
        public bool isReceivedPremiumReward;
        public string Key;

        [JsonIgnore] public BattlePass Sheet => Key.ToTableData<BattlePass>();
        [JsonIgnore] public ItemInfo PassReward => Sheet?.PassReward;
        [JsonIgnore] public List<ItemInfo> PremiumReward => Sheet?.PremiumRewards;

        [JsonIgnore]
        public bool isReceiveComplete
        {
            get
            {
                if (Instance.isPurchasePremiumPass)
                    return isReceivedPassReward && isReceivedPremiumReward;
                return isReceivedPassReward;
            }
        }
    }
}