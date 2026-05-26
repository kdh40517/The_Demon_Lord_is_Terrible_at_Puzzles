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
        [SerializeField] private GameObject confirmPanel;

        [Header("씬 이동 설정")]
        [SerializeField] private string mainMenuSceneName = "01_TitleScene";

        [Header("일시정지 BGM 설정")]
        [Tooltip("비워두시면 게임 시작 시 알아서 BGMManager를 찾아 연결합니다!")]
        [SerializeField] private AudioSource bgmPlayer;
        [SerializeField][Range(0f, 1f)] private float pausedBgmRatio = 0.3f;

        [Header("슬라이더 연결 (옵션창용)")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        [Header("옵션창 효과음 설정 (구멍!)")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip sliderTickSound;

        private AudioSource myAudioSource;
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
            if (confirmPanel != null) confirmPanel.SetActive(false);

            if (masterSlider != null) masterSlider.value = AudioVolumeData.MasterVolume;
            if (bgmSlider != null) bgmSlider.value = AudioVolumeData.BGMVolume;
            if (sfxSlider != null) sfxSlider.value = AudioVolumeData.SFXVolume;

            if (masterSlider != null) masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(OnBGMChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSFXChanged);

            // 👇 씬이 시작되면 스피커를 알아서 찾습니다!
            AutoFindBGMPlayer();
            UpdateBGMVolume();
        }

        // ==========================================
        // 🕵️‍♂️ BGM 스피커 자동 추적 마법 함수
        // ==========================================
        private void AutoFindBGMPlayer()
        {
            // 이미 드래그해서 넣은 게 있다면 그대로 씁니다.
            if (bgmPlayer != null) return;

            // 1순위: 권삣삐님 씬 구조처럼 "BGMManager"라는 이름의 오브젝트를 찾습니다.
            GameObject bgmObj = GameObject.Find("BGMManager");
            if (bgmObj != null)
            {
                bgmPlayer = bgmObj.GetComponent<AudioSource>();
                if (bgmPlayer != null) return;
            }

            // 2순위: 만약 이름이 다르다면? -> 씬 전체를 뒤져서 '무한 반복(Loop)'이 켜져 있는 스피커를 BGM으로 간주하고 납치합니다!
            AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
            foreach (var audio in allAudioSources)
            {
                // 자기 자신(옵션창 스피커)은 제외하고, 루프가 켜져 있는 녀석 찾기
                if (audio.loop && audio.gameObject != this.gameObject)
                {
                    bgmPlayer = audio;
                    Debug.Log($"[자동 연결] {audio.gameObject.name} 오브젝트를 BGM 스피커로 인식했습니다!");
                    return;
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (confirmPanel != null && confirmPanel.activeSelf)
                {
                    CancelConfirm();
                }
                else if (optionPanel != null && optionPanel.activeSelf)
                {
                    CloseOptionMenu();
                }
                else if (pausePanel != null)
                {
                    bool isOpening = !pausePanel.activeSelf;
                    pausePanel.SetActive(isOpening);
                    Time.timeScale = isOpening ? 0f : 1f;

                    if (isOpening) PlaySound(buttonClickSound);

                    UpdateBGMVolume();
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

        private void UpdateBGMVolume()
        {
            if (bgmPlayer == null) return;

            bool isPaused = (pausePanel != null && pausePanel.activeSelf) ||
                            (optionPanel != null && optionPanel.activeSelf) ||
                            (confirmPanel != null && confirmPanel.activeSelf);

            float targetVolume = AudioVolumeData.MasterVolume * AudioVolumeData.BGMVolume;

            if (isPaused)
            {
                targetVolume *= pausedBgmRatio;
            }

            bgmPlayer.volume = targetVolume;
        }

        public void ResumeGame()
        {
            PlaySound(buttonClickSound);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionPanel != null) optionPanel.SetActive(false);
            if (confirmPanel != null) confirmPanel.SetActive(false);
            Time.timeScale = 1f;

            UpdateBGMVolume();
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

        public void RestartGame()
        {
            PlaySound(buttonClickSound);
            pendingAction = ConfirmType.Restart;
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        public void GoToMainMenu()
        {
            PlaySound(buttonClickSound);
            pendingAction = ConfirmType.MainMenu;
            if (confirmPanel != null) confirmPanel.SetActive(true);
        }

        public void ConfirmAction()
        {
            PlaySound(buttonClickSound);
            Time.timeScale = 1f;

            if (pendingAction == ConfirmType.Restart)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else if (pendingAction == ConfirmType.MainMenu)
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }

        public void CancelConfirm()
        {
            PlaySound(buttonClickSound);
            pendingAction = ConfirmType.None;
            if (confirmPanel != null) confirmPanel.SetActive(false);
        }

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

            UpdateBGMVolume();
        }
    }
}