using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 추가

namespace TM
{
    public class PuzzleManager : MonoBehaviour
    {
        // [핵심 1. 싱글톤 패턴]
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes;

        // [핵심 2. 2차원 배열 (그리드)]
        private Pipe[,] grid = new Pipe[4, 3];
        public bool isCleared = false;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip clearSound;

        [Header("Effect Settings")]
        public ParticleSystem waterParticle;

        [Header("UI Settings")]
        public CanvasGroup clearUIGroup;

        // [추가됨] StageClearImage.png 이미지를 넣을 곳
        [Tooltip("StageClearImage.png 이미지가 들어간 UI Image 컴포넌트를 연결해주세요.")]
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
            // 기존 UI 그룹 투명도 초기화
            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 0f;
                clearUIGroup.interactable = false;
                clearUIGroup.blocksRaycasts = false;
            }

            // [추가됨] 클리어 이미지 투명도 초기화 (시작할 때 안 보이게)
            if (stageClearImage != null)
            {
                Color imgColor = stageClearImage.color;
                imgColor.a = 0f;
                stageClearImage.color = imgColor;
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
                    Debug.Log("퍼즐 클리어!");

                    if (audioSource != null && clearSound != null) audioSource.PlayOneShot(clearSound);
                    if (waterParticle != null) waterParticle.Play();

                    // UI와 이미지를 동시에 띄우는 코루틴 실행
                    StartCoroutine(FadeInClearUIAndReturn());
                }
            }
            else
            {
                isCleared = false;
            }
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

        // [핵심 4. 코루틴을 이용한 시간 제어 및 연출]
        private IEnumerator FadeInClearUIAndReturn()
        {
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;

            // fadeDuration 시간 동안 서서히 투명도(alpha)를 0에서 1로 올립니다.
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);

                // 기존 CanvasGroup 페이드인
                if (clearUIGroup != null)
                {
                    clearUIGroup.alpha = currentAlpha;
                }

                // [추가됨] 클리어 이미지 페이드인
                if (stageClearImage != null)
                {
                    Color imgColor = stageClearImage.color;
                    imgColor.a = currentAlpha;
                    stageClearImage.color = imgColor;
                }

                yield return null;
            }

            // 완전히 다 보이게 값 고정
            if (clearUIGroup != null) clearUIGroup.alpha = 1f;
            if (stageClearImage != null)
            {
                Color finalColor = stageClearImage.color;
                finalColor.a = 1f;
                stageClearImage.color = finalColor;
            }

            yield return new WaitForSeconds(autoReturnDelay);

            // 데이터 저장 및 씬 전환
            SeoAhn.StageClearManager.SetVillageClear();
            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}