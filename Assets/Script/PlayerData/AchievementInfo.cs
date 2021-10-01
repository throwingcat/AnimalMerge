using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SheetData;
using UnityEngine;
using Violet;

public class AchievementInfo
{
    private static AchievementInfo _instance;

    public static AchievementInfo Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AchievementInfo();
            return _instance;
        }
    }

    public class AchievementData
    {
        //업적 키 
        public string Key;
        //누적치
        public ulong Value;
        //시작 값
        public ulong StartValue;
        //종료 값
        public ulong EndValue;

        
        [JsonIgnore]public bool isClear => EndValue <= Value;
        [JsonIgnore]public Achievement Sheet => Key.ToTableData<Achievement>();
    }

    public Dictionary<string, AchievementData> Achievements = new Dictionary<string, AchievementData>();

    public void OnUpdate(string json)
    {
        if (json.IsNullOrEmpty() == false)
            Achievements = JsonConvert.DeserializeObject<Dictionary<string, AchievementData>>(json);
    }

    public bool Report(string tracker, ulong value)
    {
        var table = TableManager.Instance.GetTable<Achievement>();
        foreach (var row in table)
        {
            var achievement = row.Value as Achievement;

            if (achievement.TrackerKey == tracker)
            {
                string key = achievement.key;
                
                if (Achievements.ContainsKey(key) == false)
                    Achievements.Add(key, new AchievementData()
                    {
                        Key = key,
                        Value = 0,
                        StartValue = 0,
                        EndValue = (ulong)achievement.Grow,
                    });
                Achievements[key].Value += value;
                return true;
            }
        }

        return false;
    }

    public AchievementData Get(string key)
    {
        if (Achievements.ContainsKey(key))
            return Achievements[key];
        return null;
    }
}