using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    public class TitleSceneController : MonoBehaviour
    {
        [Header("옵션창")]
        [SerializeField] private GameObject optionPanel;

        [Header("타이틀 화면 오브젝트")]
        [SerializeField] private GameObject titleLogo;
        [SerializeField] private GameObject startButton;
        [SerializeField] private GameObject optionButton;

        [Header("슬라이더")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("오디오")]
        [SerializeField] private AudioManager audioManager;

        private void Start()
        {
            if (optionPanel != null)
            {
                optionPanel.SetActive(false);
            }

            ShowTitleObjects();

            SetupSlider(masterSlider);
            SetupSlider(bgmSlider);
            SetupSlider(sfxSlider);

            SetSliderValue(masterSlider, 1f);
            SetSliderValue(bgmSlider, 1f);
            SetSliderValue(sfxSlider, 1f);

            ChangeMasterVolume(GetSliderValue(masterSlider));
            ChangeBGMVolume(GetSliderValue(bgmSlider));
            ChangeSFXVolume(GetSliderValue(sfxSlider));
        }

        private void SetupSlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }

        private void SetSliderValue(Slider slider, float value)
        {
            if (slider == null)
            {
                return;
            }

            slider.value = value;
        }

        private float GetSliderValue(Slider slider)
        {
            if (slider == null)
            {
                return 1f;
            }

            return slider.value;
        }

        public void StartGame()
        {
            SceneManager.LoadScene("02_StoryScene");
        }

        public void OpenOptions()
        {
            if (optionPanel != null)
            {
                optionPanel.SetActive(true);
            }

            HideTitleObjects();
        }

        public void CloseOptions()
        {
            if (optionPanel != null)
            {
                optionPanel.SetActive(false);
            }

            ShowTitleObjects();
        }

        private void HideTitleObjects()
        {
            if (titleLogo != null)
            {
                titleLogo.SetActive(false);
            }

            if (startButton != null)
            {
                startButton.SetActive(false);
            }

            if (optionButton != null)
            {
                optionButton.SetActive(false);
            }
        }

        private void ShowTitleObjects()
        {
            if (titleLogo != null)
            {
                titleLogo.SetActive(true);
            }

            if (startButton != null)
            {
                startButton.SetActive(true);
            }

            if (optionButton != null)
            {
                optionButton.SetActive(true);
            }
        }

        public void ChangeMasterVolume(float volume)
        {
            if (audioManager != null)
            {
                audioManager.SetMasterVolume(volume);
            }
        }

        public void ChangeBGMVolume(float volume)
        {
            if (audioManager != null)
            {
                audioManager.SetBGMVolume(volume);
            }
        }

        public void ChangeSFXVolume(float volume)
        {
            if (audioManager != null)
            {
                audioManager.SetSFXVolume(volume);
            }
        }
    }
}