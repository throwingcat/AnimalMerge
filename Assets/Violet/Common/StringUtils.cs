using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Violet.Utility
{
    public class StringUtils
    {
        // Bool 타입 데이터에 숫자로 기입되있는 경우가 있어서 0 , 1 비교도 추가
        public const string BOOLEAN_TRUE_STRING_NUM = "1";
        public const string BOOLEAN_FALSE_STRING_NUM = "0";

        public static string NullString = "null";

        public static string ConvertUTF8String(string origin)
        {
            if (IsBase64String(origin) == false)
            {
                VioletLogger.LogError("올바른 UTF8 문자열이 아닙니다 [ " + origin + " ]");
                return "";
            }

            var bytes = Convert.FromBase64String(origin);
            return Encoding.UTF8.GetString(bytes);
        }


        public static string ConvertBase64String(string origin)
        {
            var bytes = Encoding.UTF8.GetBytes(origin);
            return Convert.ToBase64String(bytes);
        }


        public static bool IsBase64String(string str)
        {
            str = str.Trim();
            return str.Length % 4 == 0 && Regex.IsMatch(str, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
        }

        //
        //  기본 value type 변환 
        //
        public static int ToInt(string str)
        {
            return Convert.ToInt32(str);
        }

        public static short ToInt16(string str)
        {
            return Convert.ToInt16(str);
        }

        public static long ToInt64(string str)
        {
            return Convert.ToInt64(str);
        }

        public static byte ToByte(string str)
        {
            return Convert.ToByte(str);
        }

        public static sbyte ToSbyte(string str)
        {
            return Convert.ToSByte(str);
        }

        public static float ToFloat(string str)
        {
            return Convert.ToSingle(str);
        }

        public static bool ToBool(string str)
        {
            if (BOOLEAN_TRUE_STRING_NUM.Equals(str))
                return true;
            if (BOOLEAN_FALSE_STRING_NUM.Equals(str))
                return false;

            return Convert.ToBoolean(str);
        }


        // Note : () 로 묶여 있으면 () 뒨위로 분리하고 없으면 ,표 단위로 분리한다
        public static string[] ToStringArray(string str)
        {
            string[] strArray;
            string[] seperator = {"),"};

            if (str.Contains("("))
            {
                strArray = str.Replace(" ", "").Split(seperator, StringSplitOptions.None);

                for (var i = 0; i < strArray.Length; i++) strArray[i] = strArray[i].Replace("(", "").Replace(")", "");
                return strArray;
            }

            strArray = str.Replace(" ", "").Split(',');
            return strArray;
        }

        // Note : (f , f) 형태 또는 f , f 형태의 스트링을 vector2로 변환
        public static Vector2 ToVector2(string str)
        {
            char[] trimOption = {'(', ')', ' '};
            var splits = str.Trim(trimOption).Split(',');

            if (splits.Length != 2) VioletLogger.LogError("[Vector2] 잘못된 값이 들어왔음! : " + str);


            return new Vector2(Convert.ToSingle(splits[0]), Convert.ToSingle(splits[1]));
        }

        // Note : (f , f , f) 형태 또는 f , f , f 형태의 스트링을 vector3로 변환
        public static Vector3 ToVector3(string str)
        {
            char[] trimOption = {'(', ')', ' '};
            var splits = str.Trim(trimOption).Split(',');

            if (splits.Length != 3) VioletLogger.LogError("[Vector3] 잘못된 값이 들어왔음! : " + str);

            return new Vector3(Convert.ToSingle(splits[0]), Convert.ToSingle(splits[1]), Convert.ToSingle(splits[2]));
        }

        // Note : RGBA(f , f , f , f) 형태 또는 f , f , f , f형태의 스트링을 color 로 변환
        public static Color ToColor(string str)
        {
            string[] splits;
            var header = "RGBA";

            str = str.Replace(" ", "").Replace(header, "").Replace("(", "").Replace(")", "").Replace(header, "");
            splits = str.Split(',');

            if (splits.Length != 4) VioletLogger.LogError("[Color] 잘못된 값이 들어왔음! : " + str);

            return new Color(Convert.ToSingle(splits[0]), Convert.ToSingle(splits[1]), Convert.ToSingle(splits[2]),
                Convert.ToSingle(splits[3]));
        }

        // Note : RGBA(f , f , f , f) 형태 또는 f , f , f , f형태의 스트링을 color 로 변환
        public static Color32 ToColor32(string str)
        {
            string[] splits;
            var header = "RGBA";

            str = str.Replace(" ", "").Replace(header, "").Replace("(", "").Replace(")", "").Replace(header, "");
            splits = str.Split(',');

            if (splits.Length != 4) VioletLogger.LogError("[Color] 잘못된 값이 들어왔음! : " + str);

            return new Color32(Convert.ToByte(splits[0]), Convert.ToByte(splits[1]), Convert.ToByte(splits[2]),
                Convert.ToByte(splits[3]));
        }

        public static string ChangeUpperDirectory(string path)
        {
            var s = path.Split('/');
            path = "";
            for (var i = 0; i < s.Length - 1; i++)
                if (i < s.Length - 2)
                    path += s[i] + "/";
                else
                    path += s[i];
            return path;
        }

        public static string RemoveExtention(string name)
        {
            var s = name.Split('.');
            name = s[0];
            return name;
        }

        public static string RemoveLastWord(string text, char separator)
        {
            var s = text.Split(separator);

            if (s.Length < 2) return text;

            text = "";
            for (var i = 0; i < s.Length - 1; i++) text += s[i] + separator;
            text = text.Remove(text.Length - 1, 1);

            return text;
        }

        public static string GetFileNameFromPath(string path)
        {
            var name = "";
            var s = path.Split('/');

            if (0 < s.Length)
                name = s[s.Length - 1];

            return name;
        }

        public static bool IsNullOrEmpty(string str)
        {
            if (str == null || str == string.Empty || str == NullString) return true;
            return false;
        }

        // (Clone) 스트링을 없애고 반환한다.
        public static string RemoveClone(string name)
        {
            return name.Replace("(Clone)", "");
        }

        public static string PrettyJson(string json, string indent)
        {
            var buffer = new StringBuilder();
            var level = 0;
            char target;
            var isStrOpen = false;

            var length = json.Length;
            for (var i = 0; i < length; i++)
            {
                target = json[i]; // json.Substring(i, i + 1);

                if (target.Equals('{'))
                {
                    buffer.Append(target);
                    if (0 < i && json[i - 1].Equals('"'))
                    {
                        isStrOpen = true;
                    }
                    else
                    {
                        buffer.Append('\n');
                        level++;
                        for (var j = 0; j < level; j++) buffer.Append(indent);
                    }
                }
                else if (target.Equals('['))
                {
                    buffer.Append(target);
                    if (0 < i && json[i - 1].Equals('"'))
                    {
                        isStrOpen = true;
                    }
                    else
                    {
                        buffer.Append(' ');
                        //buffer.Append('\n');
                        level++;
                        //for (int j = 0; j < level; j++)
                        //{
                        //    buffer.Append(indent);
                        //}
                    }
                }
                else if (target.Equals('}'))
                {
                    if (!isStrOpen)
                    {
                        buffer.Append('\n');
                        level--;
                        for (var j = 0; j < level; j++) buffer.Append(indent);
                    }
                    else
                    {
                        if (json[i + 1].Equals('"')) isStrOpen = false;
                    }

                    buffer.Append(target);
                }
                else if (target.Equals(']'))
                {
                    if (!isStrOpen)
                    {
                        buffer.Append(' ');
                        //buffer.Append('\n');
                        level--;
                        //for (int j = 0; j < level; j++)
                        //{
                        //    buffer.Append(indent);
                        //}
                    }
                    else
                    {
                        if (json[i + 1].Equals('"')) isStrOpen = false;
                    }

                    buffer.Append(target);
                }
                else if (target.Equals(','))
                {
                    buffer.Append(target);
                    if (!isStrOpen)
                    {
                        buffer.Append('\n');
                        for (var j = 0; j < level; j++) buffer.Append(indent);
                    }
                }
                else
                {
                    buffer.Append(target);
                }
            }

            return buffer.ToString();
        }

        public static string GetTimeText(int sec)
        {
            if (sec > 86400)
                return sec / 86400 + "일";
            if (sec > 3600)
                return sec / 3600 + "시간";
            if (sec > 60)
                return sec / 60 + "분";

            return sec + "초";
        }

        public static int Contains(string dest, string src)
        {
            var contains = 0;

            for (var i = 0; i <= dest.Length - src.Length; i++)
            {
                var word = dest.Substring(i, src.Length);
                if (word.Equals(src))
                    contains++;
            }

            return contains;
        }
    }
}