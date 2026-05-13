using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TM
{
    public class PuzzleManager : MonoBehaviour
    {
        // [핵심 1. 싱글톤 패턴]
        // 게임 내에 퍼즐 매니저는 딱 1개만 존재하므로, 다른 스크립트(예: 파이프 클릭 스크립트)에서
        // GetComponent로 귀찮게 찾을 필요 없이 PuzzleManager.instance 로 바로 접근하기 위해 만듭니다.
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes;

        // [핵심 2. 2차원 배열 (그리드)]
        // 하이어라키에 일렬로 나열된 파이프들을 (x, y) 좌표계로 관리하기 위한 배열입니다.
        // 상하좌우 파이프가 서로 이어져 있는지 수학적으로 계산하려면 2차원 형태가 훨씬 편합니다.
        private Pipe[,] grid = new Pipe[4, 3];
        public bool isCleared = false;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip clearSound;

        [Header("Effect Settings")]
        public ParticleSystem waterParticle;

        [Header("UI Settings")]
        public CanvasGroup clearUIGroup;

        [Header("타이밍 설정")]
        public float clearDelay = 2.5f;
        public float fadeDuration = 1.5f;
        public float autoReturnDelay = 3f;

        [Header("Scene Settings")]
        public string loadingSceneName = "99_LoadingScene";
        public string stageSceneName = "03_StageScene";

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 0f;
                clearUIGroup.interactable = false;
                clearUIGroup.blocksRaycasts = false;
            }

            if (allPipes == null || allPipes.Length < 12) return;

            // 1차원 배열(allPipes)의 데이터를 4x3 크기의 2차원 배열(grid)로 차곡차곡 옮겨 담습니다.
            // 이렇게 하면 grid[0, 0]은 왼쪽 맨 위, grid[3, 2]는 오른쪽 맨 아래 파이프가 됩니다.
            int index = 0;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    grid[x, y] = allPipes[index];
                    index++;
                }
            }

            ShufflePipes();
            CheckWaterFlow();
        }

        public void ShufflePipes()
        {
            foreach (Pipe pipe in allPipes)
            {
                if (pipe == null) continue;

                int randomRotationCount = Random.Range(0, 4);
                for (int i = 0; i < randomRotationCount; i++)
                {
                    pipe.RotatePipe();
                }
            }
        }

        public void CheckWaterFlow()
        {
            // 물길을 새로 계산하기 전에 모든 파이프의 물을 말라있는 상태(false)로 초기화합니다.
            foreach (Pipe pipe in allPipes)
            {
                if (pipe != null) pipe.SetWater(false);
            }

            // 물이 출발하는 시작점 파이프 (왼쪽 가운데)
            Pipe startPipe = grid[0, 1];
            // 시작점 파이프의 왼쪽 구멍(isOpened[3])이 뚫려있어야 물이 들어오기 시작합니다.
            if (startPipe != null && startPipe.isOpened[3])
            {
                // DFS(깊이 우선 탐색) 알고리즘 시작! 여기서부터 물이 연결된 길을 따라 쫙 퍼져나갑니다.
                DFS_CheckConnection(0, 1);
            }

            // 물이 최종적으로 도착해야 하는 목표점 파이프 (오른쪽 가운데)
            Pipe goalPipe = grid[3, 1];
            // 목표 파이프에 물이 도달했고(hasWater), 오른쪽 구멍(isOpened[1])으로 물이 빠져나갈 수 있다면 클리어 판정!
            if (goalPipe != null && goalPipe.hasWater && goalPipe.isOpened[1])
            {
                if (!isCleared)
                {
                    isCleared = true;
                    Debug.Log("퍼즐 클리어!");

                    if (audioSource != null && clearSound != null) audioSource.PlayOneShot(clearSound);
                    if (waterParticle != null) waterParticle.Play();

                    if (clearUIGroup != null)
                    {
                        StartCoroutine(FadeInClearUIAndReturn());
                    }
                }
            }
            else
            {
                isCleared = false; // 연결이 끊어지면 클리어 상태 해제
            }
        }

        // [핵심 3. DFS (깊이 우선 탐색) 알고리즘]
        // 현재 위치(x, y)에서 상/하/좌/우를 살펴보고, 길이 뚫려있으면 그쪽 방향의 파이프로 넘어가서 다시 탐색하는 재귀 함수입니다.
        private void DFS_CheckConnection(int x, int y)
        {
            // 보드판 범위를 벗어나면 에러가 나므로 탐색 중지
            if (x < 0 || x >= 4 || y < 0 || y >= 3) return;

            Pipe currentPipe = grid[x, y];
            // 파이프가 비어있거나, 이미 물이 차있는 파이프면 중복해서 검사할 필요가 없으므로 탐색 중지
            if (currentPipe == null || currentPipe.hasWater) return;

            // 현재 파이프에 물을 채웁니다.
            currentPipe.SetWater(true);

            // 주의: 배열 인덱스 매칭 -> 0: 위, 1: 오른쪽, 2: 아래, 3: 왼쪽

            // [위쪽 방향 검사]
            // 내 파이프 윗부분(0)이 뚫림 && 위쪽 칸이 존재함 && 위쪽 파이프가 존재함 && 위쪽 파이프의 아랫부분(2)이 뚫림
            if (currentPipe.isOpened[0] && y > 0 && grid[x, y - 1] != null && grid[x, y - 1].isOpened[2])
                DFS_CheckConnection(x, y - 1); // 위쪽 파이프로 이동해서 다시 탐색

            // [오른쪽 방향 검사]
            // 내 파이프 오른쪽(1)이 뚫림 && 오른쪽 칸이 존재함 && 오른쪽 파이프가 존재함 && 오른쪽 파이프의 왼쪽(3)이 뚫림
            if (currentPipe.isOpened[1] && x < 3 && grid[x + 1, y] != null && grid[x + 1, y].isOpened[3])
                DFS_CheckConnection(x + 1, y);

            // [아래쪽 방향 검사]
            // 내 파이프 아랫부분(2)이 뚫림 && 아래쪽 칸이 존재함 && 아래쪽 파이프가 존재함 && 아래쪽 파이프의 윗부분(0)이 뚫림
            if (currentPipe.isOpened[2] && y < 2 && grid[x, y + 1] != null && grid[x, y + 1].isOpened[0])
                DFS_CheckConnection(x, y + 1);

            // [왼쪽 방향 검사]
            // 내 파이프 왼쪽(3)이 뚫림 && 왼쪽 칸이 존재함 && 왼쪽 파이프가 존재함 && 왼쪽 파이프의 오른쪽(1)이 뚫림
            if (currentPipe.isOpened[3] && x > 0 && grid[x - 1, y] != null && grid[x - 1, y].isOpened[1])
                DFS_CheckConnection(x - 1, y);
        }

        // [핵심 4. 코루틴을 이용한 시간 제어 및 연출]
        // 클리어 직후 UI를 부드럽게 띄우고, 일정 시간 뒤에 씬을 넘어가는 등 '시간의 흐름'이 필요한 연출을 담당합니다.
        private IEnumerator FadeInClearUIAndReturn()
        {
            // 클리어 이펙트(물 뿜기 등)를 볼 수 있게 잠시 대기
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;
            // fadeDuration 시간 동안 서서히 투명도(alpha)를 0에서 1로 올립니다. (페이드 인 효과)
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                clearUIGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null; // 다음 프레임까지 대기
            }
            clearUIGroup.alpha = 1f; // 확실하게 100% 보이게 고정

            // UI가 완전히 다 뜨고 난 뒤, 플레이어가 상황을 인지할 수 있게 잠시 대기
            yield return new WaitForSeconds(autoReturnDelay);

            // [데이터 저장 및 씬 전환]
            // 외부 스크립트(SeoAhn 패키지/네임스페이스)를 참조하여 현재 스테이지 클리어를 세이브합니다.
            SeoAhn.StageClearManager.SetVillageClear();
            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);

            // 로딩 씬으로 넘어갑니다. (이후 로딩 씬에서 자동으로 stageSceneName으로 이동하게 됩니다)
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}