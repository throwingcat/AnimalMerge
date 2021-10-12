using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;
using Random = UnityEngine.Random;

public class Utils
{
    private static Vector2 _lastInputPosition = Vector2.zero;

    public static void SetLayer(int layer, GameObject go)
    {
        var t = go.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < t.Length; i++)
            t[i].gameObject.layer = layer;
    }

    public static void SetLayer(string layer, GameObject go)
    {
        SetLayer(LayerMask.NameToLayer(layer), go);
    }

    public static Vector2 WorldToCanvas(Camera worldCam, Vector3 worldPos, RectTransform canvasRect)
    {
        Vector2 ViewportPosition = worldCam.WorldToViewportPoint(worldPos);
        var WorldObject_ScreenPosition = new Vector2(
            ViewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
            ViewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);

        return WorldObject_ScreenPosition;
    }

    public static void WorldToCanvas(ref RectTransform sourceRect, Camera worldCam, Vector3 worldPos,
        RectTransform canvasRect)
    {
        Vector2 ViewportPosition = worldCam.WorldToViewportPoint(worldPos);
        var WorldObject_ScreenPosition = new Vector2(
            ViewportPosition.x * canvasRect.sizeDelta.x - canvasRect.sizeDelta.x * 0.5f,
            ViewportPosition.y * canvasRect.sizeDelta.y - canvasRect.sizeDelta.y * 0.5f);

        sourceRect.anchoredPosition = WorldObject_ScreenPosition;
    }

    public static void ScreenToCanvas(Canvas canvas,Vector2 screen_pos,ref RectTransform rt)
    {
        var pos = Vector2.zero;
        var uiCamera = canvas.worldCamera;
        var canvasRect = canvas.GetComponent<RectTransform> ();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen_pos, uiCamera, out pos);
        rt.localPosition = pos;
    }

    public static T RandomPickDefault<T>(List<T> list)
    {
        if (list.Count == 0)
            return default;
        if (list.Count == 1)
            return list[0];
        var index = Random.Range(0, list.Count);
        return list[index];
    }

    public static int RandomPick(List<double> list)
    {
        var pick = 0;
        var rand_max = 1000000;
        var r = (double) Random.Range(0, rand_max) / rand_max;
        var dr = r * 100.0f;
        double cumulative = 0.0f;

        for (var i = 0; i < list.Count; i++)
        {
            cumulative += list[i];
            if (dr <= cumulative)
            {
                pick = i;
                break;
            }
        }

        return pick;
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        var rng = new System.Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static string ParseSeconds(long seconds)
    {
        if (seconds < 0) seconds = 0;
        var t = TimeSpan.FromSeconds(seconds);

        var result = "";

        //1시간 이상 HH:MM
        if (t.Hours > 0)
        {
            if (t.Minutes == 0)
                result = string.Format("{0}{1}", t.Hours, "Hour".ToLocalization());
            else
                result = string.Format("{0}{1} {2}{3}", t.Hours, "Hour".ToLocalization(), t.Minutes,
                    "Minute".ToLocalization());
        }
        //1시간 이하 
        else
        {
            //1분 이상 MM:SS
            if (t.Minutes > 0)
                result = string.Format("{0}{1} {2}{3}", t.Minutes, "Minute".ToLocalization(), t.Seconds,
                    "Seconds".ToLocalization());
            //1분 이하 SS
            else
                result = string.Format("{0}{1}", t.Seconds, "Seconds".ToLocalization());
        }

        return result;
    }

/*
    public static string ParseSeconds(long seconds)
    {
        if (seconds < 0) seconds = 0;
        var t = TimeSpan.FromSeconds(seconds);

        var result = "";

        //1시간 이상
        if (t.Hours > 0)
        {
            result = string.Format("{0}{1} {2}{3} {4}{5}", t.Hours, Localization.GetText("hour"), t.Minutes,
                Localization.GetText("minute"), t.Seconds, Localization.GetText("seconds"));
        }
        //1시간 이하
        else
        {
            //1분 이상
            if (t.Minutes > 0)
                result = string.Format("{0}{1} {2}{3}", t.Minutes, Localization.GetText("minute"), t.Seconds,
                    Localization.GetText("seconds"));
            //1분 이하
            else
                result = string.Format("{0}{1}", t.Seconds, Localization.GetText("seconds"));
        }

        return result;
    }

    public static string ParseSecondsSingle(long seconds)
    {
        if (seconds < 0) seconds = 0;
        var t = TimeSpan.FromSeconds(seconds);

        var result = "";

        //1시간 이상
        if (t.Hours > 0)
        {
            result = string.Format("{0}{1}", t.Hours, Localization.GetText("hour"));
        }
        //1시간 이하
        else
        {
            //1분 이상
            if (t.Minutes > 0)
                result = string.Format("{0}{1}", t.Minutes, Localization.GetText("minute"));
            //1분 이하
            else
                result = string.Format("{0}{1}", t.Seconds, Localization.GetText("seconds"));
        }

        return result;
    }

    public static string ParseSecondsSingleFloat(long seconds)
    {
        if (seconds < 0) seconds = 0;
        var t = TimeSpan.FromSeconds(seconds);

        var result = "";

        //1시간 이상
        if (t.Hours > 0)
        {
            var r = t.Hours + t.Minutes / 60f;
            result = string.Format("{0}{1}", r.ToString("F1"), Localization.GetText("hour"));
        }
        //1시간 이하
        else
        {
            //1분 이상
            if (t.Minutes > 0)
            {
                var r = t.Minutes + t.Seconds / 60f;
                result = string.Format("{0}{1}", r.ToString("F1"), Localization.GetText("minute"));
            }
            //1분 이하
            else
            {
                result = string.Format("{0}{1}", t.Seconds, Localization.GetText("seconds"));
            }
        }

        return result;
    }
*/
    public static string ParseComma(int value, string extention = "")
    {
        var r = value == 0 ? "0" : value.ToString("#,###");
        r = r + extention;
        return r;
    }

    public static void IncreasePrice(Text t, int from, int to, float time, string extention = "")
    {
        DOTween.To(() => from,
            x => t.text = ParseComma(x, extention),
            to, time);
    }

    public static string ConvertHugeValue(int value)
    {
        var text = "";
        if (value >= 1000000)
        {
            var f = value * 0.000001f;
            text = string.Format("{0:0.#}M", f);
        }
        else if (value >= 1000)
        {
            var f = value * 0.001f;
            text = string.Format("{0:0.#}K", f);
        }
        else
        {
            text = value.ToString();
        }

        return text;
    }

    public static string ConvertHugeValue(long value)
    {
        var text = "";
        if (value >= 1000000)
        {
            var f = value * 0.000001f;
            text = string.Format("{0:0.#}M", f);
        }
        else if (value >= 1000)
        {
            var f = value * 0.001f;
            text = string.Format("{0:0.#}K", f);
        }
        else
        {
            text = value.ToString();
        }

        return text;
    }

    public static string ConvertHugeValue(float value)
    {
        var text = "";
        if (value >= 1000000)
        {
            var f = value * 0.000001f;
            text = string.Format("{0:0.#}M", f);
        }
        else if (value >= 1000)
        {
            var f = value * 0.001f;
            text = string.Format("{0:0.#}K", f);
        }
        else
        {
            text = string.Format("{0:0.#}", value);
        }

        return text;
    }

    public static bool isNullOrEmpty(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        if (text == "None" || text == "none" || text == "Null" || text == "null")
            return true;

        return false;
    }

    public static bool GetTouchPhase(out TouchPhase phase)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            phase = TouchPhase.Began;
            _lastInputPosition = GetTouchPoint();
            return true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            phase = TouchPhase.Ended;
            return true;
        }

        if (Input.GetMouseButton(0))
        {
            var d = Vector2.Distance(_lastInputPosition, GetTouchPoint());
            if (d <= float.Epsilon)
                phase = TouchPhase.Stationary;
            else
                phase = TouchPhase.Moved;
            return true;
        }
