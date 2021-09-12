using System.Collections.Generic;
using DG.Tweening;
using SheetData;
using UnityEngine;
using UnityEngine.UI;
using Violet;

public class PanelAdventure : SUIPanel
{
    private List<CellStage> _stages = new List<CellStage>();
    public List<CellChapter> CellChapters = new List<CellChapter>();
    private List<Chapter> _chapters = new List<Chapter>();

    public RectTransform ChapterRoot;
    public RectTransform StageRoot;

    public CellStage CellStagePrefab;
    public Text ChapterName;
    public GameObject Information;
    public DOTweenPlayer InformationTweenPlayer;
    public GameObject PrevChapterButton;
    public GameObject NextChapterButton;

    public int CurrentChapterIndex;
    public int PrevChapterIndex = -1;
    public Chapter CurrentChapter;
    public Stage CurrentStage;
    public Text StageLevel;
    public GameObject StageScrollRoot;
    public ScrollRect StageScrollView;

    private ulong _moveChapterScrollInvokeID = 0L;
    private List<Tweener> _moveChapterTwwenTweeners = new List<Tweener>();

    protected override void OnShow()
    {
        base.OnShow();

        CurrentChapterIndex = -1;
        PrevChapterIndex = 0;
        Information.SetActive(false);

        var pos = ChapterRoot.anchoredPosition;
        pos.y = -66;
        ChapterRoot.anchoredPosition = pos;

        StageRoot.gameObject.SetActive(false);
        pos = StageRoot.anchoredPosition;
        pos.y = -1750;
        StageRoot.anchoredPosition = pos;

        Initialize();
    }

    public void Initialize()
    {
        var table = TableManager.Instance.GetTable<Chapter>();
        _chapters = new List<Chapter>();
        foreach (var row in table)
            _chapters.Add(row.Value as Chapter);

        foreach (var cell in CellChapters)
            cell.Initialize();

        // var need = sheet.Count - _chapters.Count;
        //
        // for (var i = 0; i < need; i++)
        // {
        //     var cell = Instantiate(CellChapterPrefab);
        //     cell.transform.SetParent(ChapterScrollRoot.transform);
        //     cell.transform.LocalReset();
        //     _chapters.Add(cell);
        // }
        //
        // foreach (var cell in _chapters)
        //     cell.gameObject.SetActive(false);
        //
        // var index = 0;
        // foreach (var row in sheet)
        // {
        //     var chapter = row.Value as Chapter;
        //     _chapters[index].Set(chapter);
        //     _chapters[index].gameObject.SetActive(true);
        //     index++;
        // }


        MoveChapter(0);
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

        PrevChapterIndex = CurrentChapterIndex;
        CurrentChapterIndex = index;
        MoveChapterScroll(PrevChapterIndex - CurrentChapterIndex);
    }

    private void SetChapter(int chapter)
    {
        foreach (var tween in _moveChapterTwwenTweeners)
            tween.Kill();
        _moveChapterTwwenTweeners.Clear();

        int index = -2;
        foreach (var cell in CellChapters)
        {
            cell.RectTransform.anchoredPosition = cell.OrigianlPosition;

            int value = index + chapter;
            if (0 <= value && value < _chapters.Count)
            {
                cell.Set(_chapters[value], OnClickChapter);
                cell.gameObject.SetActive(true);
            }
            else
                cell.gameObject.SetActive(false);

            index++;
        }

        SetInputEnable(true);
        SetEnableChapterMoveButton(true);

        if (chapter == CurrentChapterIndex)
        {
            Information.SetActive(true);
            InformationTweenPlayer.SetEnable(true);

            CurrentChapter = _chapters[CurrentChapterIndex];
            //챕터 설정
            ChapterName.text = _chapters[CurrentChapterIndex].name.ToLocalization();
        }
    }

    public void MoveChapterScroll(int direction)
    {
        GameManager.DelayInvokeCancel(_moveChapterScrollInvokeID);
        SetChapter(PrevChapterIndex);

        SetInputEnable(false);
        SetEnableChapterMoveButton(false);
        
        direction = Mathf.Clamp(direction, -1, 1);
        for (int i = 0; i < CellChapters.Count; i++)
        {
            int index = i + direction;
            if (0 <= index && index < CellChapters.Count)
            {
                Vector2 to = CellChapters[index].OrigianlPosition;
                _moveChapterTwwenTweeners.Add(CellChapters[i].RectTransform.DOAnchorPosX(to.x, 0.33f)
                    .SetEase(Ease.OutBack));
                _moveChapterTwwenTweeners.Add(CellChapters[i].RectTransform.DOAnchorPosY(to.y, 0.33f)
                    .SetEase(Ease.OutBack));
            }
        }

        _moveChapterScrollInvokeID = GameManager.DelayInvoke(() => { SetChapter(CurrentChapterIndex); }, 0.5f);

        // var destination = CurrentIndex == 0 ? 0f : (float) CurrentIndex / (_chapters.Count - 1);
        // DOTween.To(() => ChapterScrollView.horizontalNormalizedPosition,
        //         x => ChapterScrollView.horizontalNormalizedPosition = x,
        //         destination, 0.3f).SetEase(Ease.OutBack)
        //     .Play();
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
        if (CurrentChapterIndex < _chapters.Count - 1)
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
            NextChapterButton.SetActive(CurrentChapterIndex < (_chapters.Count - 1));
        }
        else
        {
            PrevChapterButton.SetActive(false);
            NextChapterButton.SetActive(false);
        }
    }

    #endregion
}