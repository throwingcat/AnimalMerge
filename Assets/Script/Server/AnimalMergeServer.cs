using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BackEnd;
using Define;
using LitJson;
using MessagePack;
using Packet;
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
        }
        
        public void OnUpdate()
        {
            foreach (var DB in DBList)
            {
                if (DB.isReservedUpdate)
                    DB.DoUpdate();
            }
        }

        public void ReceivePacket(byte[] bytes)
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
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

            //패배 처리
            if (isWin == false)
            {
                PlayerInfo.Instance.RankScore -= 5;
                
                //플레이어 정보 업데이트
                UpdateDB<DBPlayerInfo>(
                    () =>
                    {
                        NetworkManager.Instance.ReceivePacket(new ReceivePacket()
                        {
                            GUID = packet.hash["packet_guid"].ToString(),
                            packet = new Packet.PacketBase(),
                        });
                    });
            }
            //승리 처리
            else
            {
                PlayerInfo.Instance.RankScore += 5; 

                UpdateDB<DBPlayerInfo>(() =>
                {
                    if (ChestInventory.Instance.Chests.Count < EnvironmentValue.CHEST_SLOT_MAX_COUNT)
                    {
                        //상자 지급
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

                        UpdateDB<DBChestInventory>(() =>
                        {
                            NetworkManager.Instance.ReceivePacket(new ReceivePacket()
                            {
                                GUID = packet.hash["packet_guid"].ToString(),
                                packet = new Packet.PacketBase(),
                            });
                        });
                    }
                    else
                    {
                        NetworkManager.Instance.ReceivePacket(new ReceivePacket()
                        {
                            GUID = packet.hash["packet_guid"].ToString(),
                            packet = new Packet.PacketBase(),
                        });
                    }
                });
            }
        }
    
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

        public static void ApplyField(FieldInfo field,object target,JsonData row,string key)
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
            [Key(1)] public Hashtable hash;

            [Key(0)] public ePACKET_TYPE PacketType;
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