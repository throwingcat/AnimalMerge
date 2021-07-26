using System;
using System.Collections.Generic;
using System.Security.Permissions;
using BackEnd;
using Define;
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
            foreach (var chest in ChestInventory.Instance.ChestSlots)
            {
                if (chest.isChanged)
                {
                    var param = new Param();

                    var fields = chest.GetType().GetFields();
                    foreach (var field in fields)
                    {
                        if (field.Name == "inDate") continue;
                        if (field.Name == "isChanged") continue;
                        
                        if(field.FieldType == typeof(DateTime))
                            param.Add(field.Name, field.GetValue(chest).ToString());
                        else
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
                    
                    ChestInventory.Instance.ChestSlots = new ChestInventory.ChestSlot[EnvironmentValue.CHEST_SLOT_MAX_COUNT];
                    int index = 0;
                    foreach (JsonData row in rows)
                    {
                        string inDate = row["inDate"]["S"].ToString();

                        //로컬에 반영
                        if (ChestInventory.Instance.isContains(inDate) == false)
                        {
                            //데이터 추가
                            ChestInventory.Instance.ChestSlots[index] = new ChestInventory.ChestSlot();
                            ChestInventory.Instance.ChestSlots[index].inDate = inDate;
                            index++;
                        }
                        
                        foreach (var chest in ChestInventory.Instance.ChestSlots)
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
                
                Array.Sort(ChestInventory.Instance.ChestSlots,
                (a, b) =>
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