using System;
using System.Collections;
using System.Collections.Generic;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class CellChapter : MonoBehaviour
{
    public Text Index;
    public RectTransform RectTransform;
    public Vector3 OrigianlPosition;
    public Chapter Chatper;

    private Action<CellChapter> _onClick;

    void Reset()
    {
        RectTransform = GetComponent<RectTransform>();
        Index = transform.Find("Index").GetComponent<Text>();
    }

    public void Initialize()
    {
        OrigianlPosition = RectTransform.anchoredPosition;
    }

    public void Set(Chapter chapter, Action<CellChapter> onClick)
    {
        Chatper = chapter;
        _onClick = onClick;
        Index.text = (chapter.index + 1).ToString("D2");
    }

    public void OnClick()
    {
        _onClick?.Invoke(this);
    }
}