using System;

namespace Server
{
    public class DBPlayerTracker : DBBase
    {
        public override string DB_KEY()
        {
            return "player_tracker";
        }

        public override void Save()
        {
            base.Save();
            _Save(PlayerTracker.Instance.Tracker);
        }

        public override void Load(Action onFinishDownload)
        {
            _Load(json =>
            {
                PlayerTracker.Instance.OnUpdate(json);
                onFinishDownload?.Invoke();
            });
        }
    }
}