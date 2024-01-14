using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Server
{
    public abstract class DBBase
    {
        public string InDate;
        protected Action _onFinishUpdate;
        public bool isReservedUpdate;

        public abstract string DB_KEY();
        
        //세이브 예약
        public void SaveReserve(Action onFinishUpdate)
        {
            isReservedUpdate = true;
            _onFinishUpdate += onFinishUpdate;
        }

        public virtual void Save() { } 
        
        protected void _Save(object data)
        {
            //Json으로 변환
            var json = JsonConvert.SerializeObject(data);
            
            //Json을 byte[] 로 변환
            var serialize = ES3.Serialize(json);
            
            //저장
            ES3.Save(DB_KEY(), serialize);

            //저장 완료이벤트
            _onFinishUpdate?.Invoke();
            
            //저장 플래그 변경
            isReservedUpdate = false;
        }
        
        public virtual void Load(Action onFinish){}
        
        protected string _Load(Action<string> onFinish)
        {
            //byte[] 로드
            var deserialize =  ES3.Load<byte[]>(DB_KEY());
            
            //byte[]를 Json으로 변환
            var json = ES3.Deserialize<string>(deserialize);

            return json;
        }
    }
}