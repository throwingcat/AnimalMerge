using System;

namespace Server
{
    public class DBAchievement : DBBase
    {
        public override string DB_KEY()
        {
            return "achievement";
        }

        public override void Save()
        {
            _Save(AchievementInfo.Instance.Achievements);
        }

        public override void Load(Action onFinish)
        {
            _Load((json) =>
            {
                AchievementInfo.Instance.OnUpdate(json);
            });
        }
    }
}