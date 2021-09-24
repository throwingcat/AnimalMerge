using System.Collections.Generic;
using UnityEngine.UIElements;

public class PlayerInfo
{
    public static PlayerInfo Instance
    {
        get
        {
            if(_instance==null)
                _instance = new PlayerInfo();
            return _instance;
        }
    }
    
    private static PlayerInfo _instance;
    
    public string GUID;
    public string NickName;
    public int Level;
    public int RankScore;
    public string SelectHero;
}