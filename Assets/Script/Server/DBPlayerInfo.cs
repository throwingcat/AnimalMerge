using System.Reflection;
using BackEnd;
using LitJson;
using Newtonsoft.Json;

namespace Server
{
    public class DBPlayerInfo : DBBase
    {
        public override string DB_KEY()
        {
            return "player_info";
        }

        public override void DoUpdate()
        {
            var param = new Param();

            PlayerInfoManager.Instance.Refresh();

            string json = JsonConvert.SerializeObject(PlayerInfoManager.Instance);
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
                        PlayerInfoManager.Instance.GUID = GameManager.Instance.GUID;
                        PlayerInfoManager.Instance.NickName = Backend.UserNickName;
                        PlayerInfoManager.Instance.Level = 1;
                        PlayerInfoManager.Instance.Exp = 0;
                        PlayerInfoManager.Instance.RankScore = 0;
                        PlayerInfoManager.Instance.SelectHero ="Cat";
                        PlayerInfoManager.Instance.RewardInfoJson = "";
                        PlayerInfoManager.Instance.isPurchasePremium = false;
                    }
                    else
                    {
                        var rows = bro.Rows();
                        foreach (JsonData row in rows)
                        {
                            var inDate = row["inDate"]["S"].ToString();
                            var json = row["Json"]["S"].ToString();
                            InDate = inDate;
                            PlayerInfoManager.Instance.Update(json);
                        }

                        PlayerInfoManager.Instance.OnUpdate();
                    }
                }
                onFinishDownload?.Invoke();
            });

        }
    }
}