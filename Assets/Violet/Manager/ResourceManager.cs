using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.U2D;

namespace Violet
{
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        public enum eAtlasType
        {
            Common,
        }

        private readonly Dictionary<string, AudioClip> _audioClip = new Dictionary<string, AudioClip>();

        private readonly Dictionary<string, SpriteAtlas> _cachedAtlas = new Dictionary<string, SpriteAtlas>();
        private readonly Dictionary<string, GameObject> _prefab = new Dictionary<string, GameObject>();

        public void Awake()
        {
            _cachedAtlas["Common"] = Resources.Load<SpriteAtlas>("Atlas/Common");
        }

        public GameObject LoadPrefab(string path)
        {
            GameObject prefab = null;
            if (_prefab.ContainsKey(path) == false)
                prefab = Resources.Load<GameObject>(path);
            else
                prefab = _prefab[path];

            if (prefab == null)
                Debug.LogError(string.Format("Load Fail  {0}", path));
            else
                _prefab[path] = prefab;

            return prefab;
        }

        public AudioClip LoadAudioClip(string path)
        {
            AudioClip audioClip = null;
            if (_audioClip.ContainsKey(path) == false)
                audioClip = Resources.Load<AudioClip>(path);
            else
                audioClip = _audioClip[path];

            if (audioClip == null)
                Debug.LogError(string.Format("Load Fail  {0}", path));
            else
                _audioClip[path] = audioClip;

            return audioClip;
        }

        public void Clear()
        {
            _prefab.Clear();
            _audioClip.Clear();
        }

        private Dictionary<string, Sprite> _cachedSprite = new Dictionary<string, Sprite>();
        private StringBuilder _stringBuilder= new StringBuilder();
        public Sprite GetSprite(string atlasKey, string name)
        {
            _stringBuilder.Clear();
            _stringBuilder.Append(atlasKey).Append("_").Append(name);
            
            string key = _stringBuilder.ToString();
            
            if (_cachedSprite.ContainsKey(key) == false)
            {
                if (_cachedAtlas.ContainsKey(atlasKey) == false)
                {
                    var atlas = Resources.Load<SpriteAtlas>(string.Format("Atlas/{0}", atlasKey));
                    if(atlas != null)
                        _cachedAtlas.Add(atlasKey,atlas);
                    else
                        return null;
                }
                Sprite sprite = _cachedAtlas[atlasKey].GetSprite(name);
                _cachedSprite.Add(key, sprite);
            }

            return _cachedSprite[key];
        }

        private const string UIFX_PRFAB_PATH = "FX_Prefabs/UIFX_Prefab/{0}";
        public GameObject UIFXParent => UIManager.Instance.GetLayer(UIManager.eUILayer.VFX).gameObject;

        public void CreateUIVFXPool(string file, int capacity)
        {
            var pool = GameObjectPool.GetPool(file);
            if (pool == null)
            {
                pool = GameObjectPool.CreatePool(file, () =>
                {
                    var prefab = LoadPrefab(string.Format(UIFX_PRFAB_PATH, file));
                    if (prefab != null)
                    {
                        var go = Instantiate(prefab, UIFXParent.transform);
                        go.transform.LocalReset();
                        go.gameObject.SetActive(false);
                        return go;
                    }

                    return null;
                }, capacity, UIFXParent, Define.Key.UIVFXPoolCategory);
            }
            else
                pool.AddCapacity(capacity);
        }
        public GameObject GetUIVFX(string file)
        {
            var pool = GameObjectPool.GetPool(file);
            if (pool == null)
            {
                pool = GameObjectPool.CreatePool(file, () =>
                {
                    var prefab = LoadPrefab(string.Format(UIFX_PRFAB_PATH, file));
                    if (prefab != null)
                    {
                        var go = Instantiate(prefab, UIFXParent.transform);
                        go.transform.LocalReset();
                        go.gameObject.SetActive(false);
                        return go;
                    }

                    return null;
                }, 1, UIFXParent, Define.Key.UIVFXPoolCategory);
            }

            var go = pool.Get();
            go.name = pool.Key;
            return go;
        }

        public void RestoreUIVFX(GameObject go)
        {
            if (go == null) return;

            var pool = GameObjectPool.GetPool(go.name);
            if (pool != null)
                pool.Restore(go);
        }
    }
}