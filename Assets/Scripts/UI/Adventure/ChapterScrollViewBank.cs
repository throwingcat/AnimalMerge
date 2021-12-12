using System.Collections;
using System.Collections.Generic;
using AirFishLab.ScrollingList;
using SheetData;
using UnityEngine;

public class ChapterScrollViewBank : BaseListBank
{
    private PanelAdventure _owner;

    public List<Chapter> Contents => _owner.Chapters;
    
    public void Initialize(PanelAdventure owner)
    {
        _owner = owner;
    }
    
    public override object GetListContent(int index)
    {
        return Contents[index];
    }

    public override int GetListLength()
    {
        return Contents.Count;
    }
}
