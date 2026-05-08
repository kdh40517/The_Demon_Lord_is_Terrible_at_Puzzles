using UnityEngine;

namespace SeoAhn
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("효과음")]
        [SerializeField] private AudioClip buttonClickClip;

        private float masterVolume = 1f;
        private float bgmVolume = 1f;
        private float sfxVolume = 1f;

        public void SetMasterVolume(float sliderValue)
        {
            masterVolume = Mathf.Clamp01(sliderValue);
            ApplyVolumes();
        }

        public void SetBGMVolume(float sliderValue)
        {
            bgmVolume = Mathf.Clamp01(sliderValue);
            ApplyVolumes();
        }

        public void SetSFXVolume(float sliderValue)
        {
            sfxVolume = Mathf.Clamp01(sliderValue);
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = masterVolume * bgmVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = masterVolume * sfxVolume;
            }
        }

        public void PlayButtonClickSFX()
        {
            if (sfxSource == null || buttonClickClip == null)
            {
                Debug.LogWarning("SFX Source 또는 Button Click Clip이 비어있어.");
                return;
            }

            sfxSource.PlayOneShot(buttonClickClip);
        }
    }
}