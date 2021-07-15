using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace SheetData
{
    public partial class EnvironmentValueSheet
    {
        public override void Initialize()
        {
            var type = typeof(Define.EnvironmentValue);
            var field = type.GetField(key, BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                string fieldType = field.FieldType.Name;
                fieldType = fieldType.ToLower();
                switch (fieldType)
                {
                    case "bool":
                        field.SetValue(null, bool.Parse(value));
                        break;
                    case "int":
                        field.SetValue(null, int.Parse(value));
                        break;
                    case "float":
                    case "single":
                        field.SetValue(null, float.Parse(value));
                        break;
                    case "double":
                        field.SetValue(null, double.Parse(value));
                        break;
                    case "string":
                        field.SetValue(null, value);
                        break;
                }
                
            }

            base.Initialize();
        }
    }
}