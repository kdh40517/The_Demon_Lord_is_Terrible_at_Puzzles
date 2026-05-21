using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    [RequireComponent(typeof(AudioSource))]
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI 패널 연결")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject optionPanel;
        [SerializeField] private GameObject confirmPanel; // 👇 새로 추가된 재확인 팝업창!

        [Header("씬 이동 설정")]
        [SerializeField] private string mainMenuSceneName = "01_TitleScene";

        [Header("슬라이더 연결 (옵션창용)")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("옵션창 효과음 설정 (구멍!)")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip sliderTickSound;

        private AudioSource myAudioSource;

        // 👇 플레이어가 '재시작'을 눌렀는지, '메뉴'를 눌렀는지 기억하는 메모장입니다.
        private enum ConfirmType { None, Restart, MainMenu }
        private ConfirmType pendingAction = ConfirmType.None;

        private void Awake()
        {
            myAudioSource = GetComponent<AudioSource>();
            myAudioSource.playOnAwake = false;
            myAudioSource.loop = false;
        }

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionPanel != null) optionPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false); // 시작할 때 숨김!

            if (masterSlider != null) masterSlider.value = AudioVolumeData.MasterVolume;
            if (bgmSlider != null) bgmSlider.value = AudioVolumeData.BGMVolume;
            if (sfxSlider != null) sfxSlider.value = AudioVolumeData.SFXVolume;

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // 1. 재확인 팝업창이 켜져있을 때 ESC 누르면 -> "아니오" 누른 것과 똑같이 취소!
                if (confirmPanel != null && confirmPanel.activeSelf)
                {
                    CancelConfirm();
                }
                // 2. 옵션창이 켜져있을 때 -> 옵션 닫기
                else if (optionPanel != null && optionPanel.activeSelf)
                {
                    CloseOptionMenu();
                }
                // 3. 일시정지 창 켜고 끄기
                else if (pausePanel != null)
                {
                    bool isOpening = !pausePanel.activeSelf;
                    pausePanel.SetActive(isOpening);
                    Time.timeScale = isOpening ? 0f : 1f;

                    if (isOpening) PlaySound(buttonClickSound);
                }
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (myAudioSource != null && clip != null)
            {
                myAudioSource.volume = AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume;
                myAudioSource.PlayOneShot(clip);
            }
        }

        // ==========================================
        // 🔘 기존 버튼 기능들
        // ==========================================
        public void ResumeGame()
        {
            PlaySound(buttonClickSound);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionPanel != null) optionPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void OpenOptionMenu()
        {
            PlaySound(buttonClickSound);
            if (optionPanel != null) optionPanel.SetActive(true);
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void CloseOptionMenu()
        {
            PlaySound(buttonClickSound);
            if (optionPanel != null) optionPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        // 👇 재시작/메뉴 버튼을 누르면 바로 씬이 안 넘어가고 "팝업창"을 띄웁니다!
        public void RestartGame()
        {
            PlaySound(buttonClickSound);
            pendingAction = ConfirmType.Restart; // "재시작을 누름!" 이라고 기억
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        public void GoToMainMenu()
        {
            PlaySound(buttonClickSound);
            pendingAction = ConfirmType.MainMenu; // "메뉴로 가기를 누름!" 이라고 기억
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        // ==========================================
        // 🚨 팝업창의 [예] / [아니오] 버튼 기능
        // ==========================================
        public void ConfirmAction() // [예] 버튼에 연결!
        {
            PlaySound(buttonClickSound);
            Time.timeScale = 1f; // 시간 복구

            if (pendingAction == ConfirmType.Restart)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (pendingAction == ConfirmType.MainMenu)
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        public void CancelConfirm() // [아니오] 버튼에 연결!
        {
            PlaySound(buttonClickSound);
            pendingAction = ConfirmType.None; // 기억 지우기
            if (confirmPanel != null) confirmPanel.SetActive(false); // 팝업만 닫기 (일시정지 창은 남아있음)
        }

        // ==========================================
        // 🔊 오디오 조절 (기존과 동일)
        // ==========================================
        private void OnMasterChanged(float value) { AudioVolumeData.MasterVolume = value; UpdateAllAudiosInScene(); }
        private void OnBGMChanged(float value) { AudioVolumeData.BGMVolume = value; UpdateAllAudiosInScene(); }
        private void OnSFXChanged(float value)
        {
            AudioVolumeData.SFXVolume = value;
            UpdateAllAudiosInScene();
            if (!myAudioSource.isPlaying) PlaySound(sliderTickSound);
        }

        private void UpdateAllAudiosInScene()
        {
            AudioManager[] audioManagers = FindObjectsOfType<AudioManager>();
            foreach (var am in audioManagers) { am.SetMasterVolume(AudioVolumeData.MasterVolume); }

            SavedVolumeAudioSource[] savedAudios = FindObjectsOfType<SavedVolumeAudioSource>();
            foreach (var saved in savedAudios) { saved.ApplyVolume(); }
        }
    }
}