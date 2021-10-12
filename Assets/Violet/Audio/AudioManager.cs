using System;
using DG.Tweening;
using UnityEngine;

namespace Violet.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        private const int DEFAULT_AUDIO_ITEM_POOLING_COUNT = 10;
        private const string AUDIO_ITEM_NAME = "Sound/AudioItem";

        private const string BGM_VOLUME_KEY = "bgm_volume";
        private const string SFX_VOLUME_KEY = "sfx_volume";

        [SerializeField] private AudioListener _staticAudioListner;

        public float _3D_Blend_Ratio = 0.95f;

        private AudioItem _audioItemBGM;
        private GameObject _audioItemPrefab;

        private float _bgmVolume, _sfxVolume;

        private void OnChangeScene(eSCENE_TYPE type)
        {
            PlayBGM(type);
        }

        public void Init()
        {
            _bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
            _sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.5f);
            SceneDirector.Instance.ChangeCompleteCallback += OnChangeScene;
        }

        //public void CacheAudioClip(string clipName)
        //{
        //    ResourceManager.Instance.LoadAsset<AudioClip>(clipName);
        //}

        public AudioItem Play(string clipName, eAUDIO_TYPE audioType = eAUDIO_TYPE.SFX)
        {
            var clip = ResourceManager.Instance.LoadAudioClip(clipName);
            if (clip == null)
            {
                VioletLogger.LogWarningFormat("AudioItem Play [clipName]{0} 이 없음!", clipName);
                return null;
            }

            AudioItem item = null;

            if (_audioItemPrefab == null)
                _audioItemPrefab = ResourceManager.Instance.LoadPrefab(AUDIO_ITEM_NAME);

            if (MonoObjectPoolManager.Instance.Contains<AudioItem>(AUDIO_ITEM_NAME) == false)
                MonoObjectPoolManager.Instance.CreatePool<AudioItem>(_audioItemPrefab, AUDIO_ITEM_NAME,
                    DEFAULT_AUDIO_ITEM_POOLING_COUNT, transform);

            switch (audioType)
            {
                case eAUDIO_TYPE.BGM:
                {
                    if (_audioItemBGM == null)
                        _audioItemBGM = MonoObjectPoolManager.Instance.GetMonoObject<AudioItem>(AUDIO_ITEM_NAME);

                    if (_audioItemBGM.AudioSource.clip == clip)
                        // 이미 재생중인 BGM
                        return _audioItemBGM;

                    item = _audioItemBGM;
                    item.Loop = true;

                    // 2D 사운드로 재생
                    item.AudioSource.spatialBlend = 0f;
                }
                    break;
                case eAUDIO_TYPE.SFX:
                {
                    item = MonoObjectPoolManager.Instance.GetMonoObject<AudioItem>(AUDIO_ITEM_NAME);

                    // 2D 사운드로 재생
                    item.AudioSource.spatialBlend = 0f;
                }
                    break;
                case eAUDIO_TYPE.SFX_3D:
                {
                    item = MonoObjectPoolManager.Instance.GetMonoObject<AudioItem>(AUDIO_ITEM_NAME);

                    // _3D_Blend_Ratio 만큼의 3D 사운드 + (1 - _3D_Blend_Ratio)만큼의 2D사운드로 재생
                    item.AudioSource.spatialBlend = _3D_Blend_Ratio;
                }
                    break;
            }

            item.gameObject.SetActive(true);

            item.Play(clip);

            if (audioType == eAUDIO_TYPE.BGM)
                FadeIn(eAUDIO_TYPE.BGM, 2f);
            else if (audioType == eAUDIO_TYPE.SFX)
                item.Volume = _sfxVolume;

            return item;
        }

        public void Release(AudioItem item)
        {
            MonoObjectPoolManager.Instance.ReturnMonoObject(AUDIO_ITEM_NAME, item);
        }

        public void ChangeBGMVolume(float volume)
        {
            _bgmVolume = volume;
            if (_audioItemBGM != null)
                _audioItemBGM.Volume = _bgmVolume;
        }

        public void FadeIn(eAUDIO_TYPE type, float duration, Action onComplete = null)
        {
            switch (type)
            {
                case eAUDIO_TYPE.BGM:
                    _audioItemBGM.AudioSource.volume = 0;
                    _audioItemBGM.AudioSource.DOFade(_bgmVolume, duration);
                    break;
            }
        }

        public void FadeOut(eAUDIO_TYPE type, float duration, Action onComplete = null)
        {
            switch (type)
            {
                case eAUDIO_TYPE.BGM:
                    _audioItemBGM.AudioSource.volume = _bgmVolume;
                    _audioItemBGM.AudioSource.DOFade(0, duration);
                    break;
            }
        }

        public void PlayBGM(eSCENE_TYPE type)
        {
            switch (type)
            {
                case eSCENE_TYPE.None:
                    break;
            }
        }

        public void StopBGM()
        {
            if (_audioItemBGM != null)
                _audioItemBGM.Stop();
        }

        public void ChangeSFXVolume(float volume)
        {
            _sfxVolume = volume;
        }

        // 씬에서 별도로 사용하는 오디오 리스너가 있다면
        // 기본 오디오 리스너를 비활성화 (기본 오디오 리스너는 씬 전환 시 다시 켜짐)
        public void SetSceneAudioListner(AudioListener audioListener)
        {
            if (audioListener != _staticAudioListner)
                _staticAudioListner.enabled = false;
        }

        public void ResetAudioListner()
        {
            _staticAudioListner.enabled = true;
        }
    }
}