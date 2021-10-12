using UnityEngine;
using UnityEngine.UI;

public class SUIFitter : MonoBehaviour
{
    public Vector2 offset;
    public Text targetText;

    private RectTransform _rectTrn;
    //private float _prevWidth, _prevHeight;

    private void Awake()
    {
        if (targetText != null)
        {
            targetText.RegisterDirtyVerticesCallback(ChangeRect);
            ChangeRect();
        }
    }

    public void ChangeRect()
    {
        if (targetText == null)
            return;

        if (_rectTrn == null)
            _rectTrn = GetComponent<RectTransform>();

        _rectTrn.sizeDelta = new Vector2(targetText.preferredWidth + offset.x, targetText.preferredHeight + offset.y);
    }
}