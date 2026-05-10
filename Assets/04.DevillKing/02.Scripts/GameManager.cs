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
        public int playerHP = 20;
        public int playerMaxHP = 20; // ★ 최대 체력 20 제한!
        public int playerShield = 0; // ★ 유고수 추가: 현재 내가 쌓아둔 방어도!

        [Header("턴제 시스템")]
        public int turnCount = 0;
        public int poisonTurnsLeft = 0;

        void Awake() { Instance = this; }

        void Start()
        {
            isGameStarted = true;
            UpdateHPUI();
        }

        public void AttackBoss(int damage)
        {
            bossHP -= damage;
            if (bossHP < 0) bossHP = 0;
            UpdateHPUI();

            Debug.Log($"💥 보스를 때렸습니다! 데미지: {damage} / 남은 체력: {bossHP}");
            if (bossHP <= 0) GameOver(true);
        }

        // ★ 유고수 추가: 갑옷(Armor)을 이었을 때 내 체력을 회복합니다!
        public void HealPlayer(int healAmount)
        {
            playerHP += healAmount;
            if (playerHP > playerMaxHP) playerHP = playerMaxHP; // 최대 체력을 넘지 않게!
            UpdateHPUI();
            Debug.Log($"💊 갑옷으로 체력을 {healAmount} 회복했습니다! (현재 체력: {playerHP})");
        }

        // ★ 유고수 추가: 방패(Shield)를 이었을 때 방어도를 쌓습니다!
        public void AddShield(int shieldAmount)
        {
            playerShield += shieldAmount;
            Debug.Log($"🛡️ 방패를 세웠습니다! 방어도 +{shieldAmount} (현재 방어도: {playerShield})");
        }

        // 독 데미지 (방어도를 무시하고 직접 피를 깎음!)
        public void TakePoisonDamage(int damage)
        {
            playerHP -= damage;
            if (playerHP < 0) playerHP = 0;
            UpdateHPUI();

            Debug.Log($"🤢 독 노트를 만져서 체력이 {damage} 깎였습니다! (남은 체력: {playerHP})");
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
            Debug.Log("👿 보스의 공격 턴!");

            // ★ 유고수 수정: 보스의 기본 10 데미지를 방패로 먼저 막습니다!
            int incomingDamage = 10;

            if (playerShield > 0)
            {
                if (playerShield >= incomingDamage)
                {
                    // 방어도가 충분해서 데미지를 전부 막았을 때!
                    playerShield -= incomingDamage;
                    incomingDamage = 0;
                    Debug.Log($"🛡️ 깡! 방패로 보스의 공격을 완벽하게 막아냈습니다! (남은 방어도: {playerShield})");
                }
                else
                {
                    // 방어도가 모자라서 방패가 부서지고 남은 데미지가 들어올 때!
                    incomingDamage -= playerShield;
                    Debug.Log($"🛡️ 쩌적.. 방패가 파괴되면서 {playerShield}의 데미지를 막았습니다.");
                    playerShield = 0;
                }
            }

            // 방패로 못 막은 데미지만 내 체력에서 깎기
            if (incomingDamage > 0)
            {
                playerHP -= incomingDamage;
                Debug.Log($"⚔️ 앗! 보스에게 {incomingDamage}의 데미지를 입었습니다! (남은 체력: {playerHP})");
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
                Debug.Log("💖 [추가 패턴] 보스가 데미지를 주면서 동시에 체력을 20 회복합니다!");
            }
            else if (dice <= 40)
            {
                poisonTurnsLeft = 3;
                Debug.Log("☠️ [추가 패턴] 보스가 공격하며 독 구름까지 뿌렸습니다!");
            }
            else if (dice <= 70)
            {
                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(8);
                Debug.Log("🪨 [추가 패턴] 보스가 공격과 함께 바위를 떨어뜨립니다!");
            }
            else
            {
                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
                Debug.Log("🔨 [방패 부수기] 보스가 보드판의 방어구들을 고철로 만들었습니다!");
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
            if (isWin) Debug.Log("🎉 승리!");
            else Debug.Log("💀 패배...");
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}