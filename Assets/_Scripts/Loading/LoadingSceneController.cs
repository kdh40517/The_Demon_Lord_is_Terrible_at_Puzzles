using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    // 로딩씬에서 다음 씬을 불러오고,
    // 로딩바, 로딩 텍스트, 페이드 효과를 관리하는 스크립트입니다.
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("로딩바 이미지")]
        [SerializeField] private Image loadingFillImage;

        [Header("로딩 텍스트")]
        [SerializeField] private TMP_Text loadingText;
        [SerializeField] private float dotAnimationSpeed = 0.5f;

        [Header("페이드")]
        [SerializeField] private Image fadeOverlayImage;
        [SerializeField] private float fadeDuration = 1f;

        [Header("로딩 설정")]
        [SerializeField] private float minimumLoadingTime = 5f;
        [SerializeField] private string defaultNextSceneName = "03_StageScene";

        private Coroutine loadingTextCoroutine;

        private void Start()
        {
            if (loadingFillImage != null)
            {
                loadingFillImage.fillAmount = 0f;
            }

            if (fadeOverlayImage != null)
            {
                Color color = fadeOverlayImage.color;
                color.a = 0f;
                fadeOverlayImage.color = color;
            }

            loadingTextCoroutine = StartCoroutine(AnimateLoadingText());

            // SceneTransitionData에 목적지가 있으면 그곳으로 이동하고,
            // 없으면 기본값인 03_StageScene으로 이동합니다.
            string nextSceneName = SceneTransitionData.GetNextScene(defaultNextSceneName);

            // 한 번 사용한 목적지는 바로 비웁니다.
            SceneTransitionData.Clear();

            StartCoroutine(LoadSceneRoutine(nextSceneName));
        }

        private IEnumerator AnimateLoadingText()
        {
            while (true)
            {
                if (loadingText != null) loadingText.text = "로딩중입니다";
                yield return new WaitForSeconds(dotAnimationSpeed);

                if (loadingText != null) loadingText.text = "로딩중입니다.";
                yield return new WaitForSeconds(dotAnimationSpeed);

                if (loadingText != null) loadingText.text = "로딩중입니다..";
                yield return new WaitForSeconds(dotAnimationSpeed);

                if (loadingText != null) loadingText.text = "로딩중입니다...";
                yield return new WaitForSeconds(dotAnimationSpeed);
            }
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            float timer = 0f;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                timer += Time.deltaTime;

                float loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);
                float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);
                float finalProgress = Mathf.Min(loadingProgress, timeProgress);

                if (loadingFillImage != null)
                {
                    loadingFillImage.fillAmount = finalProgress;
                }

                if (operation.progress >= 0.9f && timer >= minimumLoadingTime)
                {
                    if (loadingFillImage != null)
                    {
                        loadingFillImage.fillAmount = 1f;
                    }

                    yield return StartCoroutine(FadeOutToNextScene());

                    if (loadingTextCoroutine != null)
                    {
                        StopCoroutine(loadingTextCoroutine);
                    }

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        private IEnumerator FadeOutToNextScene()
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);

                if (fadeOverlayImage != null)
                {
                    Color color = fadeOverlayImage.color;
                    color.a = alpha;
                    fadeOverlayImage.color = color;
                }

                yield return null;
            }
        }
    }
}