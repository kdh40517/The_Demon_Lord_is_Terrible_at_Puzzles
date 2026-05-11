using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // ★ 씬 관리를 위해 반드시 추가해야 합니다!

namespace TM
{
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager instance;

        [Header("퍼즐 보드에 있는 모든 파이프 타일들")]
        public Pipe[] allPipes;

        private Pipe[,] grid = new Pipe[4, 3];
        public bool isCleared = false;

        [Header("Audio Settings")]
        public AudioSource audioSource;
        public AudioClip clearSound;

        [Header("Effect Settings")]
        public ParticleSystem waterParticle;

        [Header("UI Settings")]
        public CanvasGroup clearUIGroup;

        // ★ 새로 추가된 변수: 클리어 후 페이드인이 시작될 때까지의 대기 시간
        public float clearDelay = 2.5f;

        public float fadeDuration = 1.5f;
        public GameObject exitButton;

        [Header("Scene Settings")]
        // ★ 새로 추가된 변수: 이동할 다음 씬의 이름을 인스펙터에서 설정할 수 있게 합니다.
        public string nextSceneName = "NextSceneName";

        void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (exitButton != null)
            {
                exitButton.SetActive(false);
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

        // ★ 수정된 부분: 씬 이동 로직 추가
        public void OnExitButtonClicked()
        {
            Debug.Log($"나가기 버튼이 클릭되었습니다! {nextSceneName} 씬으로 이동합니다!");
            SceneManager.LoadScene(nextSceneName); // 지정된 이름의 씬을 로드합니다.
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
            foreach (Pipe pipe in allPipes) { if (pipe != null) pipe.SetWater(false); }

            Pipe startPipe = grid[0, 1];
            if (startPipe != null && startPipe.isOpened[3]) { DFS_CheckConnection(0, 1); }

            Pipe goalPipe = grid[3, 1];

            if (goalPipe != null && goalPipe.hasWater && goalPipe.isOpened[1])
            {
                if (!isCleared)
                {
                    isCleared = true;
                    Debug.Log("클리어! 문이 열렸습니다!");

                    if (audioSource != null && clearSound != null) audioSource.PlayOneShot(clearSound);
                    if (waterParticle != null) waterParticle.Play();

                    if (clearUIGroup != null) StartCoroutine(FadeInClearUI());
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

        private IEnumerator FadeInClearUI()
        {
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                clearUIGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }

            clearUIGroup.alpha = 1f;
            clearUIGroup.interactable = true;
            clearUIGroup.blocksRaycasts = true;

            if (exitButton != null)
            {
                exitButton.SetActive(true);
                StartCoroutine(PopUpAnimation(exitButton.transform));
            }
        }

        private IEnumerator PopUpAnimation(Transform target)
        {
            float popDuration = 0.3f;
            float time = 0f;

            target.localScale = Vector3.zero;

            while (time < popDuration)
            {
                time += Time.deltaTime;
                float progress = time / popDuration;
                float scale;

                if (progress < 0.7f)
                {
                    scale = Mathf.Lerp(0f, 1.1f, progress / 0.7f);
                }
                else
                {
                    scale = Mathf.Lerp(1.1f, 1f, (progress - 0.7f) / 0.3f);
                }

                target.localScale = new Vector3(scale, scale, scale);
                yield return null;
            }

            target.localScale = Vector3.one;
        }
    }
}