#else
        if (Input.touchCount > 0)
        {
            phase = Input.GetTouch(0).phase;
            return true;
        }
#endif

        phase = TouchPhase.Ended;
        return false;
    }

    public static Vector2 GetTouchPoint()
    {
#if UNITY_EDITOR
        return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#else
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            return t.position;
        }
        return Vector2.zero;
#endif
    }

    public static T EnumParse<T>(string value) where T : struct
    {
        var result = default(T);
        var isParse = Enum.TryParse(value, out result);
        if (isParse) return result;

        Debug.LogError(string.Format("Enum Parse Error >> type : {0} value : {1}", typeof(T).Name, value));
        return default;
    }

    public static bool TryEnumParse<T>(string value, ref T result) where T : struct
    {
        result = default;
        return Enum.TryParse(value, out result);
    }

    public static void SetActiveCanvasGroup(CanvasGroup canvasGroup, bool isActive)
    {
        var from = isActive ? 0f : 1f;
        var to = isActive ? 1f : 0f;
        DOTween.To(() => from, value => { canvasGroup.alpha = value; }, to, 1f);

        canvasGroup.blocksRaycasts = isActive;
    }

    public static List<T> GetRandomPick<T>(List<T> list, int quantity)
    {
        var clone = new List<T>(list);
        var result = new List<T>();

        while (result.Count < quantity)
        {
            if (clone.Count <= 0) break;
            var index = Random.Range(0, clone.Count);
            result.Add(clone[index]);
            clone.RemoveAt(index);
        }

        return result;
    }

    public static decimal GetUnitDamage(int baseDamage, int level)
    {
        double ratio = 1.0 + level.ToString().ToTableData<UnitLevel>().upgrade_ratio;
        return (decimal) (baseDamage * 2 * ratio);
    }
}

