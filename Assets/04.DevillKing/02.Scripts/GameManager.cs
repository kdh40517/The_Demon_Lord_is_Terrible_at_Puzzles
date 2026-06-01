using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;

namespace DH
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public bool isGameStarted = false;
        public bool isGameOver = false;

        [Header("보스 등장 연출 (Intro)")]
        public CanvasGroup introBackground; // 화면 전체를 덮을 검은 배경 패널
        public CanvasGroup introBossImage;  // 페이드인 될 보스 일러스트
        public AudioClip introSound;        // 🚨 5초짜리 보스 등장 효과음!
        public float introFadeInTime = 1.5f;  // 일러스트가 스르륵 나타나는 시간
        public float introFadeOutTime = 1.0f; // 검은 화면이 사라지는 시간

        [Header("UI 연결")]
        public Slider bossHPBar;
        public Slider playerHPBar;
        public TextMeshProUGUI shieldText;
        public TextMeshProUGUI bossTurnText;
        public CanvasGroup clearPanel;
        public CanvasGroup gameOverPanel;

        [Header("보스 & 플레이어 설정")]
        public int bossHP = 100;
        public int bossMaxHP = 100;

        [SerializeField] private int playerMaxHP = 20;
        public int playerHP = 20;
        public int playerShield = 0;

        [Header("개별 이펙트 화면 연결")]
        public Animator attackEffect;
        public Animator healEffect;
        public Animator stoneEffect;
        public Animator breakEffect;
        public Animator EarthQuakeEffect;

        [Header("턴제 시스템")]
        public int turnCount = 0;

        [Header("사운드 설정 (효과음)")]
        public AudioSource sfxPlayer;
        public AudioClip bossAttackSound;
        public AudioClip[] patternSounds;

        [Header("BGM 설정 (배경음악)")]
        public AudioSource bgmPlayer; // 평소에 BGM을 틀고 있는 스피커
        public AudioClip clearBGM;
        public AudioClip gameOverBGM;

        [Header("오디오 믹서")]
        public AudioMixer mainMixer;

        [Header("씬 이동 설정")]
        public string stageSceneName = "03_StageScene";
        public string titleSceneName = "01_TitleScene";
        public float clearPanelFadeTime = 3.0f;
        public float clearMoveDelay = 1.5f;

        [Header("경고 연출")]
        public CanvasGroup warningPanel;
        private Coroutine warningCoroutine;

        private bool isClearMoving = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            // 연출이 끝날 때까지 게임 시작과 퍼즐 조작을 막습니다.
            isGameStarted = false;

            // 🚨 [핵심] 배경음악이 눈치 없이 먼저 나오는 걸 방지하기 위해 시작하자마자 강제로 끕니다!
            if (bgmPlayer != null)
            {
                bgmPlayer.Stop();
            }

            if (clearPanel != null)
            {
                clearPanel.gameObject.SetActive(false);
                clearPanel.alpha = 0f;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.gameObject.SetActive(false);
                gameOverPanel.alpha = 0f;
                gameOverPanel.blocksRaycasts = false;
            }

            InitializeUI();

            // 보스 등장 연출 코루틴 시작!
            StartCoroutine(BossIntroRoutine());
        }

        void InitializeUI()
        {
            if (bossHPBar != null)
            {
                bossHPBar.maxValue = bossMaxHP;
                bossHPBar.value = bossHP;
            }

            if (playerHPBar != null)
            {
                playerHPBar.maxValue = playerMaxHP;
                playerHPBar.value = playerHP;
            }

            UpdateHPUI();
        }

        // ==========================================
        // 🎬 보스 등장 연출 마법 코루틴 (타이밍 수정됨)
        // ==========================================
        IEnumerator BossIntroRoutine()
        {
            // 검은 화면은 켜고, 보스 일러스트는 투명하게 대기
            if (introBackground != null)
            {
                introBackground.gameObject.SetActive(true);
                introBackground.alpha = 1f;
                introBackground.blocksRaycasts = true;
            }
            if (introBossImage != null)
            {
                introBossImage.gameObject.SetActive(true);
                introBossImage.alpha = 0f;
            }

            // 씬 로딩 후 아주 잠깐(0.2초) 숨을 고른 뒤 효과음 재생
            yield return new WaitForSeconds(0.2f);

            // 1. 5초짜리 보스 등장 효과음 재생!
            if (sfxPlayer != null && introSound != null)
            {
                sfxPlayer.PlayOneShot(introSound);
            }

            // 2. 효과음 재생 시간(5.0초)을 완벽하게 감안하여 대기합니다.
            float soundDuration = 5.0f;
            float timer = 0f;

            while (timer < soundDuration)
            {
                timer += Time.deltaTime;

                // 설정한 introFadeInTime 동안만 보스 일러스트가 스르륵 나타납니다.
                if (introBossImage != null && timer < introFadeInTime)
                {
                    introBossImage.alpha = timer / introFadeInTime;
                }
                else if (introBossImage != null)
                {
                    introBossImage.alpha = 1f; // 페이드인이 끝나면 5초가 다 찰 때까지 선명하게 유지
                }

                yield return null;
            }

            // 3. 5초 효과음이 완벽하게 끝났으니, 검은 배경 화면을 페이드아웃 시키며 전투 화면 등장!
            timer = 0f;
            if (introBackground != null)
            {
                while (timer < introFadeOutTime)
                {
                    timer += Time.deltaTime;
                    introBackground.alpha = 1f - (timer / introFadeOutTime);
                    yield return null;
                }
                introBackground.alpha = 0f;
                introBackground.gameObject.SetActive(false);
            }

            // 4. [완벽한 타이밍] 연출 끝! 이제 게임을 활성화하고 배경음악을 스타트합니다!
            isGameStarted = true;

            if (bgmPlayer != null)
            {
                bgmPlayer.loop = true; // 인게임 전투 음악이므로 무한 반복 활성화
                bgmPlayer.Play();      // BGM 큐!
            }

            Debug.Log("🏁 5초 등장 사운드 종료 -> 인게임 BGM 재생 및 퍼즐 조작 활성화!");
        }

        // --- 이하 기존 인게임 로직 동일 ---
        public void AttackBoss(int damage)
        {
            if (isGameOver || !isGameStarted) return;
            bossHP -= damage;
            if (bossHP < 0) bossHP = 0;
            UpdateHPUI();
            if (bossHP <= 0) GameOver(true);
        }

        public void HealPlayer(int healAmount)
        {
            if (isGameOver || !isGameStarted) return;
            playerHP = Mathf.Min(playerHP + healAmount, playerMaxHP);
            UpdateHPUI();
        }

        public void AddShield(int shieldAmount)
        {
            if (isGameOver || !isGameStarted) return;
            playerShield += shieldAmount;
            UpdateHPUI();
        }

        public void NextTurn()
        {
            if (isGameOver) return;
            turnCount++;
            UpdateHPUI();
            if (turnCount % 3 == 0) StartCoroutine(BossAttackRoutine());
        }

        IEnumerator BossAttackRoutine()
        {
            if (isGameOver) yield break;

            if (sfxPlayer != null && bossAttackSound != null) sfxPlayer.PlayOneShot(bossAttackSound);
            if (attackEffect != null) attackEffect.SetTrigger("Attack");

            yield return new WaitForSeconds(1.0f);

            int incomingDamage = 10;
            if (playerShield > 0)
            {
                if (playerShield >= incomingDamage)
                {
                    playerShield -= incomingDamage;
                    incomingDamage = 0;
                }
                else
                {
                    incomingDamage -= playerShield;
                    playerShield = 0;
                }
            }

            if (incomingDamage > 0)
            {
                playerHP -= incomingDamage;
            }

            if (playerHP <= 0)
            {
                playerHP = 0;
                UpdateHPUI();
                GameOver(false);
                yield break;
            }

            UpdateHPUI();

            int dice = Random.Range(1, 101);
            if (dice <= 20)
            {
                if (healEffect != null) healEffect.SetTrigger("Heal");
                if (sfxPlayer != null && patternSounds.Length > 0) sfxPlayer.PlayOneShot(patternSounds[0]);
                yield return new WaitForSeconds(1.0f);
                bossHP += 20;
                if (bossHP > bossMaxHP) bossHP = bossMaxHP;
            }
            else if (dice <= 50)
            {
                if (EarthQuakeEffect != null) EarthQuakeEffect.SetTrigger("EarthQuake");
                if (sfxPlayer != null && patternSounds.Length > 1) sfxPlayer.PlayOneShot(patternSounds[1]);
                yield return new WaitForSeconds(1.0f);
                if (PuzzleManager.Instance != null) PuzzleManager.Instance.ShuffleBoard();
            }
            else if (dice <= 80)
            {
                if (stoneEffect != null) stoneEffect.SetTrigger("Stone");
                if (sfxPlayer != null && patternSounds.Length > 2) sfxPlayer.PlayOneShot(patternSounds[2]);
                yield return new WaitForSeconds(1.0f);
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(8);
            }
            else
            {
                if (breakEffect != null) breakEffect.SetTrigger("Break");
                if (sfxPlayer != null && patternSounds.Length > 3) sfxPlayer.PlayOneShot(patternSounds[3]);
                yield return new WaitForSeconds(1.0f);
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
            }

            UpdateHPUI();
        }

        void UpdateHPUI()
        {
            if (bossHPBar != null) bossHPBar.value = bossHP;
            if (playerHPBar != null) playerHPBar.value = playerHP;
            if (shieldText != null) shieldText.text = playerShield.ToString();

            if (bossTurnText != null)
            {
                int turnsUntilAttack = 3 - (turnCount % 3);
                bossTurnText.text = $"{turnsUntilAttack}";

                if (turnsUntilAttack == 1 && !isGameOver)
                {
                    if (warningCoroutine == null && warningPanel != null)
                    {
                        warningPanel.gameObject.SetActive(true);
                        warningCoroutine = StartCoroutine(WarningRoutine());
                    }
                }
                else
                {
                    if (warningCoroutine != null)
                    {
                        StopCoroutine(warningCoroutine);
                        warningCoroutine = null;
                    }

                    if (warningPanel != null)
                    {
                        warningPanel.alpha = 0f;
                        warningPanel.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void GameOver(bool isWin)
        {
            if (isGameOver) return;
            isGameOver = true;
            isGameStarted = false;

            if (bgmPlayer != null)
            {
                bgmPlayer.Stop();
                bgmPlayer.loop = false;
            }

            if (isWin)
            {
                if (bgmPlayer != null && clearBGM != null)
                {
                    bgmPlayer.clip = clearBGM;
                    bgmPlayer.Play();
                }
                if (!isClearMoving) StartCoroutine(ClearMoveRoutine());
            }
            else
            {
                if (bgmPlayer != null && gameOverBGM != null)
                {
                    bgmPlayer.clip = gameOverBGM;
                    bgmPlayer.Play();
                }
                StartCoroutine(GameOverRoutine());
            }
        }

        IEnumerator ClearMoveRoutine()
        {
            isClearMoving = true;
            if (clearPanel != null)
            {
                clearPanel.gameObject.SetActive(true);
                clearPanel.alpha = 0f;
                float timer = 0f;
                while (timer < clearPanelFadeTime)
                {
                    timer += Time.deltaTime;
                    clearPanel.alpha = timer / clearPanelFadeTime;
                    yield return null;
                }
                clearPanel.alpha = 1f;
            }

            SeoAhn.StageClearManager.SetDevillClear();
            yield return new WaitForSeconds(clearMoveDelay);
            SceneManager.LoadScene(stageSceneName);
        }

        IEnumerator GameOverRoutine()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.gameObject.SetActive(true);
                gameOverPanel.alpha = 0f;
                float timer = 0f;
                while (timer < clearPanelFadeTime)
                {
                    timer += Time.deltaTime;
                    gameOverPanel.alpha = timer / clearPanelFadeTime;
                    yield return null;
                }
                gameOverPanel.alpha = 1f;
                gameOverPanel.blocksRaycasts = true;
            }
        }

        IEnumerator WarningRoutine()
        {
            float timer = 0f;
            while (true)
            {
                timer += Time.deltaTime;
                warningPanel.alpha = Mathf.PingPong(timer * 2f, 0.4f);
                yield return null;
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToTitle()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(titleSceneName);
        }
        // ==========================================
        // 🛠️ 디버그용 화면 UI (OnGUI)
        // ==========================================
        void OnGUI()
        {
            // 화면 좌측 상단에 디버그 메뉴 배치
            GUILayout.BeginArea(new Rect(10, 10, 200, 300));
            GUILayout.Box("디버그 메뉴");

            if (GUILayout.Button("🌟 즉시 클리어"))
            {
                GameOver(true);
            }

            if (GUILayout.Button("패턴: 보스 체력 회복"))
            {
                if (healEffect != null) healEffect.SetTrigger("Heal");
                bossHP = Mathf.Min(bossHP + 20, bossMaxHP);
                UpdateHPUI();
            }

            if (GUILayout.Button("패턴: 지진 (보드 섞기)"))
            {
                if (EarthQuakeEffect != null) EarthQuakeEffect.SetTrigger("EarthQuake");
                if (PuzzleManager.Instance != null) PuzzleManager.Instance.ShuffleBoard();
            }

            if (GUILayout.Button("패턴: 돌 무작위 생성"))
            {
                if (stoneEffect != null) stoneEffect.SetTrigger("Stone");
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(8);
            }

            if (GUILayout.Button("패턴: 방어 타일 파괴"))
            {
                if (breakEffect != null) breakEffect.SetTrigger("Break");
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
            }

            GUILayout.EndArea();
        }
    }
}