namespace Violet
{
    public class CSVDataBase : ICSVDataBase
    {
        public string key = "";

        public string GetKey()
        {
            return key;
        }

        public virtual void Initialize()
        {
        }
    }
}