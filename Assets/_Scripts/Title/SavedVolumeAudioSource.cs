using UnityEngine;

namespace SeoAhn
{
    // 이 스크립트는 각 씬의 AudioSource에 저장된 볼륨 설정을 적용합니다.
    // 타이틀 옵션에서 저장한 MasterVolume, BGMVolume 값을 읽어서
    // 다른 씬의 BGM 소리에도 똑같이 반영되게 합니다.
    public class SavedVolumeAudioSource : MonoBehaviour
    {
        [Header("적용할 AudioSource")]
        [SerializeField] private AudioSource audioSource; // 볼륨을 적용할 AudioSource

        [Header("오디오 종류")]
        [SerializeField] private bool isBGM = true; // true면 BGM 볼륨, false면 SFX 볼륨 적용

        private void Awake()
        {
            // AudioSource를 직접 연결하지 않았으면 같은 오브젝트에서 자동으로 찾습니다.
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            ApplySavedVolume();
        }

        private void Start()
        {
            ApplySavedVolume();
        }

        private void ApplySavedVolume()
        {
            if (audioSource == null)
            {
                return;
            }

            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (isBGM)
            {
                audioSource.volume = masterVolume * bgmVolume;
            }
            else
            {
                audioSource.volume = masterVolume * sfxVolume;
            }
        }
    }
}