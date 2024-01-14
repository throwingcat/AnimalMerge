using System;

namespace Server
{
    public class DBPlayerInfo : DBBase
    {
        public PlayerInfo PlayerInfo = new();

        public override string DB_KEY()
        {
            return "player_info";
        }

        public override void Save()
        {
            _Save(PlayerInfo.attribute);
        }

        public override void Load(Action onFinishDownload)
        {
            _Load(json =>
            {
                if (json.IsNullOrEmpty())
                {
                    //최초 설정
                    PlayerInfo.attribute.GUID = GameManager.Instance.GUID;
                    PlayerInfo.attribute.Nickname = "마법사";
                    PlayerInfo.attribute.Level = 1;
                    PlayerInfo.attribute.Exp = 0;
                    PlayerInfo.attribute.RankScore = 0;
                    PlayerInfo.attribute.SelectHero = "Cat";
                    PlayerInfo.attribute.LevelRewardsJson = "";
                    PlayerInfo.attribute.isPurchasePremium = false;
                }
                else
                {
                    PlayerInfo.OnUpdate(json);
                }

                onFinishDownload?.Invoke();
            });
        }
    }
}