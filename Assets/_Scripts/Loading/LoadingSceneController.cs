using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace SeoAhn
{
    // 로딩 씬에서 다음 씬을 비동기로 불러오고
    // 로딩바와 로딩 텍스트 애니메이션을 담당하는 스크립트입니다.
    public class LoadingSceneController : MonoBehaviour
    {
        [Header("로딩바 이미지")]
        [SerializeField] private Image loadingFillImage; // 점점 차오르는 로딩 게이지 이미지

        [Header("로딩 텍스트")]
        [SerializeField] private TMP_Text loadingText; // "로딩중입니다..." 텍스트
        [SerializeField] private float dotAnimationSpeed = 0.5f; // 점 애니메이션 속도

        [Header("로딩 설정")]
        [SerializeField] private float minimumLoadingTime = 5f; // 최소 로딩 시간

        private void Start()
        {
            // 처음 로딩바는 비어있는 상태
            if (loadingFillImage != null)
            {
                loadingFillImage.fillAmount = 0f;
            }

            // 점 애니메이션 시작
            StartCoroutine(AnimateLoadingText());

            // 저장된 다음 씬 이름 가져오기
            string nextSceneName = PlayerPrefs.GetString("NextSceneName", "03_StageScene");

            // 로딩 시작
            StartCoroutine(LoadSceneRoutine(nextSceneName));
        }

        private IEnumerator AnimateLoadingText()
        {
            // 로딩 텍스트 점 애니메이션 반복
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

            // 다음 씬 비동기 로딩
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

            // 로딩 완료되어도 바로 넘어가지 않음
            operation.allowSceneActivation = false;

            while (!operation.isDone)
            {
                timer += Time.deltaTime;

                // Unity 로딩 진행률 계산
                float loadingProgress = Mathf.Clamp01(operation.progress / 0.9f);

                // 최소 시간 기준 진행률
                float timeProgress = Mathf.Clamp01(timer / minimumLoadingTime);

                // 둘 중 느린 쪽 기준으로 로딩바 표시
                float finalProgress = Mathf.Min(loadingProgress, timeProgress);

                if (loadingFillImage != null)
                {
                    loadingFillImage.fillAmount = finalProgress;
                }

                // 조건 만족 시 씬 전환
                if (operation.progress >= 0.9f && timer >= minimumLoadingTime)
                {
                    if (loadingFillImage != null)
                    {
                        loadingFillImage.fillAmount = 1f;
                    }

                    operation.allowSceneActivation = true;
                }

                yield return null;
            }
        }
    }
}