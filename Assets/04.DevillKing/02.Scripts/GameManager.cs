using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

        [Header("보스 & 플레이어 설정")]
        public int bossHP = 100;
        public int bossMaxHP = 100;

        // ★ 유고수 수정: 최대 체력을 20으로 명시합니다.
        [SerializeField] private int playerMaxHP = 20;
        public int playerHP = 20;
        public int playerShield = 0;

        [Header("턴제 시스템")]
        public int turnCount = 0;
        public int poisonTurnsLeft = 0;

        void Awake() { Instance = this; }

        void Start()
        {
            isGameStarted = true;
            InitializeUI(); // ★ UI 초기화 함수 호출
        }

        // ★ 유고수 추가: 시작할 때 슬라이더의 최대값을 내 피(20)에 맞춥니다.
        void InitializeUI()
        {
            if (bossHPBar != null)
            {
                bossHPBar.maxValue = bossMaxHP;
                bossHPBar.value = bossHP;
            }
            if (playerHPBar != null)
            {
                playerHPBar.maxValue = playerMaxHP; // ★ 슬라이더 MAX를 20으로!
                playerHPBar.value = playerHP;
            }
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

        // ★ 유고수 수정: 더 튼튼한 회복 로직 (최대 20까지만 찹니다!)
        public void HealPlayer(int healAmount)
        {
            if (isGameOver) return;

            // Mathf.Min을 써서 (현재피 + 회복량)과 (최대피) 중 작은 쪽을 택합니다.
            playerHP = Mathf.Min(playerHP + healAmount, playerMaxHP);

            UpdateHPUI();
            Debug.Log($"💊 체력을 {healAmount} 회복! (현재: {playerHP}/{playerMaxHP})");
        }

        public void AddShield(int shieldAmount)
        {
            if (isGameOver) return;
            playerShield += shieldAmount;
            Debug.Log($"🛡️ 방어도 +{shieldAmount} (현재: {playerShield})");
        }

        public void TakePoisonDamage(int damage)
        {
            if (isGameOver) return;
            playerHP -= damage;
            if (playerHP < 0) playerHP = 0;
            UpdateHPUI();

            Debug.Log($"🤢 독 데미지 {damage}! (남은 체력: {playerHP})");
            if (playerHP <= 0) GameOver(false);
        }

        public void NextTurn()
        {
            if (isGameOver) return;
            turnCount++;
            if (poisonTurnsLeft > 0) poisonTurnsLeft--;

            if (turnCount % 3 == 0)
            {
                BossAttack();
            }
        }

        void BossAttack()
        {
            if (isGameOver) return;
            Debug.Log("👿 보스의 공격!");

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
                return;
            }

            int dice = Random.Range(1, 101);

            if (dice <= 20)
            {
                bossHP += 20;
                if (bossHP > bossMaxHP) bossHP = bossMaxHP;
                Debug.Log("💖 추가 패턴: 보스 회복 +20!");
            }
            else if (dice <= 40)
            {
                poisonTurnsLeft = 3;
                Debug.Log("☠️ 추가 패턴: 독 살포!");
            }
            else if (dice <= 70)
            {
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(8);
                Debug.Log("🪨 추가 패턴: 바위 투척!");
            }
            else
            {
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
                Debug.Log("🔨 추가 패턴: 방패 부수기!");
            }

            UpdateHPUI();
        }

        void UpdateHPUI()
        {
            if (bossHPBar != null) bossHPBar.value = bossHP;
            if (playerHPBar != null) playerHPBar.value = playerHP;
        }

        public void GameOver(bool isWin)
        {
            isGameOver = true;
            isGameStarted = false;
            if (isWin) Debug.Log("🎉 마왕성 정복 성공! 공주님 진정 완료!");
            else Debug.Log("💀 용사 파티 전멸...");
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        void OnGUI()
        {
            // 화면 왼쪽 위에 가로 150, 세로 300 크기의 메뉴판을 엽니다.
            GUILayout.BeginArea(new Rect(20, 20, 150, 300));

            // 메뉴 제목
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = Color.white;
            GUILayout.Label("🛠️ 테스트 메뉴", titleStyle);

            // 1. 방패 부수기 버튼
            if (GUILayout.Button("🔨 방패 부수기", GUILayout.Height(40)))
            {
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
                Debug.Log("[디버그] 강제로 방패를 부쉈습니다!");
            }

            // 2. 돌멩이 투척 버튼
            if (GUILayout.Button("🪨 돌멩이 소환", GUILayout.Height(40)))
            {
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(6);
                Debug.Log("[디버그] 강제로 돌멩이를 소환했습니다!");
            }

            // 3. 독 패턴 ON 버튼
            if (GUILayout.Button("☠️ 독 패턴 켜기", GUILayout.Height(40)))
            {
                poisonTurnsLeft = 3;
                Debug.Log("[디버그] 독 패턴이 켜졌습니다! (빈칸이 생기면 독이 떨어집니다)");
            }

            // 4. 강제 보스 공격 버튼
            if (GUILayout.Button("⚔️ 보스 공격 (랜덤)", GUILayout.Height(40)))
            {
                turnCount = 2; // 턴을 2로 조작하고
                NextTurn();    // 턴을 넘기면 무조건 3의 배수가 되어 보스가 공격합니다!
            }

            GUILayout.EndArea();
        }
    }
}