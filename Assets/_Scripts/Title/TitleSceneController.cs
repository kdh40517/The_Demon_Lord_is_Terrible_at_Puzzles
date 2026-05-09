using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    public class TitleSceneController : MonoBehaviour
    {
        [Header("옵션창")]
        [SerializeField] private GameObject optionPanel;

        [Header("이름 입력창")]
        [SerializeField] private GameObject nameInputPanel;
        [SerializeField] private TMP_InputField nameInputField;

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

            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }

            SetupSlider(masterSlider);
            SetupSlider(bgmSlider);
            SetupSlider(sfxSlider);

            SetSliderValue(masterSlider, 1f);
            SetSliderValue(bgmSlider, 1f);
            SetSliderValue(sfxSlider, 1f);

            ChangeMasterVolume(1f);
            ChangeBGMVolume(1f);
            ChangeSFXVolume(1f);
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
            if (slider != null)
            {
                slider.value = value;
            }
        }

        public void OpenNameInput()
        {
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(true);
            }

            if (nameInputField != null)
            {
                nameInputField.text = string.Empty;
                nameInputField.ActivateInputField();
            }
        }

        public void ConfirmNameAndStart()
        {
            if (nameInputField == null)
            {
                return;
            }

            string playerName = nameInputField.text.Trim();

            if (string.IsNullOrEmpty(playerName))
            {
                Debug.Log("닉네임을 입력해야 합니다.");
                return;
            }

            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();

            SceneManager.LoadScene("02_StoryScene");
        }

        public void OpenOptions()
        {
            if (optionPanel != null)
            {
                optionPanel.SetActive(true);
            }
        }

        public void CloseOptions()
        {
            if (optionPanel != null)
            {
                optionPanel.SetActive(false);
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