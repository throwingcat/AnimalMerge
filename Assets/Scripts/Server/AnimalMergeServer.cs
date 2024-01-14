using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Define;
using MessagePack;
using Newtonsoft.Json;
using SheetData;
using UnityEngine;
using Violet;
using Random = UnityEngine.Random;

namespace Server
{
    public class AnimalMergeServer
    {
        private static AnimalMergeServer _instance;

        private readonly float _updateDelay = 1f;
        private float _updateDelta = 1f;

        public List<DBBase> DBList = new List<DBBase>();

        public AnimalMergeServer()
        {
            DBList.Add(new DBAchievement());
            DBList.Add(new DBBattlePassInfo());
            DBList.Add(new DBPlayerInfo());
            DBList.Add(new DBInventory());
            DBList.Add(new DBChestInventory());
            DBList.Add(new DBUnitInventory());
            DBList.Add(new DBPlayerTracker());
            DBList.Add(new DBQuestInfo());
        }

        public static AnimalMergeServer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AnimalMergeServer();
                return _instance;
            }
        }

        public void OnUpdate()
        {
            _updateDelta += Time.deltaTime;
            foreach (var DB in DBList)
            {
                if (DB.isReservedUpdate)
                {
                    if (_updateDelay <= _updateDelta)
                    {
                        DB.Save();
                        _updateDelta = 0f;
                        break;
                    }
                }
            }
        }

        public void OnReceivePacket(byte[] bytes)
        {
            var packet = MessagePackSerializer.Deserialize<PacketBase>(bytes);

            switch (packet.PacketType)
            {
                case ePacketType.REPORT_GAME_RESULT:
                    BattleResult(packet);
                    break;
                case ePacketType.CHEST_PROGRESS:
                    ProgressChest(packet);
                    break;
                case ePacketType.CHEST_COMPLETE:
                    CompleteChest(packet);
                    break;
                case ePacketType.UNIT_LEVEL_UP:
                    UnitLevelUpProcess(packet);
                    break;
                case ePacketType.QUEST_REFRESH:
                    QuestRefresh(packet);
                    break;
                case ePacketType.QUEST_COMPLETE:
                    QuestComplete(packet);
                    break;
                case ePacketType.DAILY_QUEST_REWARD:
                    DailyQuestReward(packet);
                    break;
                case ePacketType.CHANGE_HERO:
                    ChangeHero(packet);
                    break;
                case ePacketType.PURCHASE_PREMIUM_PASS:
                    PurchasePremiumPass(packet);
                    break;
                case ePacketType.RECEIVE_PASS_REWARD:
                    ReceivePassReward(packet);
                    break;
                case ePacketType.RECEIVE_PLAYER_LEVEL_REWARD:
                    ReceivePlayerLevelReward(packet);
                    break;
            }
        }

        #region Lobby - Collection

        public void UnitLevelUpProcess(PacketBase packet)
        {
            var unitKey = packet.hash["unit_key"].ToString();

            var isSuccess = UnitInventory.Instance.LevelUp(unitKey);
            if (isSuccess) PlayerTracker.Instance.Report(PlayerTracker.UPGRADE_ANY, 1);
            UpdateDB<DBUnitInventory>(() =>
            {
                packet.hash.Add("success", isSuccess);
                if (isSuccess)
                    UpdateDB<DBPlayerTracker>(() => { SendPacket(packet); });
                else
                    SendPacket(packet);
            });
        }

        #endregion

        public void DownloadDB<T>(Action onFinish) where T : DBBase
        {
            var db = GetDB<T>();
            db.Load(onFinish);
        }

        public void UpdateDB<T>(Action onFinish) where T : DBBase
        {
            var db = GetDB<T>();
            db.SaveReserve(onFinish);
        }

        public void SendPacket<T>(T packet) where T : PacketBase
        {
            var bytes = MessagePackSerializer.Serialize(packet);
            //NetworkManager.Instance.ReceivePacket(bytes);
        }

        public void GetRewards(List<ItemInfo> rewards, Action onFinish)
        {
            var isUpdateUnit = false;
            var isUpdateInventory = false;
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
                UpdateDB<DBUnitInventory>(() => { UpdateDB<DBInventory>(onFinish); });
            else if (isUpdateUnit)
                UpdateDB<DBUnitInventory>(onFinish);
            else if (isUpdateInventory) UpdateDB<DBInventory>(onFinish);
        }

        public T GetDB<T>() where T : DBBase
        {
            foreach (var db in DBList)
                if (db is T)
                    return db as T;

            return null;
        }

        #region Player Level Reward

        public void ReceivePlayerLevelReward(PacketBase packet)
        {
            var key = packet.hash["level_key"].ToString();
            var rewards = GetDB<DBPlayerInfo>().PlayerInfo.ReceiveReward(key);
            GetRewards(rewards, () =>
            {
                UpdateDB<DBBattlePassInfo>(() =>
                {
                    var packetReward = new PacketReward();
                    packetReward.PacketType = ePacketType.RECEIVE_PLAYER_LEVEL_REWARD;
                    packetReward.hash = packet.hash;
                    packetReward.Rewards = rewards;
                    SendPacket(packetReward);
                });
            });
        }

        #endregion

        #region Lobby - Hero Select

        private void ChangeHero(PacketBase packet)
        {
            var hero = packet.hash["hero"].ToString();

            GetDB<DBPlayerInfo>().PlayerInfo.attribute.SelectHero = hero;
            UpdateDB<DBPlayerInfo>(() => { SendPacket(packet); });
        }

        #endregion

        #region Tracker

        public void ReportAchievement(Dictionary<string, ulong> tracker, Action onFinish)
        {
            //업적 목록 갱신
            var isUpdateAchievement = false;
            foreach (var t in tracker)
            {
                if (t.Value == 0) continue;

                var isUpdate = AchievementInfo.Instance.Report(t.Key, t.Value);
                if (isUpdateAchievement == false)
                    isUpdateAchievement = isUpdate;
            }

            if (isUpdateAchievement)
                UpdateDB<DBAchievement>(() => { onFinish?.Invoke(); });
            else
                onFinish?.Invoke();
        }

        public void ReportQuest(Dictionary<string, ulong> tracker, Action onFinish)
        {
            var isUpdate = false;
            foreach (var quest in QuestInfo.Instance.QuestSlots)
                if (tracker.ContainsKey(quest.Sheet.TrackerKey))
                {
                    quest.Count += tracker[quest.Sheet.TrackerKey];
                    isUpdate = true;
                }

            if (isUpdate)
                UpdateDB<DBQuestInfo>(() => { onFinish?.Invoke(); });
            else
                onFinish?.Invoke();
        }

        #endregion

        #region Battle Result

        private void BattleResult(PacketBase packet)
        {
            var isWin = (bool) packet.hash["is_win"];

            if (packet.hash.ContainsKey("tracker_json"))
            {
                var json = packet.hash["tracker_json"].ToString();
                var tracker = JsonConvert.DeserializeObject<Dictionary<string, ulong>>(json);
                foreach (var t in tracker)
                    PlayerTracker.Instance.Report(t.Key, t.Value);
            }

            Action<PacketBase> onFinishBattleResult = p =>
            {
                GetBattlePassPoint(isWin ? 5 : 3, () => { SendPacket(p); });
            };

            PlayerTracker.Instance.Report(PlayerTracker.BATTLE_PLAY, 1);

            //패배 처리
            if (isWin == false)
            {
                PlayerTracker.Instance.Report(PlayerTracker.BATTLE_LOSE, 1);
                BattleLoseProcess_PlayerInfo(() =>
                {
                    UpdateDB<DBPlayerTracker>(() => { onFinishBattleResult(packet); });
                });
            }
            //승리 처리
            else
            {
                PlayerTracker.Instance.Report(PlayerTracker.BATTLE_WIN, 1);
                BattleWinProcess(() =>
                {
                    //스테이지 플레이 정보
                    if (packet.hash.ContainsKey("stage"))
                    {
                        var stage = (string) packet.hash["stage"];

                        if (PlayerTracker.Instance.Contains(stage) == false)
                        {
                            packet.hash.Add("first_clear", true);
                            PlayerTracker.Instance.Report(stage, 1);
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
            //종료 확인
            var res_count = 0;
            Action checkFinish = () =>
            {
                if (res_count == 0)
                    onFinish?.Invoke();
            };

            //보상 지급
            res_count++;
            BattleWinProcess_Reward(() =>
            {
                res_count--;
                checkFinish?.Invoke();
            });

            //플레이어 정보 갱신
            res_count++;
            BattleWinProcess_PlayerInfo(() =>
            {
                res_count--;
                checkFinish?.Invoke();
            });
        }

        private void BattleWinProcess_Reward(Action onFinish)
        {
            onFinish?.Invoke();
            return;
            //새로운 상자 추가
            var emptySlot = ChestInventory.Instance.GetEmptySlot();
            if (emptySlot != null)
            {
                //랜덤 상자 선택
                var default_chest = TableManager.Instance.GetTable<Chest>().Where(
                    _ => (_.Value as Chest)?.group == "default");

                var random = new List<double>();
                var keys = new List<string>();
                foreach (var row in default_chest)
                {
                    var chest = row.Value as Chest;
                    random.Add(chest.ratio);
                    keys.Add(chest.key);
                }

                var index = Utils.RandomPick(random);

                ChestInventory.Instance.Insert(keys[index]);

                UpdateDB<DBChestInventory>(() => { onFinish?.Invoke(); });
            }
            else
            {
                //보유한 상자중에 선택
                var inDate = new List<string>();

                foreach (var chest in ChestInventory.Instance.ChestSlots)
                    if (chest.grade < EnvironmentValue.CHEST_GRADE_MAX && chest.isProgress == false)
                        inDate.Add(chest.guid);

                if (0 < inDate.Count)
                {
                    var pick = Utils.RandomPickDefault(inDate);
                    ChestInventory.Instance.Upgrade(pick);
                    //DB 업데이트
                    UpdateDB<DBChestInventory>(() => { onFinish?.Invoke(); });
                }
                else
                {
                    onFinish?.Invoke();
                }
            }
        }

        private void BattleWinProcess_PlayerInfo(Action onFinish)
        {
            //플레이어 정보 갱신            
            var PlayerInfo = GetDB<DBPlayerInfo>().PlayerInfo;
            PlayerInfo.attribute.RankScore += 5;
            PlayerInfo.attribute.ChestPoint += 30;
            PlayerInfo.GetExp(15);
            UpdateDB<DBPlayerInfo>(() => { onFinish?.Invoke(); });
        }

        private void BattleLoseProcess_PlayerInfo(Action onFinish)
        {
            //플레이어 정보 갱신            
            var PlayerInfo = GetDB<DBPlayerInfo>().PlayerInfo;
            PlayerInfo.attribute.RankScore += 3;
            PlayerInfo.GetExp(7);
            UpdateDB<DBPlayerInfo>(() => { onFinish?.Invoke(); });
        }

        #endregion

        #region Lobby - Chest

        public void ProgressChest(PacketBase packet)
        {
            var inDate = packet.hash["inDate"].ToString();
            ChestInventory.Instance.Progress(inDate);
            UpdateDB<DBChestInventory>(() => { SendPacket(packet); });
        }

        public void CompleteChest(PacketBase packet)
        {
            var inDate = packet.hash["inDate"].ToString();
            var chest = ChestInventory.Instance.Get(inDate);

            var amount = chest.GetRewardAmount();
            var gold_amount = Random.Range(chest.Sheet.gold_min, chest.Sheet.gold_max);
            var rewards = GetChestReward(amount, gold_amount);
            ChestInventory.Instance.Remove(inDate);

            UpdateDB<DBChestInventory>(() =>
            {
                GetRewards(rewards, () =>
                {
                    var packetReward = new PacketReward();
                    packetReward.PacketType = ePacketType.CHEST_COMPLETE;
                    packetReward.hash = packet.hash;
                    packetReward.Rewards = rewards;
                    SendPacket(packetReward);
                });
            });
        }

        public List<ItemInfo> GetChestReward(int reward_amount, int gold_amount)
        {
            //받을수 있는 목록 정리
            var max_exp = "13".ToTableData<UnitLevel>().total;
            var get_table = new List<string>();
            foreach (var group in UnitInventory.Instance.Units)
            foreach (var unit in group.Value)
                if (unit.Exp < max_exp)
                    get_table.Add(unit.Key);

            //무작위로 분리
            var pick_count = Random.Range(3, 7);
            var randomize_spilt = new List<int>();
            var total_redomize_value = 0;
            for (var i = 0; i < pick_count; i++)
            {
                var rand = Random.Range(3, reward_amount - 3);
                total_redomize_value += rand;
                randomize_spilt.Add(rand);
            }

            //보상 추가
            var rewards = new List<ItemInfo>();
            for (var i = 0; i < pick_count; i++)
            {
                if (get_table.Count == 0) break;
                var result = Utils.RandomPickDefault(get_table);
                get_table.Remove(result);

                var itemInfo = new ItemInfo
                {
                    Key = result,
                    Type = eItemType.Card,
                    Amount = (int) (randomize_spilt[i] / (float) total_redomize_value * reward_amount)
                };
                rewards.Add(itemInfo);
            }

            //보상 확인
            var result_amount = 0;
            var high_amount_reward = 0;
            var high_amount_reward_index = -1;
            for (var i = 0; i < rewards.Count; i++)
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
            if (high_amount_reward_index != -1) rewards[high_amount_reward_index].Amount += result_amount;

            //남은 보상만큼 골드 추가
            var gold = new ItemInfo
            {
                Key = "Coin",
                Type = eItemType.Currency,
                Amount = (int) (gold_amount + result_amount * 3f)
            };

            rewards.Add(gold);

            return rewards;
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
            var rewards = new List<ItemInfo>();
            //보상 획득
            QuestInfo.Instance.QuestPoint += QuestInfo.Instance.QuestSlots[slot_index].Sheet.Point;
            Inventory.Instance.Update(Key.ITEM_COIN, QuestInfo.Instance.QuestSlots[slot_index].Sheet.Coin);
            rewards.Add(new ItemInfo(Key.ITEM_COIN, QuestInfo.Instance.QuestSlots[slot_index].Sheet.Coin));

            //퀘스트 초기화
            var quest = QuestInfo.Instance.GetQuest();
            QuestInfo.Instance.QuestSlots[slot_index].Key = quest.key;
            QuestInfo.Instance.QuestSlots[slot_index].Count = 0;
            QuestInfo.Instance.QuestSlots[slot_index].RefreshTime = GameManager.GetTime().AddMinutes(10);

            PlayerTracker.Instance.Report(PlayerTracker.QUEST_CLEAR, 1);

            //인벤토리 업데이트
            UpdateDB<DBInventory>(() =>
            {
                //퀘스트 업데이트
                UpdateDB<DBQuestInfo>(() =>
                {
                    UpdateDB<DBPlayerTracker>(() =>
                    {
                        var packetReward = new PacketReward();
                        packetReward.PacketType = ePacketType.QUEST_COMPLETE;
                        packetReward.hash = packet.hash;
                        packetReward.Rewards = rewards;
                        SendPacket(packetReward);
                    });
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
                    var key = string.Format("DailyReward_{0}", index);
                    var sheet = TableManager.Instance.GetData<DailyQuestRewardInfo>(key);
                    var chest = TableManager.Instance.GetData<Chest>(sheet.Reward);
                    var rewards = GetChestReward(chest.amount, Random.Range(chest.gold_min, chest.gold_max));

                    GetRewards(rewards, () =>
                    {
                        var packetReward = new PacketReward();
                        packetReward.PacketType = ePacketType.DAILY_QUEST_REWARD;
                        packetReward.hash = packet.hash;
                        packetReward.Rewards = rewards;
                        SendPacket(packetReward);
                    });
                });
            }
            else
            {
                SendPacket(packet);
            }
        }

        #endregion

        #region Lobby - BattlePass

        public void GetBattlePassPoint(int point, Action onFinish)
        {
            ValidateBattlePassSeason(validate =>
            {
                if (validate)
                {
                    BattlePassInfo.Instance.Point += point;
                    UpdateDB<DBBattlePassInfo>(onFinish);
                }
                else
                {
                    onFinish?.Invoke();
                }
            });
        }

        public void PurchasePremiumPass(PacketBase packet)
        {
            ValidateBattlePassSeason(validate =>
            {
                if (validate)
                {
                    BattlePassInfo.Instance.isPurchasePremiumPass = true;
                    UpdateDB<DBBattlePassInfo>(() =>
                    {
                        var packetReward = new PacketReward();
                        packetReward.PacketType = ePacketType.RECEIVE_PASS_REWARD;
                        packetReward.hash = packet.hash;
                        packetReward.Rewards = new List<ItemInfo>();
                        SendPacket(packetReward);
                    });
                }
                else
                {
                    var packetReward = new PacketReward();
                    packetReward.PacketType = ePacketType.RECEIVE_PASS_REWARD;
                    packetReward.hash = packet.hash;
                    packetReward.Rewards = new List<ItemInfo>();
                    packetReward.SetError("season_expire");
                    SendPacket(packetReward);
                }
            });
        }

        public void ReceivePassReward(PacketBase packet)
        {
            ValidateBattlePassSeason(validate =>
            {
                if (validate)
                {
                    var key = packet.hash["pass_key"].ToString();
                    var rewards = BattlePassInfo.Instance.ReceiveReward(key);
                    GetRewards(rewards, () =>
                    {
                        UpdateDB<DBBattlePassInfo>(() =>
                        {
                            var packetReward = new PacketReward();
                            packetReward.PacketType = ePacketType.RECEIVE_PASS_REWARD;
                            packetReward.hash = packet.hash;
                            packetReward.Rewards = rewards;
                            SendPacket(packetReward);
                        });
                    });
                }
                else
                {
                    var packetReward = new PacketReward();
                    packetReward.PacketType = ePacketType.RECEIVE_PASS_REWARD;
                    packetReward.hash = packet.hash;
                    packetReward.Rewards = new List<ItemInfo>();
                    packetReward.SetError("season_expire");
                    SendPacket(packetReward);
                }
            });
        }

        public void ValidateBattlePassSeason(Action<bool> onFinish)
        {
            var isValidate = true;

            //참가한 시즌이 없음
            if (BattlePassInfo.Instance.JoinSeason == null)
                isValidate = false;
            //진행중인 시즌이 없음
            if (BattlePassInfo.CurrentSeason == null)
            {
                isValidate = false;
            }
            //현재 진행중이 시즌과 참가한 시즌이 다름
            else
            {
                if (BattlePassInfo.Instance.JoinSeasonKey != BattlePassInfo.CurrentSeason.key)
                    isValidate = false;
            }

            if (isValidate == false)
            {
                BattlePassInfo.Instance.JoinSeasonKey = BattlePassInfo.CurrentSeason == null ? "" : BattlePassInfo.CurrentSeason.key;
                BattlePassInfo.Instance.Point = 0;
                BattlePassInfo.Instance.RewardInfos = new List<BattlePassInfo.BattlePassRewardInfo>();
                BattlePassInfo.Instance.isPurchasePremiumPass = false;

                UpdateDB<DBBattlePassInfo>(() => { onFinish?.Invoke(BattlePassInfo.CurrentSeason != null); });
            }
            else
            {
                onFinish?.Invoke(BattlePassInfo.CurrentSeason != null);
            }
        }

        #endregion
    }
}