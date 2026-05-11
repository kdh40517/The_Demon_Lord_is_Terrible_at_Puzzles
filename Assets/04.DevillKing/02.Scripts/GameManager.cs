using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [Header("보스 & 플레이어 설정")]
        public int bossHP = 100;
        public int bossMaxHP = 100;

        [Header("애니메이션 설정")]
        public Animator bossAnimator;

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

            // ★ 유고수가 빼먹었던 바로 그 코드! (방패 먹을 때 즉시 화면 갱신)
            UpdateHPUI();

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
                StartCoroutine(BossAttackRoutine());
            }
        }

        System.Collections.IEnumerator BossAttackRoutine()
        {
            if (isGameOver) yield break;
            Debug.Log("👿 보스의 공격 시작!");

            // 1. 애니메이션 먼저 뽝! 틀어줍니다.
            if (bossAnimator != null) bossAnimator.SetTrigger("Attack");

            // ★ 핵심! 애니메이션이 재생될 동안 1초간 코드를 멈추고 기다립니다.
            yield return new WaitForSeconds(1.0f);

            // 1초 뒤에 실제로 데미지 계산을 시작합니다.
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
                yield break; // 여기서 죽었으면 함수를 바로 끝냅니다.
            }

            UpdateHPUI(); // 맞았으니 체력바 한번 깎아주고!

            // 2. 추가 패턴(주사위) 연출 시작
            int dice = Random.Range(1, 101);

            if (dice <= 20)
            {
                if (bossAnimator != null) bossAnimator.SetTrigger("Heal");
                yield return new WaitForSeconds(1.0f); // 힐 애니메이션 끝날 때까지 1초 대기

                bossHP += 20;
                if (bossHP > bossMaxHP) bossHP = bossMaxHP;
                Debug.Log("💖 추가 패턴: 보스 회복 +20!");
            }
            else if (dice <= 40)
            {
                if (bossAnimator != null) bossAnimator.SetTrigger("Poison");
                yield return new WaitForSeconds(1.0f); // 독 애니메이션 1초 대기

                poisonTurnsLeft = 3;
                Debug.Log("☠️ 추가 패턴: 독 살포!");
            }
            else if (dice <= 70)
            {
                if (bossAnimator != null) bossAnimator.SetTrigger("Stone");
                yield return new WaitForSeconds(1.0f); // 바위 애니메이션 1초 대기

                if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(8);
                Debug.Log("🪨 추가 패턴: 바위 투척!");
            }
            else
            {
                if (bossAnimator != null) bossAnimator.SetTrigger("Break");
                yield return new WaitForSeconds(1.0f); // 방패 깨는 애니메이션 1초 대기

                if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
                Debug.Log("🔨 추가 패턴: 방패 부수기!");
            }

            // 모든 연출이 끝나면 마지막으로 UI 한번 더 갱신!
            UpdateHPUI();
        }

        void UpdateHPUI()
        {
            if (bossHPBar != null) bossHPBar.value = bossHP;
            if (playerHPBar != null) playerHPBar.value = playerHP;
            if (shieldText != null)
            {
                shieldText.text = playerShield.ToString();
            }
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
        //void OnGUI()
        //{
        //    // 화면 왼쪽 위에 가로 150, 세로 300 크기의 메뉴판을 엽니다.
        //    GUILayout.BeginArea(new Rect(20, 20, 150, 300));

        //    // 메뉴 제목
        //    GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        //    titleStyle.fontSize = 15;
        //    titleStyle.normal.textColor = Color.white;
        //    GUILayout.Label("🛠️ 테스트 메뉴", titleStyle);

        //    // 1. 방패 부수기 버튼
        //    if (GUILayout.Button("🔨 방패 부수기", GUILayout.Height(40)))
        //    {
        //        if (BoardManager.Instance != null) BoardManager.Instance.BreakDefenseNotes();
        //        Debug.Log("[디버그] 강제로 방패를 부쉈습니다!");
        //    }

        //    // 2. 돌멩이 투척 버튼
        //    if (GUILayout.Button("🪨 돌멩이 소환", GUILayout.Height(40)))
        //    {
        //        if (BoardManager.Instance != null) BoardManager.Instance.SpawnStones(6);
        //        Debug.Log("[디버그] 강제로 돌멩이를 소환했습니다!");
        //    }

        //    // 3. 독 패턴 ON 버튼
        //    if (GUILayout.Button("☠️ 독 패턴 켜기", GUILayout.Height(40)))
        //    {
        //        poisonTurnsLeft = 3;
        //        Debug.Log("[디버그] 독 패턴이 켜졌습니다! (빈칸이 생기면 독이 떨어집니다)");
        //    }

        //    // 4. 강제 보스 공격 버튼
        //    if (GUILayout.Button("⚔️ 보스 공격 (랜덤)", GUILayout.Height(40)))
        //    {
        //        turnCount = 2; // 턴을 2로 조작하고
        //        NextTurn();    // 턴을 넘기면 무조건 3의 배수가 되어 보스가 공격합니다!
        //    }

        //    GUILayout.EndArea();
        //}
    }
}