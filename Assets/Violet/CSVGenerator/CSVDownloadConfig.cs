using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Version = System.Version;

namespace Violet
{
    public class CSVDownloadConfig : ScriptableObject
    {
        private const int MenuPropertyOrder = 0;
        private const int DataPropertyOrder = 100;

        public const string VersionURL = "https://shdd.synology.me/Simon/Animal Merge/Sheet/version.json";

        public float DownloadProgress;

        public Object ClassCreateFolder;

        public static string DOWNLOAD_PATH => Path.Combine(Application.persistentDataPath, "CSVData");

        public static string LOCAL_DOWNLOAD_PATH => string.Format("{0}/{1}", Application.streamingAssetsPath, "CSVData");

        public static Dictionary<string,string> CSV = new Dictionary<string, string>();
        
        [SerializeField]
        public CSVDownloadConfigData Config
        {
            get
            {
                var config = new CSVDownloadConfigData();
                var tx = Resources.Load("CSVConfig") as TextAsset;
                if (tx != null)
                    config = JsonConvert.DeserializeObject<CSVDownloadConfigData>(tx.text);
                return config;
            }
        }
#if UNITY_EDITOR
        public void Download(Action<float> onUpdate, Action onComplete)
        {
            DownloadProgress = 0;
            EditorCoroutines.StartCoroutine(DownloadProcess(onUpdate, onComplete, true), this);
        }
#endif

        public void Download(MonoBehaviour mono, Action<float> onUpdate, Action onComplete)
        {
            mono.StartCoroutine(DownloadProcess(onUpdate, onComplete));
        }

        private IEnumerator DownloadProcess(Action<float> onUpdate, Action onComplete, bool isForceDownload = false)
        {
            //Path 마지막 부분이 '/'(슬래시)라면 삭제
            var downloadPath = DOWNLOAD_PATH;
            if (downloadPath[downloadPath.Length - 1] == '/')
                downloadPath.Remove(downloadPath.Length - 1, 1);

            //Directory 생성
            if (Directory.Exists(downloadPath) == false)
                Directory.CreateDirectory(downloadPath);

            var isNeedDownloadNewSheet = false;

            //Version Check
            UnityWebRequest req = null;
            var versionFilePath = "";
            var versionFileText = "";
            if (string.IsNullOrEmpty(VersionURL) == false)
            {
                string prevVersion = "";
                //기본 버젼파일 확인
                versionFilePath = Path.Combine(downloadPath, "version.json");
                if (File.Exists(versionFilePath) == false)
                {
                    isNeedDownloadNewSheet = true;
                }
                else
                {
                    //기존에 있는 Version 파일 로딩
                    var sr = new StreamReader(versionFilePath);
                    prevVersion = sr.ReadToEnd();
                    sr.Dispose();
                    sr.Close();
                }

                //다운로드 진행
                req = UnityWebRequest.Get(VersionURL);
                yield return req.SendWebRequest();

                //새로운 버젼파일 생성
                File.WriteAllText(versionFilePath, req.downloadHandler.text);

                //새로운 버젼파일 텍스트 저장
                versionFileText = req.downloadHandler.text;
                try
                {
                    var localVersion = JsonUtility.FromJson<VersionData>(prevVersion);
                    var newVersion = JsonUtility.FromJson<VersionData>(versionFileText);

                    Debug.Log(string.Format("현재 Sheet Version : {0} , 최신 Sheet Version : {1}",
                        localVersion.Version,
                        newVersion.Version));

                    var isSameVersion = localVersion.GetVersion().Equals(newVersion.GetVersion());
                    if (isSameVersion == false)
                    {
                        //현재 가지고있는 VersionFile 삭제, 새로운 CSV 다운로드
                        File.Delete(versionFilePath);
                        isNeedDownloadNewSheet = true;
                    }
                }
                catch
                {
                    // Version Check 도중 오류발생, 무조건 새로운 CSV 다운로드
                    File.Delete(versionFilePath);
                    isNeedDownloadNewSheet = true;
                }
            }

            //if (isForceDownload)
                isNeedDownloadNewSheet = true;

            //CSV Download
            if (isNeedDownloadNewSheet)
            {
                CSV.Clear();
                var step = 1f / Config.Files.Count;
                foreach (var csv in Config.Files)
                {
                    Debug.Log(string.Format("CSV Download : {0}", csv.Name));

                    var url = Config.BuildURL(csv);
                    req = UnityWebRequest.Get(url);
                    req.certificateHandler = new GameManager.BypassCertificate();
                    yield return req.SendWebRequest();

                    DownloadProgress += step;
                    onUpdate?.Invoke(DownloadProgress);

                    string text = "";
                    if (req.isNetworkError)
                    {
                        Debug.LogErrorFormat("CSV Download Fail (UWR) ): {0} >>> {1}" , csv.Name , req.error);
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.Encoding = Encoding.UTF8;
                            text = webClient.DownloadString(url);

                            if (text.IsNullOrEmpty() == false)
                            {
                                string file = string.Format("{0}.csv", csv.Name);
                                var path = Path.Combine(downloadPath, file);
                                CSV.Add(path, req.downloadHandler.text);
                            }
                            else
                            {
                                Debug.LogErrorFormat("CSV Download Fail (WC) : {0} >>> {1}" , csv.Name , req.error);                                
                            }
                        }                        
                    }
                    else
                    {
                        string file = string.Format("{0}.csv", csv.Name);
                        var path = Path.Combine(downloadPath, file);
                        File.WriteAllText(path, req.downloadHandler.text);
                        CSV.Add(path,req.downloadHandler.text);
                    }
                }

                //CSV 다운로드 완료 후 새로운 VersionFile 작성
                if (string.IsNullOrEmpty(versionFilePath) == false &&
                    string.IsNullOrEmpty(versionFileText) == false)
                    File.WriteAllText(versionFilePath, versionFileText);
            }

