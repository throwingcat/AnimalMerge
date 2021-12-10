using System;

public class PlayerDataBase
{
    public virtual void OnUpdate(string json)
    {
    }
    
    public virtual void Download(Action onFinish){}
}