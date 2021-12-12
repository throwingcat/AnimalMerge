using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.UI;
#endif

public class ScrollRectDisableDrag : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData)
    {
    }

    public override void OnDrag(PointerEventData eventData)
    {
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ScrollRectDisableDrag))]
public class ScrollRectDisableDragEditor : ScrollRectEditor
{
}
#endif