            DownloadProgress = 1f;
            onUpdate?.Invoke(DownloadProgress);
            onComplete?.Invoke();
        }

        [Serializable]
        public class VersionData
        {
            public string Version;

            public Version GetVersion()
            {
                return System.Version.Parse(Version);
            }
        }

        public class CSVDownloadConfigData
        {
            public string BaseURL = "";
            public List<CSVDownloadFile> Files = new List<CSVDownloadFile>();

            public string BuildURL(CSVDownloadFile file)
            {
                return string.Format("{0}/{1}.csv", BaseURL, file.Name);
            }
        }

        [Serializable]
        public class CSVDownloadFile
        {
            [SerializeField] public string Name;

            [NonSerialized] [HideInInspector] public string cssPath = "";

            public void Generate()
            {
                var csvPath = string.Format("{0}/{1}.csv", DOWNLOAD_PATH, Name);
                if (File.Exists(csvPath)) CSVGenerator.Generate(csvPath, cssPath, Name);
            }
        }

        #region Singleton Behaviour

#if UNITY_EDITOR
        private static CSVDownloadConfig instance;
        public static CSVDownloadConfig Instance
        {
            get
            {
                if (instance != null)
                    return instance;

                var possibleTempData = AssetDatabase.LoadAssetAtPath<CSVDownloadConfig>(GetSettingsFilePath());
                if (possibleTempData != null)
                {
                    instance = possibleTempData;
                    return instance;
                }

                // no instance exists, create a new instance.
                instance = CreateInstance<CSVDownloadConfig>();
                AssetDatabase.CreateAsset(instance, GetSettingsFilePath());
                AssetDatabase.SaveAssets();
                return instance;
            }
        }
#endif

        private static string LocateVioletFolder()
        {
            var results = Directory.GetFiles(Application.dataPath, "CSVDownloadConfigCore.cs",
                SearchOption.AllDirectories);
            if (results.Length > 0)
            {
                var parent = Directory.GetParent(results[0]);
                while (parent.Name != "Violet")
                    parent = parent.Parent;

                return parent.FullName;
            }

            return null;
        }

        public static string GetSettingsFilePath()
        {
            var path = LocateVioletFolder();
            path += "\\CSVGenerator\\CSVDownloaderConfig.asset";
            path = path.Substring(path.IndexOf("Assets"));
            return path;
        }

        #endregion

#if UNITY_EDITOR
        public void Generate(Action<float> onUpdate, Action onComplete)
        {
            GenerateProgress = 0f;
            EditorCoroutines.StartCoroutine(GenerateProcess(onUpdate, onComplete), this);
        }

        public float GenerateProgress;

        private IEnumerator GenerateProcess(Action<float> onUpdate, Action onComplete)
        {
            var step = 1f / Config.Files.Count;

            foreach (var csv in Config.Files)
            {
                csv.cssPath = AssetDatabase.GetAssetPath(ClassCreateFolder);

                csv.Generate();

                GenerateProgress += step;
                onUpdate?.Invoke(GenerateProgress);

                yield return new WaitForSeconds(0.1f);
            }

            GenerateProgress = 1f;
            onUpdate?.Invoke(GenerateProgress);
            onComplete?.Invoke();
        }

        public void ToJson()
        {
            var json = JsonConvert.SerializeObject(Config.Files);
            Debug.Log(json);
        }
#endif
    }
}