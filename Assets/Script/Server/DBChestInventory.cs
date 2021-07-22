using System;
using BackEnd;
using LitJson;

namespace Server
{
    public class DBChestInventory : DBBase
    {
        public override string DB_KEY()
        {
            return "chest_inventory";
        }

        public override void DoUpdate()
        {
            foreach (var chest in ChestInventory.Instance.Chests)
            {
                if (chest.isChanged)
                {
                    var param = new Param();

                    var fields = chest.GetType().GetFields();
                    foreach (var field in fields)
                    {
                        if (field.Name == "inDate") continue;
                        if (field.Name == "isChanged") continue;
                        param.Add(field.Name, field.GetValue(chest));
                    }

                    if (chest.inDate.IsNullOrEmpty())
                    {
                        SendQueue.Enqueue(Backend.GameData.Insert,
                            DB_KEY(), param, bro =>
                            {
                                chest.inDate = bro.GetInDate();
                                _onFinishUpdate?.Invoke();
                                _onFinishUpdate = null;
                            });
                    }
                    else
                    {
                        SendQueue.Enqueue(Backend.GameData.Update,
                            DB_KEY(), chest.inDate, param,
                            bro =>
                            {
                                _onFinishUpdate?.Invoke();
                                _onFinishUpdate = null;
                            });
                    }
                    chest.isChanged = false;
                }
            }

            isReservedUpdate = false;
        }

        public override void Download(Action onFinishDownload)
        {
            //뒤끝기반
            SendQueue.Enqueue(Backend.GameData.GetMyData, DB_KEY(), new Where(), 10, (bro) =>
            {
                if (bro.IsSuccess() == false)
                {
                    UnityEngine.Debug.Log(bro);
                }
                else
                {
                    var rows = bro.Rows();
                    foreach (JsonData row in rows)
                    {
                        string inDate = row["inDate"]["S"].ToString();

                        //로컬에 반영
                        if (ChestInventory.Instance.isContains(inDate) == false)
                        {
                            //데이터 추가
                            ChestInventory.Chest chest = new ChestInventory.Chest();
                            chest.inDate = inDate;
                            ChestInventory.Instance.Chests.Add(chest);
                        }

                        foreach (var chest in ChestInventory.Instance.Chests)
                        {
                            if (chest.inDate == inDate)
                            {
                                foreach (var key in row.Keys)
                                {
                                    var fields = chest.GetType().GetFields();
                                    foreach (var field in fields)
                                    {
                                        if (field.Name == key)
                                        {
                                            AnimalMergeServer.ApplyField(field, chest, row, key);
                                            break;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
                ChestInventory.Instance.Chests.Sort((a, b) =>
                {
                    if (a.GetTime < b.GetTime) return -1;
                    if (a.GetTime > b.GetTime) return 1;
                    return 0;
                });
                
                onFinishDownload?.Invoke();
            });
        }
    }
}