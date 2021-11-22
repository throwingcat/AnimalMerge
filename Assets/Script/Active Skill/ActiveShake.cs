using System.Collections;
using System.Collections.Generic;
using Define;
using UnityEngine;
using Violet;

public class ActiveShake : ActiveBase
{
    public ActiveShake(GameCore core) : base(core)
    {
    }

    private VFXPlayer _vfx;

    protected override bool Active()
    {
        //AI 전용 조건
        if (Core.IsPlayer == false)
            if (UnitsInField.Count < 10)
                return false;

        if (_process != null)
            Core.StopCoroutine(_process);
        _process = Core.StartCoroutine(Process());

        return true;
    }

    private Coroutine _process = null;
    protected float HorizontalSpawnLimit => (500f * EnvironmentValue.WORLD_RATIO);

    protected override IEnumerator Process()
    {
        //스킬 타이머 실행
        GameManager.SimpleTimer(Key.SIMPLE_TIMER_RUNNING_SKILL, 3f);

        //VFX 실행
        if (Core.IsPlayer)
        {
            if (_vfx == null)
            {
                var prefab = ResourceManager.Instance.LoadPrefab("FX_Prefabs/GameFX_Prefab/VFX@ActiveShake");
                _vfx = GameObject.Instantiate(prefab).GetComponent<VFXPlayer>();
                _vfx.transform.SetParent(GameManager.Instance.GameCore.PlayerScreen.transform);
                _vfx.transform.LocalReset();
                _vfx.gameObject.SetActive(false);
            }

            _vfx.Play(5f, null);
        }

        float duration = 1.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            var units = new List<UnitBase>();
            units.AddRange(BadUnits);
            units.AddRange(UnitsInField);

            elapsed += Time.deltaTime;

            //모든 블록 위로 튕겨냄
            foreach (var unit in units)
            {
                var x = unit.transform.localPosition.x;
                var t = Mathf.InverseLerp(-HorizontalSpawnLimit, HorizontalSpawnLimit, x);
                var v = Mathf.Lerp(EnvironmentValue.SHAKE_SKILL_HORIZONTAL_POWER,
                    -EnvironmentValue.SHAKE_SKILL_HORIZONTAL_POWER, t);

                var direction = Vector2.up * EnvironmentValue.SHAKE_SKILL_VERTICAL_POWER;
                direction.x = v;
                unit.Rigidbody2D.velocity = Vector2.zero;
                unit.AddForce(direction);

                var range = EnvironmentValue.SHAKE_SKILL_TORQUE_MAX_POWER -
                            EnvironmentValue.SHAKE_SKILL_TORQUE_MIN_POWER;
                var torque = Random.Range(-range, range);
                if (torque < 0)
                    torque -= EnvironmentValue.SHAKE_SKILL_TORQUE_MIN_POWER;
                else
                    torque += EnvironmentValue.SHAKE_SKILL_TORQUE_MIN_POWER;

                unit.AddTorque(torque);
            }

            yield return new WaitForFixedUpdate();
        }
        _process = null;
    }
}