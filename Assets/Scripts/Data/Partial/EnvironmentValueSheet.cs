using System.Reflection;
using Define;

namespace SheetData
{
    public partial class EnvironmentValueSheet
    {
        public override void Initialize()
        {
            var type = typeof(EnvironmentValue);
            var field = type.GetField(key, BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                var fieldType = field.FieldType.Name;
                fieldType = fieldType.ToLower();
                switch (fieldType)
                {
                    case "bool":
                    case "boolean":
                        field.SetValue(null, bool.Parse(value));
                        break;
                    case "int":
                    case "int32":
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