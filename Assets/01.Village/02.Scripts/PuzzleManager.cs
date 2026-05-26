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

        [Header("Audio Settings (효과음)")]
        public AudioSource audioSource;
        public AudioClip clearSound; // 한 단계 깰 때마다 나는 소리

        // 👇 새롭게 추가된 BGM 교체 시스템!
        [Header("BGM Settings (최종 클리어)")]
        public AudioSource bgmPlayer;    // 평소에 마을 BGM을 틀고 있는 스피커
        public AudioClip finalClearBGM;  // 5단계 모두 깼을 때 나올 최종 BGM

        [Header("Effect Settings")]
        public ParticleSystem waterParticle;

        [Header("UI Settings")]
        public CanvasGroup clearUIGroup;
        public Image stageClearImage;

        [Header("Water Fill UI")]
        [Tooltip("인스펙터에서 5256_0 이미지를 연결해 주세요. (Image Type은 Filled)")]
        public Image fountainWaterImage;

        [Tooltip("물이 부드럽게 차오르는 데 걸리는 시간입니다.")]
        public float waterFillDuration = 1.0f;

        private Coroutine waterFillCoroutine;

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

            if (waterParticle != null)
            {
                waterParticle.Stop();
                waterParticle.Clear();
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            if (fountainWaterImage != null)
            {
                fountainWaterImage.fillAmount = 0f;
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
                    isCleared = true;
                    currentClearCount++;

                    Debug.Log($"퍼즐 클리어! ({currentClearCount}/{maxClearCount})");

                    UpdateFountainWater();

                    if (audioSource != null && clearSound != null) audioSource.PlayOneShot(clearSound);

                    if (waterParticle != null)
                    {
                        waterParticle.Stop();
                        waterParticle.Play();
                    }

                    if (currentClearCount >= maxClearCount)
                    {
                        // 👇 최종 클리어 BGM 재생 로직!
                        if (bgmPlayer != null)
                        {
                            bgmPlayer.Stop();
                            bgmPlayer.loop = false; // 클리어 음악은 무한반복 끄기

                            if (finalClearBGM != null)
                            {
                                bgmPlayer.clip = finalClearBGM;
                                bgmPlayer.Play();
                            }
                        }

                        StartCoroutine(FadeInClearUIAndReturn());
                    }
                    else
                    {
                        StartCoroutine(LoadNextStageDelay());
                    }
                }
            }
            else
            {
                isCleared = false;
            }
        }

        private void UpdateFountainWater()
        {
            if (fountainWaterImage != null)
            {
                float targetFillAmount = (float)currentClearCount / maxClearCount;

                if (waterFillCoroutine != null)
                {
                    StopCoroutine(waterFillCoroutine);
                }

                waterFillCoroutine = StartCoroutine(SmoothFillWater(targetFillAmount));
            }
        }

        private IEnumerator SmoothFillWater(float targetFill)
        {
            float startFill = fountainWaterImage.fillAmount;
            float elapsedTime = 0f;

            while (elapsedTime < waterFillDuration)
            {
                elapsedTime += Time.deltaTime;
                fountainWaterImage.fillAmount = Mathf.Lerp(startFill, targetFill, elapsedTime / waterFillDuration);
                yield return null;
            }

            fountainWaterImage.fillAmount = targetFill;
        }

        private IEnumerator LoadNextStageDelay()
        {
            yield return new WaitForSeconds(nextStageDelay);

            if (waterParticle != null)
            {
                waterParticle.Stop();
                waterParticle.Clear();
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }

            GenerateProceduralBoard();
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