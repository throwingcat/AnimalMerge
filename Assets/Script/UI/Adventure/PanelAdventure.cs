using System.Collections.Generic;
using AirFishLab.ScrollingList;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelAdventure : SUIPanel
{
    public List<CellChapter> CellChapters = new List<CellChapter>();
    private List<CellStage> _stages = new List<CellStage>();
    public List<Chapter> Chapters = new List<Chapter>();
    public CircularScrollingList ChapterScrollView;
    public RectTransform ChapterRoot;
    public RectTransform StageRoot;

    public CellStage CellStagePrefab;
    public Text ChapterName;
    public GameObject Information;
    public DOTweenPlayer InformationTweenPlayer;
    public GameObject PrevChapterButton;
    public GameObject NextChapterButton;

    public int CurrentChapterIndex;
    public Chapter CurrentChapter;
    public Stage CurrentStage;
    public Text StageLevel;
    public GameObject StageScrollRoot;
    public ScrollRect StageScrollView;

    private bool _isInitialize = false;
    protected override void OnShow()
    {
        base.OnShow();
        _isInitialize = false;
        CurrentChapterIndex = -1;
        var bank = ChapterScrollView.listBank as ChapterScrollViewBank;
        bank.Initialize(this);
        ChapterScrollView.OnBeginDragEvent = OnBeginDrag;
        
        var table = TableManager.Instance.GetTable<Chapter>();
        Chapters = new List<Chapter>();
        foreach (var row in table)
            Chapters.Add(row.Value as Chapter);
        foreach (var chapter in CellChapters)
            chapter.SetOnClickEvent(OnClickChapter);
        
        var pos = ChapterRoot.anchoredPosition;
        pos.y = -66;
        ChapterRoot.anchoredPosition = pos;
        Information.SetActive(false);
        
        StageRoot.gameObject.SetActive(false);
        pos = StageRoot.anchoredPosition;
        pos.y = -1750;
        StageRoot.anchoredPosition = pos;
    }

    void Update()
    {
        if (_isInitialize == false)
        {
            MoveChapter(0);
            _isInitialize = true;
        }
    }
    private void SetStage(Chapter chapter)
    {
        //스테이지 설정
        var stages = Stage.GetStage(chapter);

        //클리어한 스테이지 목록 정리
        List<Stage> sortedStage = new List<Stage>();
        int higher_stage = -1;
        for (int i = 0; i < stages.Count; i++)
        {
            bool isClear = PlayerTracker.Instance.Contains(stages[i].key);

            if (isClear)
            {
                sortedStage.Add(stages[i]);
                if (higher_stage < stages[i].Index)
                    higher_stage = stages[i].Index;
            }
        }

        //최신 스테이지 추가
        int new_stage = higher_stage + 1;
        foreach (var stage in stages)
        {
            if (stage.Index == new_stage)
                sortedStage.Add(stage);
        }

        stages = sortedStage;


        var need = stages.Count - _stages.Count;
        for (var i = 0; i < need; i++)
        {
            var cell = Instantiate(CellStagePrefab);
            cell.transform.SetParent(StageScrollRoot.transform);
            cell.transform.LocalReset();
            cell.transform.SetAsLastSibling();
            _stages.Add(cell);
        }

        foreach (var cell in _stages)
            cell.gameObject.SetActive(false);

        int lastIndex = 0;
        for (var i = 0; i < stages.Count; i++)
        {
            int index = (stages.Count - 1) - i;
            _stages[index].Set(stages[i], OnClickStage);
            _stages[index].gameObject.SetActive(true);
        }

        ChangeStage(lastIndex);
        StageScrollView.content.anchoredPosition = Vector2.zero;
    }

    private void MoveChapter(int index)
    {
        if (CurrentChapterIndex == index) return;
        ChapterScrollView.SelectContentID(index);
        CurrentChapterIndex = index;
        OnBeginDrag();
    }

    private void SetChapter(int chapter)
    {
        CurrentChapterIndex = chapter;
        
        SetInputEnable(true);
        SetEnableChapterMoveButton(true);

        Information.SetActive(true);
        InformationTweenPlayer.SetEnable(true);

        CurrentChapter = Chapters[CurrentChapterIndex];
        ChapterName.text = Chapters[CurrentChapterIndex].name.ToLocalization();
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
        //StageLevel.text = CurrentStage.AI_MMR.ToString();
    }

    #region Input

    public override void BackPress()
    {
        if (IgnoreBackPress) return;

        if (StageRoot.gameObject.activeSelf)
        {
            SetEnableStageView(false);
            return;
        }

        base.BackPress();
    }

    public void OnClickPrevChapter()
    {
        if (CurrentChapterIndex == 0) return;

        MoveChapter(CurrentChapterIndex - 1);
    }

    public void OnClickNextChapter()
    {
        if (CurrentChapterIndex < Chapters.Count - 1)
            MoveChapter(CurrentChapterIndex + 1);
    }

    public void OnClickStartButton()
    {
        GameManager.EnterAdventure(CurrentStage.key);
    }

    public void OnClickStage(CellStage cell)
    {
        ChangeStage(cell.Stage);
    }

    public void OnClickChapter(CellChapter chapter)
    {
        if (IgnoreBackPress) return;

        if (chapter.Chatper.index == CurrentChapter.index)
            SelectedChapter(chapter.Chatper);
        else
            MoveChapter(chapter.Chatper.index);
    }

    public void SelectedChapter(Chapter chapter)
    {
        SetEnableStageView(true);
        SetStage(chapter);
    }

    public void SetEnableStageView(bool isEnable)
    {
        SetInputEnable(false);
        SetEnableChapterMoveButton(false);
        if (isEnable)
        {
            StageRoot.gameObject.SetActive(true);

            ChapterRoot.DOAnchorPosY(450f, 0.3f).SetEase(Ease.OutQuad);
            StageRoot.DOAnchorPosY(-426f, 0.3f).SetEase(Ease.OutQuad);
        }
        else
        {
            ChapterRoot.DOAnchorPosY(-66f, 0.3f).SetEase(Ease.OutQuad);
            StageRoot.DOAnchorPosY(-1750, 0.3f).SetEase(Ease.OutQuad);
        }

        GameManager.DelayInvoke(() =>
        {
            SetInputEnable(true);
            SetEnableChapterMoveButton(!isEnable);
            StageRoot.gameObject.SetActive(isEnable);
        }, 0.6f);
    }

    public void SetInputEnable(bool isEnable)
    {
        IgnoreBackPress = !isEnable;
    }

    public void SetEnableChapterMoveButton(bool isEnable)
    {
        if (isEnable)
        {
            PrevChapterButton.SetActive(0 < CurrentChapterIndex);
            NextChapterButton.SetActive(CurrentChapterIndex < (Chapters.Count - 1));
        }
        else
        {
            PrevChapterButton.SetActive(false);
            NextChapterButton.SetActive(false);
        }
    }

    public void OnBeginDrag()
    {
        SetInputEnable(false);
        SetEnableChapterMoveButton(false);
    }
    public void OnScrollingFinish()
    {
        var center = ChapterScrollView.GetCenteredBox() as CellChapter;
        SetChapter(center.Chatper.index);
        SetInputEnable(true);
        SetEnableChapterMoveButton(true);
    }
    #endregion
}