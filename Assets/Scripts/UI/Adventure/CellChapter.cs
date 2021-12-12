using System;
using AirFishLab.ScrollingList;
using SheetData;
using UnityEngine;
using UnityEngine.UI;

public class CellChapter : ListBox
{
    private Action<CellChapter> _onClick;
    public Chapter Chatper;
    public Text Index;

    public RectTransform RectTransform;

    [ContextMenu("Setup")]
    private void Setup()
    {
        RectTransform = GetComponent<RectTransform>();
        Index = transform.Find("Root/Index").GetComponent<Text>();
    }

    public void SetOnClickEvent(Action<CellChapter> onClick)
    {
        _onClick = onClick;
    }

    protected override void UpdateDisplayContent(object contents)
    {
        Set(contents as Chapter);
    }
    
    private void Set(Chapter chapter)
    {
        Chatper = chapter;
        Index.text = (chapter.index + 1).ToString("D2");
    }

    public void OnClick()
    {
        _onClick?.Invoke(this);
    }
}