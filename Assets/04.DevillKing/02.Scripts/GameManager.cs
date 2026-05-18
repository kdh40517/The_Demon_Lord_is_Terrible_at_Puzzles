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

        [Header("사운드 설정")]
        public AudioSource sfxPlayer;
        public AudioClip bossAttackSound;
        public AudioClip[] patternSounds;

        [Header("오디오 믹서")]
        public AudioMixer mainMixer;

        [Header("UI 연결")]
        public CanvasGroup clearPanel;

        [Header("클리어 후 씬 이동 설정")]
        public string stageSceneName = "03_StageScene";
        public string endingSceneName = "04_EndingScene";
        public string clearStampKey = "DevillKing_Clear";
        public string nextSceneKey = "NextSceneAfterStage";
        public float clearMoveDelay = 1.5f;

        private bool isClearMoving = false;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            isGameStarted = true;
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

            if (bossHP < 0)
            {
                bossHP = 0;
            }

            UpdateHPUI();

            Debug.Log($"💥 보스 데미지: {damage} / 남은 체력: {bossHP}");

            if (bossHP <= 0)
            {
                GameOver(true);
            }
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

            if (turnCount % 3 == 0)
            {
                StartCoroutine(BossAttackRoutine());
            }
        }

        IEnumerator BossAttackRoutine()
        {
            if (isGameOver) yield break;

            Debug.Log("👿 보스의 공격 시작!");

            if (sfxPlayer != null && bossAttackSound != null)
            {
                sfxPlayer.PlayOneShot(bossAttackSound);
            }

            if (attackEffect != null)
            {
                attackEffect.SetTrigger("Attack");
            }

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
                if (healEffect != null)
                {
                    healEffect.SetTrigger("Heal");
                }

                if (sfxPlayer != null && patternSounds.Length > 0)
                {
                    sfxPlayer.PlayOneShot(patternSounds[0]);
                }

                yield return new WaitForSeconds(1.0f);

                bossHP += 20;

                if (bossHP > bossMaxHP)
                {
                    bossHP = bossMaxHP;
                }

                Debug.Log("💖 추가 패턴: 보스 회복 +20!");
            }
            else if (dice <= 40)
            {
                if (EarthQuakeEffect != null)
                {
                    EarthQuakeEffect.SetTrigger("EarthQuake");
                }

                if (sfxPlayer != null && patternSounds.Length > 1)
                {
                    sfxPlayer.PlayOneShot(patternSounds[1]);
                }

                yield return new WaitForSeconds(1.0f);

                if (PuzzleManager.Instance != null)
                {
                    PuzzleManager.Instance.ShuffleBoard();
                }

                Debug.Log("🌍 추가 패턴: 지진 발생! 퍼즐 조각이 뒤죽박죽 섞입니다!");
            }
            else if (dice <= 70)
            {
                if (stoneEffect != null)
                {
                    stoneEffect.SetTrigger("Stone");
                }

                if (sfxPlayer != null && patternSounds.Length > 2)
                {
                    sfxPlayer.PlayOneShot(patternSounds[2]);
                }

                yield return new WaitForSeconds(1.0f);

                if (BoardManager.Instance != null)
                {
                    BoardManager.Instance.SpawnStones(8);
                }

                Debug.Log("🪨 추가 패턴: 바위 투척!");
            }
            else
            {
                if (breakEffect != null)
                {
                    breakEffect.SetTrigger("Break");
                }

                if (sfxPlayer != null && patternSounds.Length > 3)
                {
                    sfxPlayer.PlayOneShot(patternSounds[3]);
                }

                yield return new WaitForSeconds(1.0f);

                if (BoardManager.Instance != null)
                {
                    BoardManager.Instance.BreakDefenseNotes();
                }

                Debug.Log("🔨 추가 패턴: 방패 부수기!");
            }

            UpdateHPUI();
        }

        void UpdateHPUI()
        {
            if (bossHPBar != null)
            {
                bossHPBar.value = bossHP;
            }

            if (playerHPBar != null)
            {
                playerHPBar.value = playerHP;
            }

            if (shieldText != null)
            {
                shieldText.text = playerShield.ToString();
            }

            if (bossTurnText != null)
            {
                int turnsUntilAttack = 3 - (turnCount % 3);
                bossTurnText.text = $"{turnsUntilAttack}";
            }
        }

        public void GameOver(bool isWin)
        {
            if (isGameOver) return;

            isGameOver = true;
            isGameStarted = false;

            if (isWin)
            {
                Debug.Log("🎉 마왕성 정복 성공! 스테이지 씬으로 이동 후 엔딩으로 이동합니다!");

                if (!isClearMoving)
                {
                    StartCoroutine(ClearMoveRoutine());
                }
            }
            else
            {
                Debug.Log("💀 용사 파티 전멸...");
            }
        }

        IEnumerator ClearMoveRoutine()
        {
            isClearMoving = true;

            // 👇 빠져있던 페이드인(서서히 나타나는 효과) 추가!
            if (clearPanel != null)
            {
                clearPanel.gameObject.SetActive(true);
                clearPanel.alpha = 0f;

                float timer = 0f;
                float fadeTime = 3.0f; // 1초 동안 스르륵 나타납니다.

                while (timer < fadeTime)
                {
                    timer += Time.deltaTime;
                    clearPanel.alpha = timer / fadeTime;
                    yield return null;
                }
                clearPanel.alpha = 1f;
            }

            // 클리어 도장 찍고 다음 갈 곳 저장
            PlayerPrefs.SetInt(clearStampKey, 1);
            PlayerPrefs.SetString(nextSceneKey, endingSceneName);
            PlayerPrefs.Save();

            // 그림이 다 나타난 상태로 잠시 대기
            yield return new WaitForSeconds(clearMoveDelay);

            // 진짜로 씬 이동!
            SceneManager.LoadScene(stageSceneName);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnGUI()
        {
            // 메뉴 길이가 길어져서 높이를 300 -> 350으로 살짝 늘렸습니다!
            GUILayout.BeginArea(new Rect(20, 20, 150, 350));
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = Color.white;
            GUILayout.Label("🛠️ 테스트 메뉴", titleStyle);

            // 👇 새로 추가된 게임 클리어 테스트 버튼!
            if (GUILayout.Button("🎉 게임 클리어", GUILayout.Height(40)))
            {
                GameOver(true); // true = 승리(클리어) 상태로 GameOver 함수 실행
                Debug.Log("🎉 테스트: 게임 클리어 강제 실행!");
            }

            if (GUILayout.Button("🔨 방패 부수기", GUILayout.Height(40)))
            {
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
            }
            if (GUILayout.Button("🪨 돌멩이 소환", GUILayout.Height(40)))
            {
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(6);
            }
            if (GUILayout.Button("🌍 지진 발생", GUILayout.Height(40)))
            {
                if (EarthQuakeEffect != null) EarthQuakeEffect.SetTrigger("EarthQuake");
                if (PuzzleManager.Instance != null) PuzzleManager.Instance.ShuffleBoard();
                Debug.Log("🌍 테스트: 지진 발생!");
            }
            if (GUILayout.Button("⚔️ 보스 공격 (랜덤)", GUILayout.Height(40)))
            {
                turnCount = 2;
                NextTurn();
            }
            GUILayout.EndArea();
        }
    }
}