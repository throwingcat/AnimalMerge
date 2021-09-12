using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Define;
using LitJson;
using MessagePack;
using Newtonsoft.Json;
using Packet;
using SheetData;
using UnityEngine;
using UnityEngine.Monetization;
using Violet;
using Random = UnityEngine.Random;

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
            DBList.Add(new DBInventory());
            DBList.Add(new DBChestInventory());
            DBList.Add(new DBUnitInventory());
            DBList.Add(new DBPlayerTracker());
            DBList.Add(new DBQuestInfo());
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
                case Packet.ePACKET_TYPE.CHEST_COMPLETE:
                    CompleteChest(packet);
                    break;
                case ePACKET_TYPE.UNIT_LEVEL_UP:
                    UnitLevelUpProcess(packet);
                    break;
                case ePACKET_TYPE.QUEST_REFRESH:
                    QuestRefresh(packet);
                    break;
                case ePACKET_TYPE.QUEST_COMPLETE:
                    QuestComplete(packet);
                    break;
                case ePACKET_TYPE.DAILY_QUEST_REWARD:
                    DailyQuestReward(packet);
                    break;
            }
        }

        #region Battle Result

        private void BattleResult(PacketBase packet)
        {
            var isWin = (bool) packet.hash["is_win"];

            Action<PacketBase> onFinishBattleResult = (p) =>
            {
                if (packet.hash.ContainsKey("tracker_json"))
                {
                    string json = packet.hash["tracker_json"].ToString();
                    UpdateTracker(json, () => { SendPacket(p); });
                }
                else
                    SendPacket(p);
            };
            //패배 처리
            if (isWin == false)
            {
                PlayerInfo.Instance.RankScore -= 5;

                //플레이어 정보 업데이트
                UpdateDB<DBPlayerInfo>(() => { onFinishBattleResult(packet); });
            }
            //승리 처리
            else
            {
                BattleWinProcess(() =>
                {
                    //스테이지 플레이 정보
                    if (packet.hash.ContainsKey("stage"))
                    {
                        var stage = (string) packet.hash["stage"];

                        if (PlayerTracker.Instance.Contains(stage) == false)
                        {
                            packet.hash.Add("first_clear", true);
                            PlayerTracker.Instance.Set(stage, 1);
                            UpdateDB<DBPlayerTracker>(() => { onFinishBattleResult(packet); });
                        }
                        else
                        {
                            packet.hash.Add("first_clear", false);
                            onFinishBattleResult(packet);
                        }
                    }
                    else
                    {
                        onFinishBattleResult(packet);
                    }
                });
            }
        }

        public void BattleWinProcess(Action onFinish)
        {
            PlayerInfo.Instance.RankScore += 5;

            UpdateDB<DBPlayerInfo>(() =>
            {
                //새로운 상자 추가
                var emptySlot = ChestInventory.Instance.GetEmptySlot();
                if (emptySlot != null)
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

                    foreach (var chest in ChestInventory.Instance.ChestSlots)
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
            UpdateDB<DBChestInventory>(() => { SendPacket(packet); });
        }

        public void CompleteChest(PacketBase packet)
        {
            string inDate = packet.hash["inDate"].ToString();
            var chest = ChestInventory.Instance.Get(inDate);

            int amount = chest.GetRewardAmount();
            int gold_amount = Random.Range(chest.Sheet.gold_min, chest.Sheet.gold_max);
            var rewards = GetChestReward(amount, gold_amount);
            ChestInventory.Instance.Remove(inDate);

            UpdateDB<DBChestInventory>(() =>
            {
                GetRewards(rewards, () =>
                {
                    PacketReward packetReward = new PacketReward();
                    packetReward.PacketType = Packet.ePACKET_TYPE.CHEST_COMPLETE;
                    packetReward.hash = packet.hash;
                    packetReward.Rewards = rewards;
                    SendPacket(packetReward);
                });
            });
        }

        public List<ItemInfo> GetChestReward(int reward_amount, int gold_amount)
        {
            //받을수 있는 목록 정리
            int max_exp = "13".ToTableData<UnitLevel>().total;
            List<string> get_table = new List<string>();
            foreach (var group in UnitInventory.Instance.Units)
            {
                foreach (var unit in group.Value)
                {
                    if (unit.Exp < max_exp)
                        get_table.Add(unit.Key);
                }
            }

            //무작위로 분리
            int pick_count = Random.Range(3, 7);
            List<int> randomize_spilt = new List<int>();
            int total_redomize_value = 0;
            for (int i = 0; i < pick_count; i++)
            {
                int rand = Random.Range(3, reward_amount - 3);
                total_redomize_value += rand;
                randomize_spilt.Add(rand);
            }

            //보상 추가
            List<ItemInfo> rewards = new List<ItemInfo>();
            for (int i = 0; i < pick_count; i++)
            {
                if (get_table.Count == 0) break;
                var result = Utils.RandomPickDefault(get_table);
                get_table.Remove(result);

                ItemInfo itemInfo = new ItemInfo()
                {
                    Key = result,
                    Type = eItemType.Card,
                    Amount = (int) ((randomize_spilt[i] / (float) total_redomize_value) * reward_amount),
                };
                rewards.Add(itemInfo);
            }

            //보상 확인
            int result_amount = 0;
            int high_amount_reward = 0;
            int high_amount_reward_index = -1;
            for (int i = 0; i < rewards.Count; i++)
            {
                if (high_amount_reward < rewards[i].Amount)
                {
                    high_amount_reward = rewards[i].Amount;
                    high_amount_reward_index = i;
                }

                result_amount += rewards[i].Amount;
            }

            result_amount = reward_amount - result_amount;

            //보상 숫자 정리
            if (high_amount_reward_index != -1)
            {
                rewards[high_amount_reward_index].Amount += result_amount;
            }

            //남은 보상만큼 골드 추가
            ItemInfo gold = new ItemInfo()
            {
                Key = "Coin",
                Type = eItemType.Currency,
                Amount = (int) (gold_amount + result_amount * 3f),
            };

            rewards.Add(gold);

            return rewards;
        }

        #endregion

        #region Lobby - Collection

        public void UnitLevelUpProcess(PacketBase packet)
        {
            string unitKey = packet.hash["unit_key"].ToString();

            bool isSuccess = UnitInventory.Instance.LevelUp(unitKey);
            UpdateDB<DBUnitInventory>(() =>
            {
                packet.hash.Add("success", isSuccess);
                SendPacket(packet);
            });
        }

        #endregion

        #region Lobby - Quest

        public void QuestRefresh(PacketBase packet)
        {
            var slot_index = int.Parse(packet.hash["slot_index"].ToString());
            var quest = QuestInfo.Instance.GetQuest();
            QuestInfo.Instance.QuestSlots[slot_index].Key = quest.key;
            QuestInfo.Instance.QuestSlots[slot_index].RefreshTime = GameManager.GetTime().AddMinutes(10);
            UpdateDB<DBQuestInfo>(() => { SendPacket(packet); });
        }

        public void QuestComplete(PacketBase packet)
        {
            var slot_index = int.Parse(packet.hash["slot_index"].ToString());
            List<ItemInfo> rewards = new List<ItemInfo>();
            //보상 획득
            QuestInfo.Instance.QuestPoint += QuestInfo.Instance.QuestSlots[slot_index].Sheet.Point;
            Inventory.Instance.Update(Key.ITEM_COIN, QuestInfo.Instance.QuestSlots[slot_index].Sheet.Coin);
            rewards.Add(new ItemInfo(Key.ITEM_COIN, QuestInfo.Instance.QuestSlots[slot_index].Sheet.Coin));

            //퀘스트 초기화
            var quest = QuestInfo.Instance.GetQuest();
            QuestInfo.Instance.QuestSlots[slot_index].Key = quest.key;
            QuestInfo.Instance.QuestSlots[slot_index].Count = 0;
            QuestInfo.Instance.QuestSlots[slot_index].RefreshTime = GameManager.GetTime().AddMinutes(10);

            //인벤토리 업데이트
            UpdateDB<DBInventory>(() =>
            {
                //퀘스트 업데이트
                UpdateDB<DBQuestInfo>(() =>
                {
                    PacketReward packetReward = new PacketReward();
                    packetReward.PacketType = Packet.ePACKET_TYPE.QUEST_COMPLETE;
                    packetReward.hash = packet.hash;
                    packetReward.Rewards = rewards;
                    SendPacket(packetReward);
                });
            });
        }

        public void DailyQuestReward(PacketBase packet)
        {
            var index = int.Parse(packet.hash["index"].ToString());

            if (QuestInfo.Instance.HasReward(index))
            {
                QuestInfo.Instance.ReceiveReward[index] = true;
                UpdateDB<DBQuestInfo>(() =>
                {
                    string key = string.Format("DailyReward_{0}", index);
                    var sheet = TableManager.Instance.GetData<DailyQuestRewardInfo>(key);
                    var chest = TableManager.Instance.GetData<Chest>(sheet.Reward);
                    var rewards = GetChestReward(chest.amount, Random.Range(chest.gold_min, chest.gold_max));

                    GetRewards(rewards, () =>
                    {
                        PacketReward packetReward = new PacketReward();
                        packetReward.PacketType = Packet.ePACKET_TYPE.DAILY_QUEST_REWARD;
                        packetReward.hash = packet.hash;
                        packetReward.Rewards = rewards;
                        SendPacket(packetReward);
                    });
                });
            }
            else
                SendPacket(packet);
        }

        #endregion

        #region Tracker

        public void UpdateTracker(string json, Action onFinish)
        {
            Dictionary<string, int> tracker = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

            //퀘스트 목록 갱신
            foreach (var quest in QuestInfo.Instance.QuestSlots)
            {
                if (tracker.ContainsKey(quest.Sheet.TrackerKey))
                    quest.Count += tracker[quest.Sheet.TrackerKey];
            }

            UpdateDB<DBQuestInfo>(() => { onFinish?.Invoke(); });
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

        public void SendPacket<T>(T packet) where T : PacketBase
        {
            var lz4Options =
                MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var bytes = MessagePackSerializer.Serialize(packet, lz4Options);

            NetworkManager.Instance.ReceivePacket(bytes);
        }

        public void GetRewards(List<ItemInfo> rewards, Action onFinish)
        {
            bool isUpdateUnit = false;
            bool isUpdateInventory = false;
            foreach (var item in rewards)
            {
                if (item.Type == eItemType.Card)
                {
                    UnitInventory.Instance.GainEXP(item.Key, item.Amount);
                    isUpdateUnit = true;
                }

                if (item.Type == eItemType.Currency)
                {
                    Inventory.Instance.Update(item.Key, item.Amount);
                    isUpdateInventory = true;
                }
            }

            if (isUpdateUnit && isUpdateInventory)
            {
                UpdateDB<DBUnitInventory>(() => { UpdateDB<DBInventory>(onFinish); });
            }
            else if (isUpdateUnit)
            {
                UpdateDB<DBUnitInventory>(onFinish);
            }
            else if (isUpdateInventory)
            {
                UpdateDB<DBInventory>(onFinish);
            }
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

        [MessagePackObject]
        public class PacketReward : PacketBase
        {
            [Key(2)] public List<ItemInfo> Rewards = new List<ItemInfo>();
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