using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TM
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [Header("UI 설정")]
        public CanvasGroup clearUIGroup; // 활짝 열린 문(실루엣) 이미지가 있는 CanvasGroup

        [Header("타이밍 설정")]
        public float clearDelay = 1.0f;     // 클리어 직후 대기 시간
        public float fadeDuration = 1.5f;   // 이미지가 서서히 나타나는 시간
        public float autoReturnDelay = 3f;  // 이미지가 완전히 뜬 후 씬 이동 전 대기 시간

        [Header("씬 이동 설정")]
        public string loadingSceneName = "99_LoadingScene";
        public string stageSceneName = "03_StageScene";

        private bool isCleared = false;

        private void Awake()
        {
            // 싱글톤 세팅: 다른 스크립트에서 GameManager.instance 로 바로 접근할 수 있게 합니다.
            if (instance == null)
            {
                instance = this;
            }
        }

        private void Start()
        {
            // 시작할 때 클리어 이미지를 완전히 숨깁니다.
            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 0f;
                clearUIGroup.interactable = false;
                clearUIGroup.blocksRaycasts = false;
            }
        }

        // 퍼즐 클리어 시 외부에서 호출할 함수
        public void TriggerClearSequence()
        {
            if (isCleared) return; // 중복 실행 방지
            isCleared = true;

            if (clearUIGroup != null)
            {
                StartCoroutine(FadeInClearUIAndReturn());
            }
        }

        private IEnumerator FadeInClearUIAndReturn()
        {
            // 1. 퍼즐을 막 푼 직후 약간의 여운을 위해 대기
            yield return new WaitForSeconds(clearDelay);

            // 2. 투명도를 0에서 1로 서서히 올림 (페이드 인 효과)
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                clearUIGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
                yield return null;
            }
            clearUIGroup.alpha = 1f;

            // 3. 문이 활짝 열리고 실루엣이 보이는 상태로 잠시 대기
            yield return new WaitForSeconds(autoReturnDelay);

            // 4. 데이터 저장 및 씬 전환
            SeoAhn.StageClearManager.SetVillageClear();
            SeoAhn.SceneTransitionData.SetNextScene(stageSceneName);
            SceneManager.LoadScene(loadingSceneName);
        }
    }
}