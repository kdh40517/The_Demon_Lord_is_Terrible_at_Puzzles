using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    public class EndingCreditController : MonoBehaviour
    {
        [Header("크레딧 전체 오브젝트")]
        [SerializeField] private GameObject creditRoot;

        [Header("크레딧 텍스트")]
        [SerializeField] private RectTransform creditTextRect;
        [SerializeField] private TMP_Text creditText;

        [Header("자동 시작 설정")]
        [SerializeField] private bool playOnStart = false;
        [SerializeField] private float startDelay = 0f;

        [Header("크레딧 이동 설정")]
        [SerializeField] private float startY = -700f;
        [SerializeField] private float endY = 1200f;
        [SerializeField] private float scrollDuration = 20f;

        [Header("엔딩 후 이동 설정")]
        [SerializeField] private bool moveSceneAfterCredits = true;
        [SerializeField] private string nextSceneName = "00_TitleScene";
        [SerializeField] private float waitAfterCredits = 3f;

        private bool isPlaying = false;

        private void Start()
        {
            if (creditRoot != null)
            {
                creditRoot.SetActive(false);
            }

            if (playOnStart)
            {
                StartCoroutine(PlayOnStartRoutine());
            }
        }

        private IEnumerator PlayOnStartRoutine()
        {
            yield return new WaitForSeconds(startDelay);
            PlayCredits();
        }

        public void PlayCredits()
        {
            if (isPlaying)
            {
                return;
            }

            isPlaying = true;

            if (creditRoot != null)
            {
                creditRoot.SetActive(true);
            }

            SetupCreditText();
            StartCoroutine(PlayCreditRoutine());
        }

        private void SetupCreditText()
        {
            if (creditTextRect != null)
            {
                creditTextRect.anchoredPosition = new Vector2(creditTextRect.anchoredPosition.x, startY);
            }

            if (creditText != null)
            {
                creditText.alignment = TextAlignmentOptions.Center;
                creditText.text =
                    "마왕님은 퍼즐 젬병!\n\n" +
                    "ENDING CREDIT\n\n\n" +
                    "기획\n" +
                    "최서안\n\n" +
                    "오디오\n" +
                    "권동환\n\n" +
                    "아트 / UI\n" +
                    "정태민\n\n" +
                    "VillageScene + CastleScene\n" +
                    "정태민\n\n" +
                    "TitleScene + ForestScene\n" +
                    "최서안\n\n" +
                    "DevilScene + EndingScene\n" +
                    "권동환\n\n" +
                    "그 외 Scene\n" +
                    "Team.Oi\n\n" +
                    "플레이해주셔서 감사합니다!\n\n\n" +
                    "THE END";
            }
        }

        private IEnumerator PlayCreditRoutine()
        {
            float timer = 0f;

            while (timer < scrollDuration)
            {
                timer += Time.deltaTime;

                float y = Mathf.Lerp(startY, endY, timer / scrollDuration);

                if (creditTextRect != null)
                {
                    creditTextRect.anchoredPosition = new Vector2(creditTextRect.anchoredPosition.x, y);
                }

                yield return null;
            }

            yield return new WaitForSeconds(waitAfterCredits);

            if (moveSceneAfterCredits)
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }
}