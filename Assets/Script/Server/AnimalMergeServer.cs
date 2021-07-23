using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Define;
using LitJson;
using MessagePack;
using SheetData;
using UnityEngine;
using Violet;

namespace Server
{
    public class AnimalMergeServer
    {
        private static AnimalMergeServer _instance;

        public List<DBBase> DBList = new List<DBBase>();

        public static AnimalMergeServer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AnimalMergeServer();
                return _instance;
            }
        }

        public AnimalMergeServer()
        {
            DBList.Add(new DBPlayerInfo());
            DBList.Add(new DBChestInventory());
            DBList.Add(new DBUnitInventory());
        }

        public void OnUpdate()
        {
            foreach (var DB in DBList)
            {
                if (DB.isReservedUpdate)
                    DB.DoUpdate();
            }
        }

        public void OnReceivePacket(byte[] bytes)
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            PacketBase packet = MessagePackSerializer.Deserialize<PacketBase>(bytes, lz4Options);

            switch (packet.PacketType)
            {
                case Packet.ePACKET_TYPE.REPORT_GAME_RESULT:
                    BattleResult(packet);
                    break;
                case Packet.ePACKET_TYPE.CHEST_PROGRESS:
                    ProgressChest(packet);
                    break;
            }
        }

        #region Battle Result

        private void BattleResult(PacketBase packet)
        {
            var isWin = (bool) packet.hash["is_win"];

            //패배 처리
            if (isWin == false)
            {
                PlayerInfo.Instance.RankScore -= 5;

                //플레이어 정보 업데이트
                UpdateDB<DBPlayerInfo>(() => { SendPacket(packet); });
            }
            //승리 처리
            else
            {
                BattleWinProcess(() => { SendPacket(packet);  });
            }
        }

        public void BattleWinProcess(Action onFinish)
        {
            PlayerInfo.Instance.RankScore += 5;

            UpdateDB<DBPlayerInfo>(() =>
            {
                //새로운 상자 추가
                if (ChestInventory.Instance.Chests.Count < EnvironmentValue.CHEST_SLOT_MAX_COUNT)
                {
                    //랜덤 상자 선택
                    var default_chest = TableManager.Instance.GetTable<Chest>().Where(
                        _ => (_.Value as Chest)?.group == "default");

                    List<double> random = new List<double>();
                    List<string> keys = new List<string>();
                    foreach (var row in default_chest)
                    {
                        Chest chest = (row.Value as Chest);
                        random.Add(chest.ratio);
                        keys.Add(chest.key);
                    }

                    int index = Utils.RandomPick(random);

                    ChestInventory.Instance.Insert(keys[index]);

                    UpdateDB<DBChestInventory>(() => { onFinish?.Invoke(); });
                }
                else
                {
                    //보유한 상자중에 선택
                    List<string> inDate = new List<string>();

                    foreach (var chest in ChestInventory.Instance.Chests)
                    {
                        if (chest.Grade < EnvironmentValue.CHEST_GRADE_MAX && chest.isProgress == false)
                            inDate.Add(chest.inDate);
                    }

                    if (0 < inDate.Count)
                    {
                        string pick = Utils.RandomPickDefault(inDate);
                        ChestInventory.Instance.Upgrade(pick);
                        //DB 업데이트
                        UpdateDB<DBChestInventory>(() => { onFinish?.Invoke(); });
                    }
                    else
                    {
                        onFinish?.Invoke();
                    }
                }
            });
        }

        #endregion

        #region Lobby - Chest

        public void ProgressChest(PacketBase packet)
        {
            string inDate = packet.hash["inDate"].ToString();
            ChestInventory.Instance.Progress(inDate);
            UpdateDB<DBChestInventory>(() =>
            {
                SendPacket(packet); 
            });
        }

        public void CompleteChest()
        {
        }

        #endregion

        public void DownloadDB<T>(System.Action onFinish) where T : DBBase
        {
            var db = GetDB<T>();
            db.Download(onFinish);
        }

        public void UpdateDB<T>(System.Action onFinish) where T : DBBase
        {
            var db = GetDB<T>();
            db.Update(onFinish);
        }

        public void SendPacket(PacketBase packet)
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bytes = MessagePackSerializer.Serialize(packet, lz4Options);
            
            NetworkManager.Instance.ReceivePacket(bytes);
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

        public static void ApplyField(FieldInfo field, object target, JsonData row, string key)
        {
            var type = WhichDataTypeIsIt(row, key);
            var value = row[key][type].ToString();

            var fieldType = field.FieldType.Name;
            fieldType = fieldType.ToLower();

            switch (fieldType)
            {
                case "bool":
                case "boolean":
                    field.SetValue(target, bool.Parse(value));
                    break;
                case "int":
                case "int32":
                    field.SetValue(target, int.Parse(value));
                    break;
                case "float":
                case "single":
                    field.SetValue(target, float.Parse(value));
                    break;
                case "double":
                    field.SetValue(target, double.Parse(value));
                    break;
                case "string":
                    field.SetValue(target, value);
                    break;
                case "datetime":
                    field.SetValue(target, DateTime.Parse(value));
                    break;
                default:
                    Debug.LogFormat("Not supported type {0}", fieldType);
                    break;
            }
        }

        #endregion

        [MessagePackObject]
        public class PacketBase
        {
            [Key(0)] public Packet.ePACKET_TYPE PacketType;
            [Key(1)] public Hashtable hash;
        }

        public T GetDB<T>() where T : DBBase
        {
            foreach (var db in DBList)
            {
                if (db is T)
                    return db as T;
            }

            return null;
        }
    }
}