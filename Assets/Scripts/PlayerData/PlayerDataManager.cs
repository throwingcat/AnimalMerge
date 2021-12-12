using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager
{
    private static PlayerDataManager _instance;

    public static PlayerDataManager Instance
    {
        get
        {
            if(_instance == null)
                _instance = new PlayerDataManager();
            return _instance;
        }
    }

    public List<PlayerDataBase> Datas = new List<PlayerDataBase>(); 
    public PlayerDataManager()
    {
        Datas.Add(new PlayerInfo());
    }

    public static T Get<T>() where T : PlayerDataBase
    {
        foreach (var data in Instance.Datas)
            if (data is T result) return result;
        return null;
    }

    public static void Download<T>(Action onFinish) where T : PlayerDataBase
    {
        var target = Get<T>();
        target.Download(onFinish);
    }

    public static void Update<T>(string json) where T : PlayerDataBase
    {
        var data = Get<T>();
        data.OnUpdate(json);
    }
}
