using Define;
using UnityEngine;

//Game Core와 동일한 동작을 하지만 불필요한 동작을 제거함
//1)UI를 사용하지 않음
//2)VFX,SFX를 재생하지 않음

public class AICore : GameCore
{
    public int MMR = 1000;
    public float InputDelayElapsed = 0f;
    public float InputDelayDuration = 0f;
    public override void Initialize(bool isPlayer)
    {
        IsPlayer = isPlayer;
        Initialize();

        MMR = PlayerInfo.Instance.RankScore;
        float mmr_ratio = Mathf.InverseLerp(EnvironmentValue.AI_INPUT_DELAY_LOW_MMR, EnvironmentValue.AI_INPUT_DELAY_HIGH_MMR, MMR);

        InputDelayDuration = Mathf.Lerp(EnvironmentValue.AI_INPUT_DELAY_MAX, EnvironmentValue.AI_INPUT_DELAY_MIN, mmr_ratio);
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
            Vector3 pos = UnitSpawnPosition;
            foreach (var unit in UnitsInField)
                if (CurrentReadyUnit.Info.Key == unit.Info.Key)
                {
                    pos.x = unit.transform.localPosition.x;
                    isFindFriend = true;
                    break;
                }

            if (isFindFriend == false)
            {
                var horizontalLimit = 540f - EnvironmentValue.UNIT_SPRITE_BASE_SIZE * EnvironmentValue.WORLD_RATIO *
                    CurrentReadyUnit.Sheet.size;
                pos.x  = Random.Range(-horizontalLimit, horizontalLimit);
            }

            CurrentReadyUnit.transform.localPosition = pos;
            
            
            OnRelease();
        }
    }

    public override void Clear()
    {
        base.Clear();
        gameObject.SetActive(false);
    }
}