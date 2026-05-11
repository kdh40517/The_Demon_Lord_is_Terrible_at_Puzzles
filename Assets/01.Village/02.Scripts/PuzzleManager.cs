using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TM
{
    // 마을 퍼즐의 클리어 판정, 클리어 UI 표시,
    // 클리어 후 자동으로 스테이지 선택 씬으로 복귀하는 스크립트입니다.
    public class PuzzleManager : MonoBehaviour
    {
        // 핵심 1: 언제 어디서든 PuzzleManager.instance 로 쉽게 접근할 수 있게 만드는 싱글톤 패턴입니다.
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes; // 퍼즐에 배치된 모든 파이프

<<<<<<< Updated upstream
        private Pipe[,] grid = new Pipe[4, 3]; // 4x3 퍼즐 배열
        public bool isCleared = false; // 클리어 여부
=======
        // 핵심 2: 일렬로 된 파이프들을 (x, y) 좌표로 쉽게 찾기 위해 4x3 크기의 2차원 표(grid)로 만듭니다.
        private Pipe[,] grid = new Pipe[4, 3];
        public bool isCleared = false;
>>>>>>> Stashed changes

        [Header("Audio Settings")]
        public AudioSource audioSource; // 효과음 재생용 AudioSource
        public AudioClip clearSound; // 클리어 효과음

        [Header("Effect Settings")]
        public ParticleSystem waterParticle; // 클리어 파티클

        [Header("UI Settings")]
<<<<<<< Updated upstream
        public CanvasGroup clearUIGroup; // 클리어 이미지(CanvasGroup 필요)

        [Header("타이밍 설정")]
        public float clearDelay = 2.5f; // 클리어 후 UI 등장 전 대기
        public float fadeDuration = 1.5f; // 클리어 이미지 페이드 시간
        public float autoReturnDelay = 3f; // 클리어 이미지 완전히 뜬 뒤 자동 복귀 대기

        [Header("Scene Settings")]
        public string loadingSceneName = "99_LoadingScene"; // 로딩씬 이름
        public string stageSceneName = "03_StageScene"; // 돌아갈 스테이지씬 이름
=======
        public CanvasGroup clearUIGroup;
        public float clearDelay = 2.5f;
        public float fadeDuration = 1.5f;
        public GameObject exitButton;

        [Header("Scene Settings")]
        public string nextSceneName = "NextSceneName";
>>>>>>> Stashed changes

        private void Awake()
        {
            // 다른 스크립트에서 접근할 수 있도록 instance 저장
            instance = this;
        }

        private void Start()
        {
<<<<<<< Updated upstream
            // 클리어 UI 초기 상태
            if (clearUIGroup != null)
=======
            // 시작할 때 나가기 버튼은 일단 숨겨둡니다.
            if (exitButton != null)
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
            // 1차원 배열을 2차원 grid로 변환
=======
            // 핵심 3: 1차원 배열(allPipes)에 들어있는 파이프들을 4x3 2차원 배열(grid)에 차곡차곡 정리해 넣습니다.
>>>>>>> Stashed changes
            int index = 0;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    grid[x, y] = allPipes[index];
                    index++;
                }
            }

<<<<<<< Updated upstream
            // 퍼즐 섞기
=======
            // 게임이 시작되면 파이프를 무작위로 섞고, 처음 물길이 어떻게 되어있는지 검사합니다.
>>>>>>> Stashed changes
            ShufflePipes();

<<<<<<< Updated upstream
            // 물길 검사
            CheckWaterFlow();
=======
        public void OnExitButtonClicked()
        {
            Debug.Log($"나가기 버튼이 클릭되었습니다! {nextSceneName} 씬으로 이동합니다!");
            SceneManager.LoadScene(nextSceneName);
>>>>>>> Stashed changes
        }

        // 모든 파이프를 0~3번 랜덤하게 회전시켜서 퍼즐을 섞는 함수입니다.
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

        // 핵심 4: 퍼즐의 가장 중요한 로직! 파이프가 돌아갈 때마다 물이 끝까지 통하는지 확인합니다.
        public void CheckWaterFlow()
        {
<<<<<<< Updated upstream
            // 기존 물 상태 초기화
            foreach (Pipe pipe in allPipes)
            {
                if (pipe != null)
                {
                    pipe.SetWater(false);
                }
            }
=======
            // 1. 일단 모든 파이프의 물을 쫙 뺍니다 (초기화)
            foreach (Pipe pipe in allPipes) { if (pipe != null) pipe.SetWater(false); }
>>>>>>> Stashed changes

            // 2. 시작점(왼쪽) 파이프가 왼쪽[3]으로 뚫려있다면, 거기서부터 물을 흘려보냅니다.
            Pipe startPipe = grid[0, 1];

            // 시작 파이프에서 물 탐색 시작
            if (startPipe != null && startPipe.isOpened[3])
            {
                DFS_CheckConnection(0, 1);
            }

            // 3. 도착점(오른쪽) 파이프를 확인합니다.
            Pipe goalPipe = grid[3, 1];

<<<<<<< Updated upstream
            // 목표 파이프까지 연결되었는지 검사
=======
            // 4. 도착점에 물이 도달했고 && 오른쪽[1]으로 뚫려있다면? -> 퍼즐 클리어!
>>>>>>> Stashed changes
            if (goalPipe != null && goalPipe.hasWater && goalPipe.isOpened[1])
            {
                if (!isCleared) // 중복 클리어 방지
                {
                    isCleared = true;

<<<<<<< Updated upstream
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
=======
                    // 팡파레 효과음과 분수 파티클을 재생합니다.
                    if (audioSource != null && clearSound != null) audioSource.PlayOneShot(clearSound);
                    if (waterParticle != null) waterParticle.Play();

                    // 클리어 UI를 스르륵 나타나게 합니다.
                    if (clearUIGroup != null) StartCoroutine(FadeInClearUI());
>>>>>>> Stashed changes
                }
            }
            else
            {
                isCleared = false;
            }
        }

        // 핵심 5: 물이 번져나가는 원리 (깊이 우선 탐색 - DFS)
        // 현재 파이프에서 상하좌우를 살펴보고, 파이프가 서로 이어져 있다면 그쪽으로도 물을 채우는 함수입니다.
        private void DFS_CheckConnection(int x, int y)
        {
<<<<<<< Updated upstream
            if (x < 0 || x >= 4 || y < 0 || y >= 3)
            {
                return;
            }

            Pipe currentPipe = grid[x, y];

            if (currentPipe == null || currentPipe.hasWater)
            {
                return;
            }
=======
            // 보드판 밖으로 나가면 무시합니다.
            if (x < 0 || x >= 4 || y < 0 || y >= 3) return;

            Pipe currentPipe = grid[x, y];
            // 파이프가 없거나 이미 물이 차있으면 돌아갑니다.
            if (currentPipe == null || currentPipe.hasWater) return;
>>>>>>> Stashed changes

            // 현재 파이프에 물을 채웁니다.
            currentPipe.SetWater(true);

<<<<<<< Updated upstream
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
=======
            // 북쪽[0]이 뚫려있고, 윗 파이프의 남쪽[2]이 뚫려있다면 -> 위로 물이 번짐!
            if (currentPipe.isOpened[0] && y > 0 && grid[x, y - 1] != null && grid[x, y - 1].isOpened[2])
                DFS_CheckConnection(x, y - 1);
            // 동쪽[1]이 뚫려있고, 오른쪽 파이프의 서쪽[3]이 뚫려있다면 -> 오른쪽으로 물이 번짐!
            if (currentPipe.isOpened[1] && x < 3 && grid[x + 1, y] != null && grid[x + 1, y].isOpened[3])
                DFS_CheckConnection(x + 1, y);
            // 남쪽[2]이 뚫려있고, 아랫 파이프의 북쪽[0]이 뚫려있다면 -> 아래로 물이 번짐!
            if (currentPipe.isOpened[2] && y < 2 && grid[x, y + 1] != null && grid[x, y + 1].isOpened[0])
                DFS_CheckConnection(x, y + 1);
            // 서쪽[3]이 뚫려있고, 왼쪽 파이프의 동쪽[1]이 뚫려있다면 -> 왼쪽으로 물이 번짐!
            if (currentPipe.isOpened[3] && x > 0 && grid[x - 1, y] != null && grid[x - 1, y].isOpened[1])
>>>>>>> Stashed changes
                DFS_CheckConnection(x - 1, y);
            }
        }

