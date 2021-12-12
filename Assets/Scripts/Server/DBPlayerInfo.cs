using System.Reflection;
using BackEnd;
using LitJson;
using Newtonsoft.Json;

namespace Server
{
    public class DBPlayerInfo : DBBase
    {
        public PlayerInfo PlayerInfo = new PlayerInfo();
        public override string DB_KEY()
        {
            return "player_info";
        }

        public override void DoUpdate()
        {
            var param = new Param();

            string json = PlayerInfo.ToJson();
            param.Add("Json",json);

            if (InDate.IsNullOrEmpty())
                SendQueue.Enqueue(Backend.GameData.Insert, DB_KEY(), param, bro =>
                {
                    InDate = bro.GetInDate();
                    _onFinishUpdate?.Invoke();
                    _onFinishUpdate = null;
                });
            else
                SendQueue.Enqueue(Backend.GameData.Update,
                    DB_KEY(), InDate, param,
                    bro =>
                    {
                        _onFinishUpdate?.Invoke();
                        _onFinishUpdate = null;
                    });
        }

        public override void Download(System.Action onFinishDownload)
        {
            //뒤끝기반
            SendQueue.Enqueue(Backend.GameData.GetMyData,DB_KEY(), new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    UnityEngine.Debug.Log(bro);
                }
                else
                {
                    if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
                    {
                        //최초 설정
                        PlayerInfo.elements.GUID = GameManager.Instance.GUID;
                        PlayerInfo.elements.Nickname = Backend.UserNickName;
                        PlayerInfo.elements.Level = 1;
                        PlayerInfo.elements.Exp = 0;
                        PlayerInfo.elements.RankScore = 0;
                        PlayerInfo.elements.SelectHero ="Cat";
                        PlayerInfo.elements.LevelRewardsJson = "";
                        PlayerInfo.elements.isPurchasePremium = false;
                    }
                    else
                    {
                        var rows = bro.Rows();
                        foreach (JsonData row in rows)
                        {
                            var inDate = row["inDate"]["S"].ToString();
                            var json = row["Json"]["S"].ToString();
                            InDate = inDate;
                            PlayerInfo.OnUpdate(json);
                        }
                    }
                }
                onFinishDownload?.Invoke();
            });

        }
    }
}