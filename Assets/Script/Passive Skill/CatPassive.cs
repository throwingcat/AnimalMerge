using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatPassive : PassiveBase
{
    public CatPassive(GameCore core) : base(core)
    {
    }

    public override void Run(Action onComplete)
    {
        if (isEnable == false) return;
        CoolTimeRemain = CoolTime;
        //쥐 탐색
        if (0 < BadUnits.Count)
        {
            var pick = Random.Range(0, BadUnits.Count);

            var pos = BadUnits[pick].transform.position;

            //VFX
            if (Core.IsPlayer)
            {
                PanelIngame.PlayerSkillVFX.SetActive(false);
                var rt = PanelIngame.PlayerSkillVFX.GetComponent<RectTransform>();
                Utils.WorldToCanvas(ref rt, Camera.main, pos, Canvas.GetComponent<RectTransform>());
                PanelIngame.PlayerSkillVFX.SetActive(true);
                GameManager.DelayInvoke(() => { PanelIngame.PlayerSkillVFX.SetActive(false); }, 1f);
            }

            var center = BadUnits[pick].transform.localPosition;
            var remove_range = 200f;
            var shake_range = 400f;
            var remove_target = new List<string>();
            var shake_target = new List<Tuple<string, Vector2>>();

            foreach (var unit in BadUnits)
            {
                var distance = Vector3.Distance(center, unit.transform.localPosition);
                if (distance <= remove_range)
                    remove_target.Add(unit.GUID);
                else if (distance <= shake_range)
                    shake_target.Add(new Tuple<string, Vector2>(unit.GUID,
                        (unit.transform.localPosition - center).normalized));
            }

            foreach (var guid in remove_target)
                for (var i = 0; i < BadUnits.Count; i++)
                    if (BadUnits[i].GUID == guid)
                    {
                        Core.RemoveUnit(BadUnits[i]);
                        break;
                    }

            foreach (var shake in shake_target)
                for (var i = 0; i < BadUnits.Count; i++)
                    if (BadUnits[i].GUID == shake.Item1)
                    {
                        BadUnits[i].Rigidbody2D.velocity = Vector2.zero;
                        BadUnits[i].Rigidbody2D.AddForce(shake.Item2 * 100f);
                        break;
                    }

            onComplete?.Invoke();
        }
    }
}