public static class Extention
{
    public static bool Contains(this RectTransform rt, Vector2 point)
    {
        // Get the rectangular bounding box of your UI element
        var rect = rt.rect;

        // Get the left, right, top, and bottom boundaries of the rect
        var leftSide = rt.transform.position.x - rect.width / 2;
        var rightSide = rt.transform.position.x + rect.width / 2;
        var topSide = rt.transform.position.y + rect.height / 2;
        var bottomSide = rt.transform.position.y - rect.height / 2;

        //Debug.Log(leftSide + ", " + rightSide + ", " + topSide + ", " + bottomSide);

        // Check to see if the point is in the calculated bounds
        if (point.x >= leftSide &&
            point.x <= rightSide &&
            point.y >= bottomSide &&
            point.y <= topSide)
            return true;

        return false;
    }

    public static void LocalReset(this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }

    public static void LocalReset(this RectTransform t)
    {
        t.anchoredPosition = Vector2.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }


    public static string ToLocalization(this string s)
    {
        var sheet = s.ToTableData<Localization>();
        if (sheet == null)
            return s;
        return sheet.value;
    }


    public static bool IsNullOrEmpty(this string s)
    {
        return Utils.isNullOrEmpty(s);
    }

    public static Sprite ToSprite(this string s,string atlas = "Common")
    {
        return ResourceManager.Instance.GetSprite(atlas, s);
    }

    public static T ToTableData<T>(this string s) where T : CSVDataBase
    {
        return TableManager.Instance.GetData<T>(s);
    }

    public class WaitForSecondsOrEvent : CustomYieldInstruction
    {
        const float TicksPerSecond = TimeSpan.TicksPerSecond;
        private long _duration;
        private Func<bool> _exit;

        public WaitForSecondsOrEvent(float duration, Func<bool> exit)
        {
            _duration = DateTime.Now.Ticks + (long) (TicksPerSecond * duration);
            _exit = exit;
        }

        public override bool keepWaiting
        {
            get { return DateTime.Now.Ticks < _duration && _exit() == false; }
        }
    }

    public static Tweener ButtonPressPlay(this Transform t, float value = -0.05f)
    {
        return t.DOScale(value, 0.33f).SetRelative(true).SetEase(Ease.OutElastic).Play();
    }

    public static Tweener ButtonReleasePlay(this Transform t)
    {
        return t.DOScale(1f, 0.33f).SetRelative(false).SetEase(Ease.OutElastic).Play();
    }
}