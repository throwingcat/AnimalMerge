public class PlayerInfo
{
    private static PlayerInfo _instance;

    public string GUID;
    public int Level;
    public string NickName;
    public int RankScore;
    public string SelectHero;

    public static PlayerInfo Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PlayerInfo();
            return _instance;
        }
    }
}