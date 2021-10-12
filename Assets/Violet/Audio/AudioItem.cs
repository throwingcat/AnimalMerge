using UnityEngine;

namespace Violet.Audio
{
    public class AudioItem : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        public AudioSource AudioSource => _audioSource;

        public float Volume
        {
            get => _audioSource.volume;
            set => _audioSource.volume = value;
        }

        public bool Loop
        {
            get => _audioSource.loop;
            set => _audioSource.loop = value;
        }

        private void Update()
        {
            if (_audioSource != null &&
                _audioSource.clip != null &&
                _audioSource.isPlaying == false)
                Release();
        }

        public void Play(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }


        public void Stop()
        {
            _audioSource.Stop();
            _audioSource.clip = null;
        }

        public void Release()
        {
            Stop();
            Loop = false;
            Volume = 1;

            AudioManager.Instance.Release(this);
        }
    }
}