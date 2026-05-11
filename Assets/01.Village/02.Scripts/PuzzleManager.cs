using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TM
{
    // 마을 퍼즐의 클리어 판정, 클리어 UI 표시,
    // 클리어 후 자동으로 스테이지 선택 씬으로 복귀하는 스크립트입니다.
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes; // 퍼즐에 배치된 모든 파이프

        private Pipe[,] grid = new Pipe[4, 3]; // 4x3 퍼즐 배열
        public bool isCleared = false; // 클리어 여부

        [Header("Audio Settings")]
        public AudioSource audioSource; // 효과음 재생용 AudioSource
        public AudioClip clearSound; // 클리어 효과음

        [Header("Effect Settings")]
        public ParticleSystem waterParticle; // 클리어 파티클

        [Header("UI Settings")]
        public CanvasGroup clearUIGroup; // 클리어 이미지(CanvasGroup 필요)

        [Header("타이밍 설정")]
        public float clearDelay = 2.5f; // 클리어 후 UI 등장 전 대기
        public float fadeDuration = 1.5f; // 클리어 이미지 페이드 시간
        public float autoReturnDelay = 3f; // 클리어 이미지 완전히 뜬 뒤 자동 복귀 대기

        [Header("Scene Settings")]
        public string loadingSceneName = "99_LoadingScene"; // 로딩씬 이름
        public string stageSceneName = "03_StageScene"; // 돌아갈 스테이지씬 이름

        private void Awake()
        {
            // 다른 스크립트에서 접근할 수 있도록 instance 저장
            instance = this;
        }

        private void Start()
        {
            // 클리어 UI 초기 상태
            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 0f;
                clearUIGroup.interactable = false;
                clearUIGroup.blocksRaycasts = false;
            }

            // 파이프 개수가 부족하면 종료
            if (allPipes == null || allPipes.Length < 12)
            {
                return;
            }

            // 1차원 배열을 2차원 grid로 변환
            int index = 0;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    grid[x, y] = allPipes[index];
                    index++;
                }
            }

            // 퍼즐 섞기
            ShufflePipes();

            // 물길 검사
            CheckWaterFlow();
        }

        public void ShufflePipes()
        {
            // 모든 파이프를 랜덤하게 회전시켜 퍼즐 생성
            foreach (Pipe pipe in allPipes)
            {
                if (pipe == null)
                {
                    continue;
                }

                int randomRotationCount = Random.Range(0, 4);

                for (int i = 0; i < randomRotationCount; i++)
                {
                    pipe.RotatePipe();
                }
            }
        }

        public void CheckWaterFlow()
        {
            // 기존 물 상태 초기화
            foreach (Pipe pipe in allPipes)
            {
                if (pipe != null)
                {
                    pipe.SetWater(false);
                }
            }

            Pipe startPipe = grid[0, 1];

            // 시작 파이프에서 물 탐색 시작
            if (startPipe != null && startPipe.isOpened[3])
            {
                DFS_CheckConnection(0, 1);
            }

            Pipe goalPipe = grid[3, 1];

            // 목표 파이프까지 연결되었는지 검사
            if (goalPipe != null && goalPipe.hasWater && goalPipe.isOpened[1])
            {
                if (!isCleared)
                {
                    isCleared = true;

                    Debug.Log("퍼즐 클리어!");

                    // 효과음 재생
                    if (audioSource != null && clearSound != null)
                    {
                        audioSource.PlayOneShot(clearSound);
                    }

                    // 파티클 재생
                    if (waterParticle != null)
                    {
                        waterParticle.Play();
                    }

                    // 클리어 UI 표시 후 자동 복귀
                    if (clearUIGroup != null)
                    {
                        StartCoroutine(FadeInClearUIAndReturn());
                    }
                }
            }
            else
            {
                isCleared = false;
            }
        }

        private void DFS_CheckConnection(int x, int y)
        {
            if (x < 0 || x >= 4 || y < 0 || y >= 3)
            {
                return;
            }

            Pipe currentPipe = grid[x, y];

            if (currentPipe == null || currentPipe.hasWater)
            {
                return;
            }

            currentPipe.SetWater(true);

            // 위
            if (currentPipe.isOpened[0] &&
                y > 0 &&
                grid[x, y - 1] != null &&
                grid[x, y - 1].isOpened[2])
            {
                DFS_CheckConnection(x, y - 1);
            }

            // 오른쪽
            if (currentPipe.isOpened[1] &&
                x < 3 &&
                grid[x + 1, y] != null &&
                grid[x + 1, y].isOpened[3])
            {
                DFS_CheckConnection(x + 1, y);
            }

            // 아래
            if (currentPipe.isOpened[2] &&
                y < 2 &&
                grid[x, y + 1] != null &&
                grid[x, y + 1].isOpened[0])
            {
                DFS_CheckConnection(x, y + 1);
            }

            // 왼쪽
            if (currentPipe.isOpened[3] &&
                x > 0 &&
                grid[x - 1, y] != null &&
                grid[x - 1, y].isOpened[1])
            {
                DFS_CheckConnection(x - 1, y);
            }
        }

        private IEnumerator FadeInClearUIAndReturn()
        {
            // 클리어 직후 잠깐 대기
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;

            // 클리어 이미지 페이드인
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;

                clearUIGroup.alpha = Mathf.Lerp(
                    0f,
                    1f,
                    elapsedTime / fadeDuration
                );

                yield return null;
            }

            clearUIGroup.alpha = 1f;

            // 클리어 이미지가 완전히 뜬 후 3초 대기
            yield return new WaitForSeconds(autoReturnDelay);

            // Village 클리어 저장
            SeoAhn.StageClearManager.SetVillageClear();

            // 로딩씬 끝나면 StageScene으로 가도록 지정
            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);

            // 로딩씬 이동
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}