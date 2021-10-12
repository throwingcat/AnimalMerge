using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Violet
{
    public class TableManager
    {
        private static TableManager _instance;

        private readonly Dictionary<string, Dictionary<string, CSVDataBase>> Table =
            new Dictionary<string, Dictionary<string, CSVDataBase>>();

        public static TableManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TableManager();
                return _instance;
            }
        }
        
        public void Load()
        {
            foreach (var csv in GameManager.Instance.CSVDownloadConfig.Config.Files)
            {
                //테이블 추가
                if (Table.ContainsKey(csv.Name) == false)
                    Table.Add(csv.Name, new Dictionary<string, CSVDataBase>());

                //데이터 추가
                System.Type type = Type.GetType("SheetData." + csv.Name);

                //CSV 로드
                string file = string.Format("{0}.csv", csv.Name);
                var path = Path.Combine(CSVDownloadConfig.DOWNLOAD_PATH, file);

                if (CSVDownloadConfig.CSV.ContainsKey(path) == false)
                {
                    var text = File.ReadAllText(path);
                    CSVDownloadConfig.CSV.Add(path, text);
                }

                var list = CSVReader.Read(new TextAsset(CSVDownloadConfig.CSV[path]));
                foreach (var row in list)
                {
                    string key = "";
                    var instance = Activator.CreateInstance(type);

                    foreach (var element in row)
                    {
                        string culumn = element.Key;
                        object value = element.Value;
                        var pi = type.GetProperty(culumn);
                        if (pi != null)
                        {
                            pi.SetValue(instance, Convert.ChangeType(value, pi.PropertyType));

                            if (culumn.Equals("key") || culumn.Equals("Key"))
                                key = value.ToString();

                            continue;
                        }

                        var fi = type.GetField(culumn);
                        if (fi != null)
                        {
                            fi.SetValue(instance, Convert.ChangeType(value, fi.FieldType));

                            if (culumn.Equals("key") || culumn.Equals("Key"))
                                key = value.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(key) == false)
                    {
                        Table[csv.Name].Add(key, instance as CSVDataBase);
                    }
                }
            }

            foreach (var t in Table)
            {
                var type = Type.GetType("SheetData." + t.Key);
                foreach (var v in t.Value)
                {
                    var mi = type.GetMethod("Initialize");
                    mi.Invoke(v.Value, null);
                }
            }
        }

        public Dictionary<string, CSVDataBase> GetTable<T>() where T : CSVDataBase
        {
            var key = typeof(T).Name;
            if (Table.ContainsKey(key))
                return Table[key];

            return new Dictionary<string, CSVDataBase>();
        }

        public T GetData<T>(string key) where T : CSVDataBase
        {
            var tableKey = typeof(T).Name;
            if (Table.ContainsKey(tableKey))
                if (Table[tableKey].ContainsKey(key))
                    return Table[tableKey][key] as T;

            return default;
        }
    }
}