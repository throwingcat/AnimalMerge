using System.Collections;
using BackEnd;
using LitJson;
using MessagePack;
using Packet;
using UnityEngine;

public class AnimalMergeServer
{
    private static AnimalMergeServer _instance;

    public static AnimalMergeServer Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AnimalMergeServer();
            return _instance;
        }
    }

    private bool _isReservedUpdatePlayerInfo = false;
    
    public void OnUpdate()
    {
        if (_isReservedUpdatePlayerInfo)
        {
            DoUpdatePlayerInfo();
            _isReservedUpdatePlayerInfo = false;
        }
    }
    public void ReceivePacket(byte[] bytes)
    {
        var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        var packet = MessagePackSerializer.Deserialize<PacketBase>(bytes, lz4Options);

        switch (packet.PacketType)
        {
            case ePACKET_TYPE.GET_CHEST_LIST:
                break;
            case ePACKET_TYPE.REPORT_GAME_RESULT:
                BattleResult(packet);
                break;
        }
    }

    private void BattleResult(PacketBase packet)
    {
        var isWin = (bool) packet.hash["is_win"];

        if (isWin)
            PlayerInfo.Instance.RankScore += 5;
        else
            PlayerInfo.Instance.RankScore -= 5;

        UpdatePlayerInfo();
    }

    public void RefreshPlayerInfo()
    {
        //뒤끝기반
        var bro = Backend.GameData.GetMyData("player_info", new Where(), 10);
        if (bro.IsSuccess() == false)
        {
            Debug.Log(bro);
            return;
        }

        if (bro.GetReturnValuetoJSON()["rows"].Count <= 0)
        {
            //최초 설정
            PlayerInfo.Instance.GUID = GameManager.Instance.GUID;
            PlayerInfo.Instance.NickName = Backend.UserNickName;
            PlayerInfo.Instance.Level = 1;
            PlayerInfo.Instance.RankScore = 1000;

            UpdatePlayerInfo();
            return;
        }

        var rows = bro.Rows();
        for (var i = 0; i < rows.Count; i++)
            foreach (var key in rows[i].Keys)
            {
                var fields = PlayerInfo.Instance.GetType().GetFields();
                foreach (var field in fields)
                    if (field.Name == key)
                    {
                        var type = WhichDataTypeIsIt(rows[i], key);
                        var value = rows[i][key][type].ToString();

                        var fieldType = field.FieldType.Name;
                        fieldType = fieldType.ToLower();
                        switch (fieldType)
                        {
                            case "bool":
                            case "boolean":
                                field.SetValue(PlayerInfo.Instance, bool.Parse(value));
                                break;
                            case "int":
                            case "int32":
                                field.SetValue(PlayerInfo.Instance, int.Parse(value));
                                break;
                            case "float":
                            case "single":
                                field.SetValue(PlayerInfo.Instance, float.Parse(value));
                                break;
                            case "double":
                                field.SetValue(PlayerInfo.Instance, double.Parse(value));
                                break;
                            case "string":
                                field.SetValue(PlayerInfo.Instance, value);
                                break;
                        }
                    }
            }
    }

    public void UpdatePlayerInfo()
    {
        _isReservedUpdatePlayerInfo = true;
    }

    private void DoUpdatePlayerInfo()
    {
        var param = new Param();

        var fields = PlayerInfo.Instance.GetType().GetFields();
        foreach (var field in fields)
            param.Add(field.Name, field.GetValue(PlayerInfo.Instance));

        Backend.GameData.Insert("player_info", param);
    }

    #region Utils

    public static string WhichDataTypeIsIt(JsonData data, string key)
    {
        if (data.Keys.Contains(key))
        {
            if (data[key].Keys.Contains("S")) // string
                return "S";
            if (data[key].Keys.Contains("N")) // number
                return "N";
            if (data[key].Keys.Contains("M")) // map
                return "M";
            if (data[key].Keys.Contains("L")) // list
                return "L";
            if (data[key].Keys.Contains("BOOL")) // boolean
                return "BOOL";
            if (data[key].Keys.Contains("NULL")) // null
                return "NULL";
            return null;
        }

        return null;
    }

    #endregion

    [MessagePackObject]
    public class PacketBase
    {
        [Key(1)] public Hashtable hash;

        [Key(0)] public ePACKET_TYPE PacketType;
    }
}