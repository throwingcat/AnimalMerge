#if UNITY_EDITOR
using UnityEditor;

namespace Violet
{
    [InitializeOnLoad]
    public class CSVDownloadConfigCore
    {
        private static CSVDownloadConfig _config;

        static CSVDownloadConfigCore()
        {
            _config = CSVDownloadConfig.Instance;
        }
    }
}
#endif