using BackEnd;

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

            var fields = PlayerInfo.Instance.GetType().GetFields();
            foreach (var field in fields)
                param.Add(field.Name, field.GetValue(PlayerInfo.Instance));

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

            isReservedUpdate = false;
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
                        PlayerInfo.Instance.GUID = GameManager.Instance.GUID;
                        PlayerInfo.Instance.NickName = Backend.UserNickName;
                        PlayerInfo.Instance.Level = 1;
                        PlayerInfo.Instance.RankScore = 0;
                        PlayerInfo.Instance.SelectHero = "Cat";
                        Update(onFinishDownload);
                        return;
                    }

                    var rows = bro.Rows();
                    for (var i = 0; i < rows.Count; i++)
                        foreach (var key in rows[i].Keys)
                        {
                            var fields = PlayerInfo.Instance.GetType().GetFields();
                            foreach (var field in fields)
                            {
                                if (key == "inDate")
                                {
                                    if (InDate.IsNullOrEmpty())
                                        InDate = rows[i][key]["S"].ToString();
                                }

                                if (field.Name == key)
                                {
                                    AnimalMergeServer.ApplyField(field, PlayerInfo.Instance, rows[i], key);
                                }
                            }
                        }
                }
                onFinishDownload?.Invoke();
            });

        }
    }
}