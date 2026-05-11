using UnityEngine;

namespace SeoAhn
{
    // 게임 실행 중 오디오 볼륨을 관리하는 스크립트입니다.
    // 씬이 바뀌어도 AudioVolumeData에 저장된 값으로 볼륨을 유지합니다.
    // 단, 게임을 완전히 껐다 켜면 다시 1로 초기화됩니다.
    public class AudioManager : MonoBehaviour
    {
        [Header("오디오 소스")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("효과음")]
        [SerializeField] private AudioClip buttonClickClip;

        private void Awake()
        {
            ApplyVolumes();
        }

        public void SetMasterVolume(float sliderValue)
        {
            AudioVolumeData.MasterVolume = Mathf.Clamp01(sliderValue);
            ApplyVolumes();
        }

        public void SetBGMVolume(float sliderValue)
        {
            AudioVolumeData.BGMVolume = Mathf.Clamp01(sliderValue);
            ApplyVolumes();
        }

        public void SetSFXVolume(float sliderValue)
        {
            AudioVolumeData.SFXVolume = Mathf.Clamp01(sliderValue);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = AudioVolumeData.MasterVolume * AudioVolumeData.BGMVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume;
            }
        }

        public float GetMasterVolume()
        {
            return AudioVolumeData.MasterVolume;
        }

        public float GetBGMVolume()
        {
            return AudioVolumeData.BGMVolume;
        }

        public float GetSFXVolume()
        {
            return AudioVolumeData.SFXVolume;
        }

        public void PlayButtonClickSFX()
        {
            if (sfxSource == null || buttonClickClip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(
                buttonClickClip,
                AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume
            );
        }
    }
}