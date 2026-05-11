using UnityEngine;

namespace SeoAhn
{
    // 게임 전체 오디오 볼륨을 관리하는 스크립트입니다.
    // 마스터 / BGM / SFX 볼륨을 PlayerPrefs에 저장해서
    // 씬이 바뀌어도 같은 볼륨 설정을 유지합니다.
    public class AudioManager : MonoBehaviour
    {
        [Header("오디오 소스")]
        [SerializeField] private AudioSource bgmSource; // BGM 재생용 AudioSource
        [SerializeField] private AudioSource sfxSource; // 효과음 재생용 AudioSource

        [Header("효과음")]
        [SerializeField] private AudioClip buttonClickClip; // 버튼 클릭 효과음

        private float masterVolume = 1f; // 전체 볼륨
        private float bgmVolume = 1f;    // BGM 볼륨
        private float sfxVolume = 1f;    // SFX 볼륨

        private void Awake()
        {
            // 저장된 볼륨값을 불러옵니다.
            LoadVolumes();

            // 불러온 값을 실제 AudioSource에 적용합니다.
            ApplyVolumes();
        }

        public void SetMasterVolume(float sliderValue)
        {
            // 마스터 볼륨을 0~1 사이로 저장합니다.
            masterVolume = Mathf.Clamp01(sliderValue);

            SaveVolumes();
            ApplyVolumes();
        }

        public void SetBGMVolume(float sliderValue)
        {
            // BGM 볼륨을 0~1 사이로 저장합니다.
            bgmVolume = Mathf.Clamp01(sliderValue);

            SaveVolumes();
            ApplyVolumes();
        }

        public void SetSFXVolume(float sliderValue)
        {
            // SFX 볼륨을 0~1 사이로 저장합니다.
            sfxVolume = Mathf.Clamp01(sliderValue);

            SaveVolumes();
            ApplyVolumes();
        }

        private void LoadVolumes()
        {
            // 저장된 값이 없으면 기본값 1을 사용합니다.
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        }

        private void SaveVolumes()
        {
            // 현재 볼륨값을 저장합니다.
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        private void ApplyVolumes()
        {
            // 실제 BGM 소리에 마스터 볼륨과 BGM 볼륨을 함께 적용합니다.
            if (bgmSource != null)
            {
                bgmSource.volume = masterVolume * bgmVolume;
            }

            // 실제 효과음 소리에 마스터 볼륨과 SFX 볼륨을 함께 적용합니다.
            if (sfxSource != null)
            {
                sfxSource.volume = masterVolume * sfxVolume;
            }
        }

        public float GetMasterVolume()
        {
            return masterVolume;
        }

        public float GetBGMVolume()
        {
            return bgmVolume;
        }

        public float GetSFXVolume()
        {
            return sfxVolume;
        }

        public void PlayButtonClickSFX()
        {
            // 버튼 클릭 효과음을 재생합니다.
            if (sfxSource == null || buttonClickClip == null)
            {
                Debug.LogWarning("SFX Source 또는 Button Click Clip이 비어있어.");
                return;
            }

            sfxSource.PlayOneShot(buttonClickClip, masterVolume * sfxVolume);
        }
    }
}