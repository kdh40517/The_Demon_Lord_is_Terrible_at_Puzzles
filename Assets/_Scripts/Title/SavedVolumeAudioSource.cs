using UnityEngine;

namespace SeoAhn
{
    // 각 씬의 AudioSource에 현재 게임 실행 중 볼륨값을 적용하는 스크립트입니다.
    public class SavedVolumeAudioSource : MonoBehaviour
    {
        [Header("적용할 AudioSource")]
        [SerializeField] private AudioSource audioSource;

        [Header("오디오 종류")]
        [SerializeField] private bool isBGM = true;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            ApplyVolume();
        }

        private void Start()
        {
            ApplyVolume();
        }

        public void ApplyVolume()
        {
            if (audioSource == null)
            {
                return;
            }

            if (isBGM)
            {
                audioSource.volume = AudioVolumeData.MasterVolume * AudioVolumeData.BGMVolume;
            }
            else
            {
                audioSource.volume = AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume;
            }
        }
    }
}