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

        // 👇 새롭게 추가된 BGM 교체 시스템!
        [Header("BGM 설정 (배경음악)")]
        public AudioSource bgmPlayer; // 평소에 BGM을 틀고 있는 스피커
        public AudioClip clearBGM;    // 승리 시 틀어줄 빰빠빰 BGM
        public AudioClip gameOverBGM; // 패배 시 틀어줄 우울한 BGM (선택)

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
            isGameStarted = true;

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

        public void AttackBoss(int damage)
        {
            if (isGameOver) return;

            bossHP -= damage;

            if (bossHP < 0) bossHP = 0;

            UpdateHPUI();

            Debug.Log($"💥 보스 데미지: {damage} / 남은 체력: {bossHP}");

            if (bossHP <= 0) GameOver(true);
        }

        public void HealPlayer(int healAmount)
        {
            if (isGameOver) return;

            playerHP = Mathf.Min(playerHP + healAmount, playerMaxHP);
            UpdateHPUI();

            Debug.Log($"💊 체력을 {healAmount} 회복! (현재: {playerHP}/{playerMaxHP})");
        }

        public void AddShield(int shieldAmount)
        {
            if (isGameOver) return;

            playerShield += shieldAmount;
            UpdateHPUI();

            Debug.Log($"🛡️ 방어도 +{shieldAmount} (현재: {playerShield})");
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

            Debug.Log("👿 보스의 공격 시작!");

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
                    Debug.Log($"🛡️ 방패로 막음! (남은 방어도: {playerShield})");
                }
                else
                {
                    incomingDamage -= playerShield;
                    playerShield = 0;
                    Debug.Log("🛡️ 방패가 깨짐!");
                }
            }

            if (incomingDamage > 0)
            {
                playerHP -= incomingDamage;
                Debug.Log($"⚔️ 명치 타격! {incomingDamage} 데미지! (남은 체력: {playerHP})");
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
                Debug.Log("💖 추가 패턴: 보스 회복 +20!");
            }
            else if (dice <= 50)
            {
                if (EarthQuakeEffect != null) EarthQuakeEffect.SetTrigger("EarthQuake");
                if (sfxPlayer != null && patternSounds.Length > 1) sfxPlayer.PlayOneShot(patternSounds[1]);
                yield return new WaitForSeconds(1.0f);
                if (PuzzleManager.Instance != null) PuzzleManager.Instance.ShuffleBoard();
                Debug.Log("🌍 추가 패턴: 지진 발생! 퍼즐 조각이 뒤죽박죽 섞입니다!");
            }
            else if (dice <= 80)
            {
                if (stoneEffect != null) stoneEffect.SetTrigger("Stone");
                if (sfxPlayer != null && patternSounds.Length > 2) sfxPlayer.PlayOneShot(patternSounds[2]);
                yield return new WaitForSeconds(1.0f);
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(8);
                Debug.Log("🪨 추가 패턴: 바위 투척!");
            }
            else
            {
                if (breakEffect != null) breakEffect.SetTrigger("Break");
                if (sfxPlayer != null && patternSounds.Length > 3) sfxPlayer.PlayOneShot(patternSounds[3]);
                yield return new WaitForSeconds(1.0f);
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
                Debug.Log("🔨 추가 패턴: 방패 부수기!");
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

            // 👇 1. 일단 현재 흐르던 보스전 BGM을 멈춥니다!
            if (bgmPlayer != null)
            {
                bgmPlayer.Stop();
                bgmPlayer.loop = false; // 👈 🚨 핵심! 테이프 갈아끼우기 전에 무한 반복 스위치를 강제로 꺼버립니다!
            }

            if (isWin)
            {
                Debug.Log("🎉 Devill 클리어! 스테이지 씬으로 이동합니다.");

                // 2. 클리어 전용 BGM으로 테이프를 갈아 끼우고 재생합니다!
                if (bgmPlayer != null && clearBGM != null)
                {
                    bgmPlayer.clip = clearBGM;
                    bgmPlayer.Play();
                }

                if (!isClearMoving) StartCoroutine(ClearMoveRoutine());
            }
            else
            {
                Debug.Log("💀 용사 파티 전멸... 게임 오버!");

                // 3. 패배 전용 BGM으로 갈아 끼우고 재생합니다!
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
            Debug.Log("✅ Devill 클리어 저장 완료!");

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

        //void OnGUI()
        //{
        //    GUILayout.BeginArea(new Rect(20, 20, 150, 400));

        //    GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        //    titleStyle.fontSize = 15;
        //    titleStyle.normal.textColor = Color.white;

        //    GUILayout.Label("🛠️ 테스트 메뉴", titleStyle);

        //    if (GUILayout.Button("🎉 게임 클리어", GUILayout.Height(40)))
        //    {
        //        GameOver(true);
        //        Debug.Log("🎉 테스트: 게임 클리어 강제 실행!");
        //    }

        //    if (GUILayout.Button("💀 게임 오버", GUILayout.Height(40)))
        //    {
        //        GameOver(false);
        //        Debug.Log("💀 테스트: 게임 오버 강제 실행!");
        //    }

        //    if (GUILayout.Button("🔨 방패 부수기", GUILayout.Height(40)))
        //    {
        //        if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
        //    }

        //    if (GUILayout.Button("🪨 돌멩이 소환", GUILayout.Height(40)))
        //    {
        //        if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(6);
        //    }

        //    if (GUILayout.Button("🌍 지진 발생", GUILayout.Height(40)))
        //    {
        //        if (EarthQuakeEffect != null) EarthQuakeEffect.SetTrigger("EarthQuake");
        //        if (PuzzleManager.Instance != null) PuzzleManager.Instance.ShuffleBoard();
        //    }

        //    if (GUILayout.Button("⚔️ 보스 공격 (랜덤)", GUILayout.Height(40)))
        //    {
        //        turnCount = 2;
        //        NextTurn();
        //    }

        //    GUILayout.EndArea();
        //}
    }
}