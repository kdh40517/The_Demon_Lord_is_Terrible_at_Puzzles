using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    // 로딩 씬에서 다음 씬을 비동기로 불러오고,
    // 로딩바, 로딩 텍스트 애니메이션, 씬 전환 전 페이드 효과를 담당합니다.
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("로딩바 이미지")]
        [SerializeField] private Image loadingFillImage; // 점점 차오르는 로딩바 이미지

        [Header("로딩 텍스트")]
        [SerializeField] private TMP_Text loadingText; // "로딩중입니다..." 텍스트
        [SerializeField] private float dotAnimationSpeed = 0.5f; // 점이 하나씩 늘어나는 속도

        [Header("페이드")]
        [SerializeField] private Image fadeOverlayImage; // 씬 전환 전 검게 덮을 이미지
        [SerializeField] private float fadeDuration = 1f; // 페이드 시간

        [Header("로딩 설정")]
        [SerializeField] private float minimumLoadingTime = 5f; // 최소 로딩 시간

        private void Start()
        {
            // 로딩바를 비운 상태로 시작합니다.
            if (loadingFillImage != null)
            {
                loadingFillImage.fillAmount = 0f;
            }

            // 페이드 오버레이는 처음엔 투명하게 둡니다.
            if (fadeOverlayImage != null)
            {
                Color color = fadeOverlayImage.color;
                color.a = 0f;
                fadeOverlayImage.color = color;
            }

            // 로딩 텍스트 점 애니메이션 시작
            StartCoroutine(AnimateLoadingText());

            // 이전 씬에서 저장한 다음 씬 이름을 가져옵니다.
            string nextSceneName = PlayerPrefs.GetString("NextSceneName", "03_StageScene");

            // 다음 씬 로딩 시작
            StartCoroutine(LoadSceneRoutine(nextSceneName));
        }

        private IEnumerator AnimateLoadingText()
        {
            // 로딩중입니다 → 로딩중입니다. → 로딩중입니다.. → 로딩중입니다...
            while (true)
            {
                if (loadingText != null)
                {
                    loadingText.text = "로딩중입니다";
                }

                yield return new WaitForSeconds(dotAnimationSpeed);

                if (loadingText != null)
                {
                    loadingText.text = "로딩중입니다.";
                }

                yield return new WaitForSeconds(dotAnimationSpeed);

                if (loadingText != null)
                {
                    loadingText.text = "로딩중입니다..";
                }

                yield return new WaitForSeconds(dotAnimationSpeed);

                if (loadingText != null)
                {
                    loadingText.text = "로딩중입니다...";
                }

                yield return new WaitForSeconds(dotAnimationSpeed);
            }
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            float timer = 0f;

            // 다음 씬을 비동기로 불러옵니다.
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            // 로딩이 끝나도 바로 씬 전환되지 않도록 막습니다.
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                timer += Time.deltaTime;

                // Unity의 비동기 로딩 progress는 보통 0~0.9까지만 올라갑니다.
                float loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);

                // 최소 로딩 시간에 맞춘 진행률입니다.
                float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);

                // 실제 로딩률과 시간 진행률 중 더 낮은 값을 사용합니다.
                float finalProgress = Mathf.Min(loadingProgress, timeProgress);

                if (loadingFillImage != null)
                {
                    loadingFillImage.fillAmount = finalProgress;
                }

                // 실제 로딩도 끝났고 최소 로딩 시간도 지났다면 페이드 후 씬 전환합니다.
                if (operation.progress >= 0.9f && timer >= minimumLoadingTime)
                {
                    if (loadingFillImage != null)
                    {
                        loadingFillImage.fillAmount = 1f;
                    }

                    yield return StartCoroutine(FadeOutToNextScene());

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }

        private IEnumerator FadeOutToNextScene()
        {
            // 검은 화면이 서서히 나타나도록 Alpha를 0에서 1로 올립니다.
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

            if (fadeOverlayImage != null)
            {
                Color color = fadeOverlayImage.color;
                color.a = 1f;
                fadeOverlayImage.color = color;
            }
        }
    }
}