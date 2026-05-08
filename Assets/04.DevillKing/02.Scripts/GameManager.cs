using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DH
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public bool isGameStarted = false;
        public bool isGameOver = false;

        // 👇 유고수가 추가한 부분 1: 보스의 체력!
        [Header("보스 설정")]
        public int bossHP = 100; // 보스 체력을 100으로 시작할게요!

        void Awake() { Instance = this; }

        void Start()
        {
            isGameStarted = true;
        }

        void Update()
        {
            if (!isGameStarted || isGameOver) return;
        }

        // 👇 유고수가 추가한 부분 2: 보스가 맞는 기능!
        public void AttackBoss(int damage)
        {
            // 보스 체력에서 데미지(부순 개수)만큼 뺍니다! (-)
            bossHP = bossHP - damage;
            Debug.Log("💥 보스를 때렸습니다! 남은 체력: " + bossHP);

            // 보스 체력이 0이나 그보다 작아지면 승리!
            if (bossHP <= 0)
            {
                GameOver(true);
            }
        }

        public void CheckWinCondition()
        {
            // 예전의 게이지 승리 조건은 지우고 텅 비워둘게요! 
            // 이제 위쪽의 AttackBoss에서 보스가 죽었는지 검사할 거예요.
        }

        public void GameOver(bool isWin)
        {
            isGameOver = true;

            if (isWin)
            {
                Debug.Log("🎉 보스 처치 성공! 게임 클리어!");
            }
            else
            {
                Debug.Log("💀 내 체력이 다 닳았습니다.. 게임 오버!");
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}