using System.Collections.Generic;
using Define;
using UnityEngine;

public class ActiveShake : ActiveBase
{
    public ActiveShake(GameCore core) : base(core)
    {
    }

    protected override bool RunProcess()
    {
        //AI 전용 조건
        if (Core.IsPlayer == false)
            if (UnitsInField.Count < 10)
                return false;

        GameManager.SimpleTimer(Key.SIMPLE_TIMER_RUNNING_SKILL, 3f);

        //모든 유닛 Collider 일시중단
        Core.PauseCollider(0.5f);

        var units = new List<UnitBase>();
        units.AddRange(BadUnits);
        units.AddRange(UnitsInField);

        //모든 블록 위로 튕겨냄
        foreach (var unit in units)
        {
            var direction = Vector2.up * EnvironmentValue.SHAKE_SKILL_VERTICAL_POWER;
            direction.x = Random.Range(-EnvironmentValue.SHAKE_SKILL_HORIZONTAL_POWER,
                EnvironmentValue.SHAKE_SKILL_HORIZONTAL_POWER);
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

        return true;
    }
}