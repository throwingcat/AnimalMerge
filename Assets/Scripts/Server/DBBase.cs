using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Server
{
    public abstract class DBBase
    {
        public abstract string DB_KEY();
        public string InDate;
        public bool isReservedUpdate = false;
        protected System.Action _onFinishUpdate = null;
        public virtual void Update(System.Action onFinishUpdate)
        {
            isReservedUpdate = true;
            _onFinishUpdate += onFinishUpdate;
        }
        public virtual void DoUpdate(){}

        public virtual void Download(System.Action onFinishDownload)
        {
            
        }
    }
}