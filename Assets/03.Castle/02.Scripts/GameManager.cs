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
            // Castle 클리어 상태 저장
            // StageSelectController가 이 값을 읽고 Castle 카드에 도장을 찍습니다.
            SeoAhn.StageClearManager.SetCastleClear();

            Debug.Log("✅ Castle 클리어 저장 완료! Stage 씬으로 이동합니다.");
        }
    }
}