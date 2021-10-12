using System.Collections;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PartCurtain : MonoBehaviour
{
    private bool _isInit;
    private Transform[] _trnList;
    private Transform _trnStaticCanvas;

    private void Init()
    {
        if (_isInit)
            return;

        _isInit = true;
        _trnStaticCanvas = transform.parent;
    }

    public void Show(Transform trn)
    {
        Init();

        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        if (trn == null)
            return;

        var addIndex = 0;

        if (_trnList == null)
            _trnList = transform.GetComponentsInChildren<Transform>(true);
        var layer = trn.gameObject.layer;
        foreach (var trnCurtain in _trnList)
            trnCurtain.gameObject.layer = layer;

        var targetIndex = trn.GetSiblingIndex();
        if (trn.parent == transform.parent)
            if (targetIndex > transform.GetSiblingIndex())
                addIndex = -1;

        transform.SetParent(trn.parent);
        transform.SetSiblingIndex(targetIndex + addIndex);
    }

    public void Hide()
    {
        if (!_isInit)
            return;

        if (transform.parent != _trnStaticCanvas)
            transform.SetParent(_trnStaticCanvas);

        gameObject.SetActive(false);
    }

    public void HideDirect()
    {
        if (_isInit == false)
            Init();

        if (transform.parent != _trnStaticCanvas)
            transform.parent = _trnStaticCanvas;
    }

    public void OnClickBackground()
    {
        if (SUIPanel.CurrentPanel.IsPopup)
            SUIPanel.CurrentPanel.BackPress();
    }
}