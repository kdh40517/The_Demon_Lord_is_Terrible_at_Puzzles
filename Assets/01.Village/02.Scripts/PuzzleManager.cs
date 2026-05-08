using UnityEngine;

namespace TM
{
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes;

        // 4x3 보드
        private Pipe[,] grid = new Pipe[4, 3];

        private bool isCleared = false;

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            // 배열 검사
            if (allPipes == null || allPipes.Length < 12)
            {
                Debug.LogError("allPipes 배열에 Pipe 12개를 넣어주세요!");
                return;
            }

            // 1차원 배열 → 2차원 배열 변환
            int index = 0;

            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (allPipes[index] == null)
                    {
                        Debug.LogError($"allPipes[{index}] 가 비어있습니다!");
                        return;
                    }

                    grid[x, y] = allPipes[index];
                    index++;
                }
            }

            ShufflePipes();

            CheckWaterFlow();
        }

        // 파이프 랜덤 회전
        public void ShufflePipes()
        {
            Debug.Log("파이프 셔플 시작");

            foreach (Pipe pipe in allPipes)
            {
                if (pipe == null)
                    continue;

                int randomRotationCount = Random.Range(0, 4);

                for (int i = 0; i < randomRotationCount; i++)
                {
                    pipe.RotatePipe();
                }
            }
        }

        // 물 흐름 검사
        public void CheckWaterFlow()
        {
            // 전체 초기화
            foreach (Pipe pipe in allPipes)
            {
                if (pipe != null)
                {
                    pipe.SetWater(false);
                }
            }

            // 시작 지점 검사
            Pipe startPipe = grid[0, 1];

            if (startPipe == null)
            {
                Debug.LogError("시작 파이프가 없습니다!");
                return;
            }

            // 서쪽이 열려 있으면 시작
            if (startPipe.isOpened[3])
            {
                DFS_CheckConnection(0, 1);
            }

            // 클리어 체크
            Pipe goalPipe = grid[3, 1];

            if (
                goalPipe != null &&
                goalPipe.hasWater &&
                goalPipe.isOpened[1]
            )
            {
                if (!isCleared)
                {
                    isCleared = true;

                    Debug.Log("클리어! 문이 열렸습니다!");
                }
            }
            else
            {
                isCleared = false;
            }
        }

        // DFS 탐색
        private void DFS_CheckConnection(int x, int y)
        {
            // 범위 검사
            if (x < 0 || x >= 4 || y < 0 || y >= 3)
                return;

            Pipe currentPipe = grid[x, y];

            // null 검사
            if (currentPipe == null)
                return;

            // 이미 방문한 경우
            if (currentPipe.hasWater)
                return;

            // 현재 파이프 물 채우기
            currentPipe.SetWater(true);

            // 북
            if (
                currentPipe.isOpened[0] &&
                y > 0 &&
                grid[x, y - 1] != null &&
                grid[x, y - 1].isOpened[2]
            )
            {
                DFS_CheckConnection(x, y - 1);
            }

            // 동
            if (
                currentPipe.isOpened[1] &&
                x < 3 &&
                grid[x + 1, y] != null &&
                grid[x + 1, y].isOpened[3]
            )
            {
                DFS_CheckConnection(x + 1, y);
            }

            // 남
            if (
                currentPipe.isOpened[2] &&
                y < 2 &&
                grid[x, y + 1] != null &&
                grid[x, y + 1].isOpened[0]
            )
            {
                DFS_CheckConnection(x, y + 1);
            }

            // 서
            if (
                currentPipe.isOpened[3] &&
                x > 0 &&
                grid[x - 1, y] != null &&
                grid[x - 1, y].isOpened[1]
            )
            {
                DFS_CheckConnection(x - 1, y);
            }
        }
    }
}