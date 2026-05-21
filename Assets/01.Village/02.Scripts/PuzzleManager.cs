using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TM
{
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes;

        [Header("파이프 종류별 스프라이트 등록")]
        public Sprite straightEmpty, straightWater;
        public Sprite L_Empty, L_Water;
        public Sprite T_Empty, T_Water;

        private Pipe[,] grid = new Pipe[4, 3];
        public bool isCleared = false;

        [Header("스테이지 (연속 클리어) 설정")]
        [Tooltip("최종 클리어를 위해 퍼즐을 연속으로 맞춰야 하는 횟수입니다.")]
        public int maxClearCount = 5;
        private int currentClearCount = 0;

        [Tooltip("한 판을 깬 후, 다음 맵으로 섞이기 전까지 대기하는 시간입니다.")]
        public float nextStageDelay = 3.0f;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip clearSound;

        [Header("Effect Settings")]
        public ParticleSystem waterParticle;

        [Header("UI Settings")]
        public CanvasGroup clearUIGroup;
        public Image stageClearImage;

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

            if (stageClearImage != null)
            {
                Color imgColor = stageClearImage.color;
                imgColor.a = 0f;
                stageClearImage.color = imgColor;
            }

            // [수정됨] 게임 시작 시 파티클과 사운드가 켜져있다면 강제로 끕니다.
            if (waterParticle != null)
            {
                waterParticle.Stop();
                waterParticle.Clear();
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            if (allPipes == null || allPipes.Length < 12) return;

            int index = 0;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    grid[x, y] = allPipes[index];
                    index++;
                }
            }

            // 첫 게임 시작 (카운트 0으로 초기화)
            currentClearCount = 0;
            GenerateProceduralBoard();
            CheckWaterFlow();
        }

        public void GenerateProceduralBoard()
        {
            int[,] pathGrid = new int[4, 3];
            List<Vector2Int> path = new List<Vector2Int>();

            int currentX = 0, currentY = 1;
            path.Add(new Vector2Int(currentX, currentY));
            pathGrid[currentX, currentY] = 1;

            // 매운맛 경로 꼬기 (80% 확률로 상하 이동)
            while (currentX < 3)
            {
                List<Vector2Int> verticalMoves = new List<Vector2Int>();

                if (currentY > 0 && pathGrid[currentX, currentY - 1] == 0)
                    verticalMoves.Add(new Vector2Int(currentX, currentY - 1));
                if (currentY < 2 && pathGrid[currentX, currentY + 1] == 0)
                    verticalMoves.Add(new Vector2Int(currentX, currentY + 1));

                Vector2Int next;
                if (verticalMoves.Count > 0 && Random.value < 0.8f)
                    next = verticalMoves[Random.Range(0, verticalMoves.Count)];
                else
                    next = new Vector2Int(currentX + 1, currentY);

                currentX = next.x;
                currentY = next.y;

                path.Add(new Vector2Int(currentX, currentY));
                pathGrid[currentX, currentY] = 1;
            }

            while (currentY != 1)
            {
                currentY += (currentY < 1) ? 1 : -1;
                path.Add(new Vector2Int(currentX, currentY));
                pathGrid[currentX, currentY] = 1;
            }

            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Pipe targetPipe = grid[x, y];
                    bool needN = false, needE = false, needS = false, needW = false;

                    if (pathGrid[x, y] == 1)
                    {
                        int pIndex = path.IndexOf(new Vector2Int(x, y));

                        if (x == 0 && y == 1) needW = true;
                        if (x == 3 && y == 1) needE = true;

                        if (pIndex > 0)
                        {
                            Vector2Int p = path[pIndex - 1];
                            if (p.y < y) needN = true;
                            if (p.x > x) needE = true;
                            if (p.y > y) needS = true;
                            if (p.x < x) needW = true;
                        }
                        if (pIndex < path.Count - 1)
                        {
                            Vector2Int n = path[pIndex + 1];
                            if (n.y < y) needN = true;
                            if (n.x > x) needE = true;
                            if (n.y > y) needS = true;
                            if (n.x < x) needW = true;
                        }

                        AssignPipeShape(targetPipe, needN, needE, needS, needW);
                    }
                    else
                    {
                        // 함정 페이크 타일 배치 (T자, ㄱ자 위주)
                        int rand = Random.Range(0, 10);
                        if (rand < 2) AssignPipeShape(targetPipe, true, false, true, false);
                        else if (rand < 6) AssignPipeShape(targetPipe, true, true, false, false);
                        else AssignPipeShape(targetPipe, true, true, true, false);
                    }

                    int randomRot = Random.Range(0, 4);
                    for (int i = 0; i < randomRot; i++) targetPipe.RotatePipe();
                }
            }
        }

        private void AssignPipeShape(Pipe pipe, bool n, bool e, bool s, bool w)
        {
            int openCount = (n ? 1 : 0) + (e ? 1 : 0) + (s ? 1 : 0) + (w ? 1 : 0);

            // [유지] 원본 이미지 방향에 맞춘 세팅
            if (openCount == 2 && ((n && s) || (e && w)))
                pipe.SetPipeType(true, false, true, false, straightEmpty, straightWater);
            else if (openCount == 2)
                pipe.SetPipeType(false, true, true, false, L_Empty, L_Water);
            else
                pipe.SetPipeType(false, true, true, true, T_Empty, T_Water);

            int maxRotations = 4;
            while (maxRotations > 0)
            {
                if (openCount < 3)
                {
                    if (pipe.isOpened[0] == n && pipe.isOpened[1] == e && pipe.isOpened[2] == s && pipe.isOpened[3] == w) break;
                }
                else
                {
                    if ((!n || pipe.isOpened[0]) && (!e || pipe.isOpened[1]) && (!s || pipe.isOpened[2]) && (!w || pipe.isOpened[3])) break;
                }
                pipe.RotatePipe();
                maxRotations--;
            }
        }

        public void CheckWaterFlow()
        {
            foreach (Pipe pipe in allPipes)
            {
                if (pipe != null) pipe.SetWater(false);
            }

            Pipe startPipe = grid[0, 1];
            if (startPipe != null && startPipe.isOpened[3])
            {
                DFS_CheckConnection(0, 1);
            }

            Pipe goalPipe = grid[3, 1];
            if (goalPipe != null && goalPipe.hasWater && goalPipe.isOpened[1])
            {
                if (!isCleared)
                {
                    isCleared = true; // 터치 방지
                    currentClearCount++; // 클리어 횟수 증가

                    Debug.Log($"퍼즐 클리어! ({currentClearCount}/{maxClearCount})");

                    if (audioSource != null && clearSound != null) audioSource.PlayOneShot(clearSound);

                    // 파티클 실행 (이미 실행 중일 수 있으니 껐다 켭니다)
                    if (waterParticle != null)
                    {
                        waterParticle.Stop();
                        waterParticle.Play();
                    }

                    // 목표 횟수에 도달했는지 확인
                    if (currentClearCount >= maxClearCount)
                    {
                        // 최종 3연속 클리어 달성!
                        StartCoroutine(FadeInClearUIAndReturn());
                    }
                    else
                    {
                        // 아직 남았다면 맵을 섞기 위해 다음 페이즈 코루틴 실행
                        StartCoroutine(LoadNextStageDelay());
                    }
                }
            }
            else
            {
                isCleared = false; // 물길이 끊기면 다시 터치 가능하게 풀어줌
            }
        }

        private IEnumerator LoadNextStageDelay()
        {
            // 플레이어가 성공적으로 이어진 물길과 이펙트를 볼 수 있도록 잠시 대기
            yield return new WaitForSeconds(nextStageDelay);

            // [수정된 부분] 맵이 새로 섞이기 직전에 물 파티클과 사운드를 끄고 화면에서 완전히 지워줍니다.
            if (waterParticle != null)
            {
                waterParticle.Stop();
                waterParticle.Clear();
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            // 맵 완전히 새로 짜기
            GenerateProceduralBoard();

            // 파이프들의 물 상태를 다시 초기화 (이 과정에서 isCleared가 다시 false로 풀림)
            CheckWaterFlow();
        }

        private void DFS_CheckConnection(int x, int y)
        {
            if (x < 0 || x >= 4 || y < 0 || y >= 3) return;

            Pipe currentPipe = grid[x, y];
            if (currentPipe == null || currentPipe.hasWater) return;

            currentPipe.SetWater(true);

            if (currentPipe.isOpened[0] && y > 0 && grid[x, y - 1] != null && grid[x, y - 1].isOpened[2])
                DFS_CheckConnection(x, y - 1);

            if (currentPipe.isOpened[1] && x < 3 && grid[x + 1, y] != null && grid[x + 1, y].isOpened[3])
                DFS_CheckConnection(x + 1, y);

            if (currentPipe.isOpened[2] && y < 2 && grid[x, y + 1] != null && grid[x, y + 1].isOpened[0])
                DFS_CheckConnection(x, y + 1);

            if (currentPipe.isOpened[3] && x > 0 && grid[x - 1, y] != null && grid[x - 1, y].isOpened[1])
                DFS_CheckConnection(x - 1, y);
        }

        private IEnumerator FadeInClearUIAndReturn()
        {
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

                if (clearUIGroup != null) clearUIGroup.alpha = currentAlpha;
                if (stageClearImage != null)
                {
                    Color imgColor = stageClearImage.color;
                    imgColor.a = currentAlpha;
                    stageClearImage.color = imgColor;
                }

                yield return null;
            }

            if (clearUIGroup != null) clearUIGroup.alpha = 1f;
            if (stageClearImage != null)
            {
                Color finalColor = stageClearImage.color;
                finalColor.a = 1f;
                stageClearImage.color = finalColor;
            }

            yield return new WaitForSeconds(autoReturnDelay);

            SeoAhn.StageClearManager.SetVillageClear();
            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}