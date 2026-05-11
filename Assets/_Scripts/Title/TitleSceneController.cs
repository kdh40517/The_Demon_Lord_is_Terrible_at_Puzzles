using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    // 타이틀 씬에서 옵션창, 닉네임 입력창, 볼륨 슬라이더,
    // 닉네임 저장 후 스토리 씬 이동을 관리하는 스크립트입니다.
    public class TitleSceneController : MonoBehaviour
    {
        [Header("옵션창")]
        [SerializeField] private GameObject optionPanel; // 옵션창 패널

        [Header("이름 입력창")]
        [SerializeField] private GameObject nameInputPanel; // 닉네임 입력 패널
        [SerializeField] private TMP_InputField nameInputField; // 닉네임 입력 필드

        [Header("닉네임 경고 메시지")]
        [SerializeField] private GameObject nicknameWarningPanel; // 경고 메시지를 감싸는 Image 오브젝트
        [SerializeField] private CanvasGroup nicknameWarningCanvasGroup; // 경고 패널 페이드용 CanvasGroup
        [SerializeField] private TMP_Text nicknameWarningText; // 경고 패널 안의 TMP 텍스트
        [SerializeField] private float warningFadeTime = 0.25f; // 나타나고 사라지는 시간
        [SerializeField] private float warningStayTime = 1.1f; // 유지되는 시간

        [Header("슬라이더")]
        [SerializeField] private Slider masterSlider; // 전체 볼륨 슬라이더
        [SerializeField] private Slider bgmSlider; // BGM 볼륨 슬라이더
        [SerializeField] private Slider sfxSlider; // SFX 볼륨 슬라이더

        [Header("오디오")]
        [SerializeField] private AudioManager audioManager; // 볼륨을 실제로 조절할 AudioManager

        private Coroutine warningCoroutine; // 경고 메시지 페이드 코루틴

        private void Awake()
        {
            // NicknameWarningPanel에 CanvasGroup이 없으면 자동으로 추가합니다.
            if (nicknameWarningPanel != null && nicknameWarningCanvasGroup == null)
            {
                nicknameWarningCanvasGroup = nicknameWarningPanel.GetComponent<CanvasGroup>();

                if (nicknameWarningCanvasGroup == null)
                {
                    nicknameWarningCanvasGroup = nicknameWarningPanel.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            // 시작 시 옵션창은 숨깁니다.
            if (optionPanel != null)
            {
                optionPanel.SetActive(false);
            }

            // 시작 시 닉네임 입력창은 숨깁니다.
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(false);
            }

            // 시작 시 닉네임 경고 패널은 숨깁니다.
            if (nicknameWarningPanel != null)
            {
                nicknameWarningPanel.SetActive(false);
            }

            if (nicknameWarningCanvasGroup != null)
            {
                nicknameWarningCanvasGroup.alpha = 0f;
            }

            // 슬라이더 기본 범위를 0~1로 설정합니다.
            SetupSlider(masterSlider);
            SetupSlider(bgmSlider);
            SetupSlider(sfxSlider);

            // 시작 볼륨은 전부 100%로 맞춥니다.
            SetSliderValue(masterSlider, AudioVolumeData.MasterVolume);
            SetSliderValue(bgmSlider, AudioVolumeData.BGMVolume);
            SetSliderValue(sfxSlider, AudioVolumeData.SFXVolume);

            ChangeMasterVolume(masterSlider.value);
            ChangeBGMVolume(bgmSlider.value);
            ChangeSFXVolume(sfxSlider.value);
        }

        private void SetupSlider(Slider slider)
        {
            // 슬라이더가 비어있으면 아무것도 하지 않습니다.
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
            // 슬라이더 값 설정
            if (slider != null)
            {
                slider.value = value;
            }
        }

        public void OpenNameInput()
        {
            // 게임 시작 버튼을 눌렀을 때 닉네임 입력창을 엽니다.
            if (nameInputPanel != null)
            {
                nameInputPanel.SetActive(true);
            }

            // 입력칸을 비우고 바로 입력할 수 있게 포커스를 줍니다.
            if (nameInputField != null)
            {
                nameInputField.text = string.Empty;
                nameInputField.ActivateInputField();
            }

            // 이전에 떠 있던 경고창이 있으면 숨깁니다.
            HideNicknameWarningInstantly();
        }

        public void ConfirmNameAndStart()
        {
            // 입력 필드가 연결되지 않았다면 진행하지 않습니다.
            if (nameInputField == null)
            {
                return;
            }

            string playerName = nameInputField.text.Trim();

            // 닉네임이 비어 있으면 경고 메시지를 보여주고 씬 이동을 막습니다.
            if (string.IsNullOrEmpty(playerName))
            {
                ShowNicknameWarning();
                return;
            }

            // 입력한 닉네임을 저장합니다.
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();

            // 스토리 씬으로 이동합니다.
            SceneManager.LoadScene("02_StoryScene");
        }

        private void ShowNicknameWarning()
        {
            // 경고 패널, 텍스트, CanvasGroup 중 하나라도 없으면 진행하지 않습니다.
            if (nicknameWarningPanel == null || nicknameWarningText == null || nicknameWarningCanvasGroup == null)
            {
                return;
            }

            // 이미 경고 코루틴이 실행 중이면 중지하고 다시 시작합니다.
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
            }

            warningCoroutine = StartCoroutine(ShowNicknameWarningRoutine());
        }

        private IEnumerator ShowNicknameWarningRoutine()
        {
            // 경고 문구 설정
            nicknameWarningText.text = "닉네임을 입력해주세요!";

            // 경고 패널 표시
            nicknameWarningPanel.SetActive(true);

            // 서서히 나타나기
            yield return StartCoroutine(FadeNicknameWarning(0f, 1f));

            // 잠깐 유지
            yield return new WaitForSeconds(warningStayTime);

            // 서서히 사라지기
            yield return StartCoroutine(FadeNicknameWarning(1f, 0f));

            // 완전히 사라지면 패널 비활성화
            nicknameWarningPanel.SetActive(false);
            warningCoroutine = null;
        }

        private IEnumerator FadeNicknameWarning(float startAlpha, float endAlpha)
        {
            // CanvasGroup 알파값을 이용해 자연스럽게 나타나고 사라지게 합니다.
            float timer = 0f;

            while (timer < warningFadeTime)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / warningFadeTime);

                if (nicknameWarningCanvasGroup != null)
                {
                    nicknameWarningCanvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (nicknameWarningCanvasGroup != null)
            {
                nicknameWarningCanvasGroup.alpha = endAlpha;
            }
        }

        private void HideNicknameWarningInstantly()
        {
            // 닉네임 입력창을 새로 열 때 이전 경고 메시지를 즉시 숨깁니다.
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
                warningCoroutine = null;
            }

            if (nicknameWarningCanvasGroup != null)
            {
                nicknameWarningCanvasGroup.alpha = 0f;
            }

            if (nicknameWarningPanel != null)
            {
                nicknameWarningPanel.SetActive(false);
            }
        }

        public void OpenOptions()
        {
            // 옵션 버튼을 누르면 옵션창을 엽니다.
            if (optionPanel != null)
            {
                optionPanel.SetActive(true);
            }
        }

        public void CloseOptions()
        {
            // 닫기 버튼을 누르면 옵션창을 닫습니다.
            if (optionPanel != null)
            {
                optionPanel.SetActive(false);
            }
        }

        public void ChangeMasterVolume(float volume)
        {
            // 마스터 볼륨 변경
            if (audioManager != null)
            {
                audioManager.SetMasterVolume(volume);
            }
        }

        public void ChangeBGMVolume(float volume)
        {
            // BGM 볼륨 변경
            if (audioManager != null)
            {
                audioManager.SetBGMVolume(volume);
            }
        }

        public void ChangeSFXVolume(float volume)
        {
            // SFX 볼륨 변경
            if (audioManager != null)
            {
                audioManager.SetSFXVolume(volume);
            }
        }
    }
}