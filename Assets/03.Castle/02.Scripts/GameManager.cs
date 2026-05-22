using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TM
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [Header("UI 설정")]
        public CanvasGroup clearUIGroup;

        [Header("타이밍 설정")]
        public float clearDelay = 1.0f;
        public float fadeDuration = 1.5f;
        public float autoReturnDelay = 3f;

        [Header("씬 이동 설정")]
        public string loadingSceneName = "99_LoadingScene";
        public string stageSceneName = "03_StageScene";

        [Header("오디오 설정")]
        public AudioSource audioSource;
        public AudioClip clearUIRevealSound;
        [Tooltip("방향키 입력을 정확하게 맞췄을 때 재생할 효과음입니다.")]
        public AudioClip correctInputSound;

        [Header("경고 연출")]
        public CanvasGroup warningPanel;
        private Coroutine warningCoroutine;

        private bool isCleared = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 0f;
                clearUIGroup.interactable = false;
                clearUIGroup.blocksRaycasts = false;
            }
        }

        // 정답을 맞췄을 때 외부에서 호출할 함수
        public void PlayCorrectSound()
        {
            if (audioSource != null && correctInputSound != null)
            {
                // 연속 입력 시 사운드가 겹치더라도 자연스럽게 재생됩니다.
                audioSource.PlayOneShot(correctInputSound);
            }
        }

        public void TriggerClearSequence()
        {
            if (isCleared)
            {
                return;
            }

            isCleared = true;
            StartCoroutine(FadeInClearUIAndReturn());
        }

        private IEnumerator FadeInClearUIAndReturn()
        {
            yield return new WaitForSeconds(clearDelay);

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;

                if (clearUIGroup != null)
                {
                    clearUIGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                }

                yield return null;
            }

            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 1f;
                clearUIGroup.interactable = true;
                clearUIGroup.blocksRaycasts = true;
            }

            if (audioSource != null && clearUIRevealSound != null)
            {
                audioSource.PlayOneShot(clearUIRevealSound);
            }

            yield return new WaitForSeconds(autoReturnDelay);

            SaveCastleClearData();

            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);
            SceneManager.LoadScene(loadingSceneName);
        }

        private void SaveCastleClearData()
        {
            SeoAhn.StageClearManager.SetCastleClear();
            Debug.Log("✅ Castle 클리어 저장 완료! Stage 씬으로 이동합니다.");
        }
    }
}