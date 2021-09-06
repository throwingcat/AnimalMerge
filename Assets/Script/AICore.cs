using Define;
using SheetData;
using UnityEngine;

//Game Core와 동일한 동작을 하지만 불필요한 동작을 제거함
//1)UI를 사용하지 않음
//2)VFX,SFX를 재생하지 않음

public class AICore : GameCore
{
    public float InputDelayDuration;
    public float InputDelayElapsed;
    public int MMR = 1000;
    protected override int UnitLayer => LayerMask.NameToLayer("AI_Unit");

    public override void Initialize(bool isPlayer)
    {
        IsPlayer = isPlayer;
        Initialize();

        //모험모드
        if (GameManager.Instance.isAdventure)
        {
            var stage = GameManager.Instance.StageKey.ToTableData<Stage>();
            MMR = stage.AI_MMR;
        }

        if (GameManager.Instance.isSinglePlay && GameManager.Instance.isAdventure == false)
            MMR = PlayerInfo.Instance.RankScore;

        var mmr_ratio = Mathf.InverseLerp(EnvironmentValue.AI_INPUT_DELAY_LOW_MMR,
            EnvironmentValue.AI_INPUT_DELAY_HIGH_MMR, MMR);

        InputDelayDuration = Mathf.Lerp(EnvironmentValue.AI_INPUT_DELAY_MAX, EnvironmentValue.AI_INPUT_DELAY_MIN,
            mmr_ratio);
        InputDelayElapsed = InputDelayDuration;

        gameObject.SetActive(true);
    }

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);
    }

    protected override void InputUpdate()
    {
        if (CurrentReadyUnit != null)
        {
            if (InputDelayElapsed < InputDelayDuration)
            {
                InputDelayElapsed += Time.deltaTime;
                return;
            }

            InputDelayElapsed = 0f;

            var isFindFriend = false;
            var pos = UnitSpawnPosition;
            foreach (var unit in UnitsInField)
                if (CurrentReadyUnit.Info.Key == unit.Info.Key)
                {
                    pos.x = unit.transform.localPosition.x;
                    isFindFriend = true;
                    break;
                }

            if (isFindFriend == false)
            {
                var horizontalLimit = 540f - EnvironmentValue.UNIT_SPRITE_BASE_SIZE * CurrentReadyUnit.Sheet.size *
                    EnvironmentValue.WORLD_RATIO;
                pos.x = Random.Range(-horizontalLimit, horizontalLimit);
            }

            CurrentReadyUnit.transform.localPosition = pos;

            OnRelease();
        }

        if (Active.isEnable)
            Active.Run();
    }

    public override void Clear()
    {
        base.Clear();
        gameObject.SetActive(false);
    }
}