<<<<<<< Updated upstream
        private IEnumerator FadeInClearUIAndReturn()
        {
            // 클리어 직후 잠깐 대기
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;

            // 클리어 이미지 페이드인
=======
        // 핵심 6: 클리어 성공 시 연출을 담당하는 코루틴 (시간의 흐름을 제어합니다)
        private IEnumerator FadeInClearUI()
        {
            // 분수 물줄기가 시원하게 나오는 걸 감상할 수 있게 잠시 대기합니다.
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;
            // 정해진 시간(fadeDuration) 동안 투명도를 0에서 1로 서서히 올립니다. (스르륵 나타나는 효과)
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream

            // 클리어 이미지가 완전히 뜬 후 3초 대기
            yield return new WaitForSeconds(autoReturnDelay);

            // Village 클리어 저장
            SeoAhn.StageClearManager.SetVillageClear();

            // 로딩씬 끝나면 StageScene으로 가도록 지정
            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);

            // 로딩씬 이동
            SceneManager.LoadScene(loadingSceneName);
=======
            clearUIGroup.interactable = true;
            clearUIGroup.blocksRaycasts = true; // 이제 UI를 마우스로 누를 수 있습니다.

            // 마지막으로 '나가기' 버튼을 귀엽게 통! 하고 튕겨 나오게 띄웁니다.
            if (exitButton != null)
            {
                exitButton.SetActive(true);
                StartCoroutine(PopUpAnimation(exitButton.transform));
            }
        }

        // 버튼이 나타날 때 살짝 커졌다가 원래 크기로 돌아오는 '띠용~' 하는 팝업 애니메이션입니다.
        private IEnumerator PopUpAnimation(Transform target)
        {
            float popDuration = 0.3f;
            float time = 0f;

            target.localScale = Vector3.zero; // 안 보이는 상태에서 시작

            while (time < popDuration)
            {
                time += Time.deltaTime;
                float progress = time / popDuration;
                float scale;

                // 70% 시점까지는 원래 크기보다 조금 더 크게(1.1배) 부풀립니다.
                if (progress < 0.7f)
                {
                    scale = Mathf.Lerp(0f, 1.1f, progress / 0.7f);
                }
                // 나머지 30% 시점 동안 원래 크기(1배)로 쏙 돌아옵니다.
                else
                {
                    scale = Mathf.Lerp(1.1f, 1f, (progress - 0.7f) / 0.3f);
                }

                target.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }

            target.localScale = Vector3.one; // 오차 보정을 위해 마지막엔 정확히 1로 맞춰줍니다.
>>>>>>> Stashed changes
        }
    }
}