using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelAdventure : SUIPanel
{
    private readonly List<CellStage> _stages = new List<CellStage>();

    private readonly List<CellChapter> _chapters = new List<CellChapter>();

    public CellChapter CellChapterPrefab;
    public CellStage CellStagePrefab;
    public Text ChapterDesc;
    public Text ChapterName;
    public GameObject ChapterScrollRoot;

    public ScrollRect ChapterScrollView;

    public int CurrentIndex;

    public Stage CurrentStage;
    public int PrevIndex = -1;
    public Text StageLevel;
    public GameObject StageScrollRoot;

    public ScrollRect StageScrollView;

    protected override void OnShow()
    {
        base.OnShow();

        CurrentIndex = -1;
        PrevIndex = -1;

        Initialize();
    }

    public void Initialize()
    {
        var sheet = TableManager.Instance.GetTable<Chapter>();

        var need = sheet.Count - _chapters.Count;

        for (var i = 0; i < need; i++)
        {
            var cell = Instantiate(CellChapterPrefab);
            cell.transform.SetParent(ChapterScrollRoot.transform);
            cell.transform.LocalReset();
            _chapters.Add(cell);
        }

        foreach (var cell in _chapters)
            cell.gameObject.SetActive(false);

        var index = 0;
        foreach (var row in sheet)
        {
            var chapter = row.Value as Chapter;
            _chapters[index].Set(chapter);
            _chapters[index].gameObject.SetActive(true);
            index++;
        }


        MoveChapter(0);
    }

    public void ChangeChapter(int index)
    {
        //챕터 설정
        ChapterName.text = _chapters[index].Chatper.name.ToLocalization();
        ChapterDesc.text = _chapters[index].Chatper.name.ToLocalization();

        //스테이지 설정
        var stages = Stage.GetStage(_chapters[index].Chatper);

        var need = stages.Count - _stages.Count;
        for (var i = 0; i < need; i++)
        {
            var cell = Instantiate(CellStagePrefab);
            cell.transform.SetParent(StageScrollRoot.transform);
            cell.transform.LocalReset();
            _stages.Add(cell);
        }

        foreach (var cell in _stages)
            cell.gameObject.SetActive(false);

        int lastIndex = 0;
        for (var i = 0; i < stages.Count; i++)
        {
            _stages[i].Set(stages[i], OnClickStage);
            bool isClear = PlayerTracker.Instance.Contains(stages[i].key);
            bool isLock = stages[i].UnlockCondition.IsNullOrEmpty() ? false : PlayerTracker.Instance.Contains(stages[i].UnlockCondition) == false;
            if (isLock)
                _stages[i].SetState(CellStage.eState.Lock);
            else
            {
                if (isClear)
                    _stages[i].SetState(CellStage.eState.Clear);
                else
                    _stages[i].SetState(CellStage.eState.Unlock);
                lastIndex = i;
            }

            _stages[i].gameObject.SetActive(true);
        }

        ChangeStage(lastIndex);
        float t = lastIndex / (float) stages.Count;
        StageScrollView.content.anchoredPosition = new Vector2(0, StageScrollView.content.rect.height * t);
    }

    private void MoveChapter(int index)
    {
        if (CurrentIndex == index) return;

        PrevIndex = CurrentIndex;
        CurrentIndex = index;
        ChangeChapter(CurrentIndex);

        RefreshChapterScroll();
    }

    public void RefreshChapterScroll()
    {
        var destination = CurrentIndex == 0 ? 0f : (float) CurrentIndex / (_chapters.Count - 1);
        DOTween.To(() => ChapterScrollView.horizontalNormalizedPosition,
                x => ChapterScrollView.horizontalNormalizedPosition = x,
                destination, 0.3f).SetEase(Ease.OutBack)
            .Play();
    }

    public void ChangeStage(int index)
    {
        ChangeStage(_stages[index].Stage);
    }

    public void ChangeStage(Stage stage)
    {
        CurrentStage = stage;

        for (int i = 0; i < _stages.Count; i++)
            _stages[i].SetSelect(CurrentStage.key == _stages[i].Stage.key);

        StageLevel.text = CurrentStage.AI_MMR.ToString();
    }

    #region Input

    public void OnClickPrevChapter()
    {
        if (CurrentIndex == 0) return;

        MoveChapter(CurrentIndex - 1);
    }

    public void OnClickNextChapter()
    {
        if (CurrentIndex < _chapters.Count - 1)
            MoveChapter(CurrentIndex + 1);
    }

    public void OnClickStartButton()
    {
        GameManager.EnterAdventure(CurrentStage.key);
    }

    public void OnClickStage(CellStage cell)
    {
        ChangeStage(cell.Stage);
    }

    #endregion
}