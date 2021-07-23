using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using Violet;

namespace Server
{
    public class DBUnitInventory : DBBase
    {
        //Group , InDate
        public Dictionary<string, string> GroupIndate = new Dictionary<string, string>();

        public override string DB_KEY()
        {
            return "unit_inventory";
        }

        public override void DoUpdate()
        { 
            List<string> keys = new List<string>(UnitInventory.Instance.ChangedGroup.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                string group = keys[i];
                
                if (UnitInventory.Instance.ChangedGroup[group] == false) continue;
                UnitInventory.Instance.ChangedGroup[group] = false;
                
                var units = UnitInventory.Instance.Units[group];
                string json = JsonConvert.SerializeObject(units);

                Param param = new Param();
                param.Add("group", group);
                param.Add("units", json);

                if (GroupIndate.ContainsKey(group) == false)
                {
                    SendQueue.Enqueue(Backend.GameData.Insert,
                        DB_KEY(), param, bro =>
                        {
                            GroupIndate[group] = bro.GetInDate();
                            _onFinishUpdate?.Invoke();
                            _onFinishUpdate = null;
                        });
                }
                else
                {
                    SendQueue.Enqueue(Backend.GameData.Update,
                        DB_KEY(), GroupIndate[group], param,
                        bro =>
                        {
                            _onFinishUpdate?.Invoke();
                            _onFinishUpdate = null;
                        });
                }
            }

            isReservedUpdate = false;
        }

        public override void Download(Action onFinishDownload)
        {
            SendQueue.Enqueue(Backend.GameData.GetMyData, DB_KEY(), new Where(), 20, (bro) =>
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
                        var table = TableManager.Instance.GetTable<SheetData.Unit>();
                        foreach (var row in table)
                        {
                            var sheet = (row.Value as SheetData.Unit);
                            if(sheet.isPlayerUnit)
                                UnitInventory.Instance.Insert(sheet);
                        }

                        Update(null);
                    }
                    else
                    {
                        var rows = bro.Rows();
                        foreach (JsonData row in rows)
                        {
                            string inDate = row["inDate"]["S"].ToString();
                            string json = row["units"]["S"].ToString();
                            string group = row["group"]["S"].ToString();

                            //로컬에 반영
                            if (GroupIndate.ContainsKey(group) == false)
                                GroupIndate.Add(group, inDate);

                            if(UnitInventory.Instance.Units.ContainsKey(group) == false)
                                UnitInventory.Instance.Units.Add(group,new List<UnitInventory.Unit>());
                            UnitInventory.Instance.Units[group].Clear();
                            UnitInventory.Instance.Units[group] = JsonConvert.DeserializeObject<List<UnitInventory.Unit>>(json);
                        }
                    }
                }

                onFinishDownload?.Invoke();
            });
        }
    }
}