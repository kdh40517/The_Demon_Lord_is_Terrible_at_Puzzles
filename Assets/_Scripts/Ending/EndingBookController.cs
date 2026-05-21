using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeoAhn
{
    public class EndingBookController : MonoBehaviour
    {
        [Header("엔딩 인트로 UI")]
        [SerializeField] private GameObject endingIntroPanel;
        [SerializeField] private TMP_Text endingIntroText;

        [Header("엔딩 인트로 문구")]
        [TextArea(2, 5)]
        [SerializeField] private string[] endingIntroMessages;
        [SerializeField] private float typingSpeed = 0.05f;

        [Header("플레이어 이름 설정")]
        [SerializeField] private string playerNameKey = "PlayerName";
        [SerializeField] private string defaultPlayerName = "Player";

        [Header("엔딩 페이지 이미지들")]
        [SerializeField] private Sprite[] endingPages;

        [Header("페이지 UI")]
        [SerializeField] private Image currentPageImage;
        [SerializeField] private Image nextPageImage;
        [SerializeField] private CanvasGroup currentPageCanvasGroup;
        [SerializeField] private CanvasGroup nextPageCanvasGroup;

        [Header("마지막 이미지")]
        [SerializeField] private CanvasGroup finalImageCanvasGroup;
        [SerializeField] private RectTransform finalImageRect;
        [SerializeField] private float finalImageMoveDistance = 450f;
        [SerializeField] private float finalImageMoveDuration = 6.5f;

        [Header("마지막 텍스트 연출")]
        [SerializeField] private CanvasGroup finalTextOverlayCanvasGroup;
        [SerializeField] private TMP_Text finalCenterText;
        [SerializeField] private string finalCenterMessage = "귀여운아이네.. \n어차피 곧 만나게 될거야..";
        [SerializeField] private float finalTextFadeDuration = 1f;
        [SerializeField] private float finalOverlayTargetAlpha = 0.7058824f;

        [Header("검은 화면 페이드")]
        [SerializeField] private CanvasGroup blackFadeCanvasGroup;
        [SerializeField] private float introFadeDuration = 0.35f;
        [SerializeField] private float blackFadeDuration = 1f;
        [SerializeField] private float blackHoldTime = 0.5f;

        [Header("BGM 정지 설정")]
        [SerializeField] private float bgmStopFadeAlpha = 1.0f;

        [Header("안내 텍스트")]
        [SerializeField] private GameObject guideText;

        [Header("슬라이드 + 페이드 설정")]
        [SerializeField] private float transitionDuration = 0.8f;
        [SerializeField] private float screenWidth = 1920f;

        [Header("효과음")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip pageFlipClip;
        [SerializeField] private AudioClip typingClip;
        [SerializeField] private AudioClip laughClip;
        [SerializeField] private float pageFlipVolume = 0.8f;
        [SerializeField] private float typingVolume = 0.5f;
        [SerializeField] private float laughVolume = 1f;

        [Header("BGM")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioClip endingBgmClip;
        [SerializeField] private AudioClip finalBgmClip;
        [SerializeField] private bool loopBgm = true;
        [SerializeField] private float finalBgmVolume = 1f;

        [Header("다음 씬")]
        [SerializeField] private string titleSceneName = "01_TitleScene";

        [Header("엔딩 크레딧")]
        [SerializeField] private EndingCreditController endingCreditController;

        private int currentPageIndex;
        private int currentIntroIndex;

        private bool isTransitioning;
        private bool isIntroPlaying;
        private bool isTyping;
        private bool canProceedIntro;
        private bool isFinalImageShowing;
        private bool isFinalTextShowing;
        private bool isFinalFadePlaying;
        private bool hasStoppedBgmOnFinalFade;
        private bool isCreditStarted;

        private RectTransform currentPageRect;
        private RectTransform nextPageRect;
        private Vector2 finalImageOriginalPosition;

        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (currentPageImage != null && currentPageCanvasGroup == null)
            {
                currentPageCanvasGroup = currentPageImage.GetComponent<CanvasGroup>();

                if (currentPageCanvasGroup == null)
                {
                    currentPageCanvasGroup = currentPageImage.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (nextPageImage != null && nextPageCanvasGroup == null)
            {
                nextPageCanvasGroup = nextPageImage.GetComponent<CanvasGroup>();

                if (nextPageCanvasGroup == null)
                {
                    nextPageCanvasGroup = nextPageImage.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (finalImageCanvasGroup != null && finalImageRect == null)
            {
                finalImageRect = finalImageCanvasGroup.GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            currentPageIndex = 0;
            currentIntroIndex = 0;

            isTransitioning = false;
            isIntroPlaying = true;
            isTyping = false;
            canProceedIntro = false;
            isFinalImageShowing = false;
            isFinalTextShowing = false;
            isFinalFadePlaying = false;
            hasStoppedBgmOnFinalFade = false;
            isCreditStarted = false;

            if (endingPages == null || endingPages.Length == 0)
            {
                Debug.LogWarning("Ending Pages가 비어있습니다.");
                return;
            }

            currentPageRect = currentPageImage.GetComponent<RectTransform>();
            nextPageRect = nextPageImage.GetComponent<RectTransform>();

            if (finalImageRect != null)
            {
                finalImageOriginalPosition = finalImageRect.anchoredPosition;
            }

            currentPageImage.sprite = endingPages[currentPageIndex];
            currentPageImage.gameObject.SetActive(false);
            currentPageRect.anchoredPosition = Vector2.zero;

            if (currentPageCanvasGroup != null)
            {
                currentPageCanvasGroup.alpha = 0f;
            }

            nextPageImage.gameObject.SetActive(false);
            nextPageRect.anchoredPosition = new Vector2(screenWidth, 0f);

            if (nextPageCanvasGroup != null)
            {
                nextPageCanvasGroup.alpha = 0f;
            }

            if (finalImageCanvasGroup != null)
            {
                finalImageCanvasGroup.alpha = 0f;
                finalImageCanvasGroup.gameObject.SetActive(false);
            }

            if (finalTextOverlayCanvasGroup != null)
            {
                finalTextOverlayCanvasGroup.alpha = 0f;
                finalTextOverlayCanvasGroup.gameObject.SetActive(false);
            }

            if (finalCenterText != null)
            {
                finalCenterText.text = finalCenterMessage;

                Color textColor = finalCenterText.color;
                textColor.a = 0f;
                finalCenterText.color = textColor;

                finalCenterText.gameObject.SetActive(false);
            }

            if (blackFadeCanvasGroup != null)
            {
                blackFadeCanvasGroup.gameObject.SetActive(true);
                blackFadeCanvasGroup.alpha = 0f;

                Image blackImage = blackFadeCanvasGroup.GetComponent<Image>();

                if (blackImage != null)
                {
                    Color color = blackImage.color;
                    color.a = 1f;
                    blackImage.color = color;
                }
            }

            if (bgmAudioSource != null)
            {
                bgmAudioSource.Stop();
            }

            if (guideText != null)
            {
                guideText.SetActive(true);
            }

            if (endingIntroPanel != null)
            {
                endingIntroPanel.SetActive(true);
            }

            StartCurrentIntroMessage();
        }

        private void Update()
        {
            if (isCreditStarted)
            {
                return;
            }

            if (!Input.GetKeyDown(KeyCode.Space) && !Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (isIntroPlaying)
            {
                OnIntroProceedInput();
                return;
            }

            if (isFinalImageShowing)
            {
                if (!isFinalTextShowing)
                {
                    StartCoroutine(ShowFinalTextRoutine());
                    return;
                }

                StartEndingCredits();
                return;
            }

            if (!isTransitioning)
            {
                GoNextPage();
            }
        }

        private void StartCurrentIntroMessage()
        {
            if (endingIntroMessages == null || endingIntroMessages.Length == 0)
            {
                EndIntroAndStartPages();
                return;
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            string message = ReplacePlayerName(endingIntroMessages[currentIntroIndex]);
            typingCoroutine = StartCoroutine(TypeIntroMessage(message));
        }

        private IEnumerator TypeIntroMessage(string message)
        {
            isTyping = true;
            canProceedIntro = false;

            if (endingIntroText != null)
            {
                endingIntroText.text = string.Empty;
            }

            for (int i = 0; i < message.Length; i++)
            {
                if (endingIntroText != null)
                {
                    endingIntroText.text += message[i];
                }

                if (!char.IsWhiteSpace(message[i]))
                {
                    PlayTypingSound();
                }

                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            canProceedIntro = true;
        }

        private void OnIntroProceedInput()
        {
            if (isTyping)
            {
                CompleteCurrentIntroMessage();
                return;
            }

            if (!canProceedIntro)
            {
                return;
            }

            currentIntroIndex++;

            if (currentIntroIndex >= endingIntroMessages.Length)
            {
                EndIntroAndStartPages();
                return;
            }

            StartCurrentIntroMessage();
        }

        private void CompleteCurrentIntroMessage()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (endingIntroText != null)
            {
                endingIntroText.text = ReplacePlayerName(endingIntroMessages[currentIntroIndex]);
            }

            isTyping = false;
            canProceedIntro = true;
        }

        private string ReplacePlayerName(string message)
        {
            string playerName = PlayerPrefs.GetString(playerNameKey, defaultPlayerName);

            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = defaultPlayerName;
            }

            return message.Replace("{PLAYER_NAME}", playerName);
        }

        private void EndIntroAndStartPages()
        {
            StartCoroutine(EndIntroAndStartPagesRoutine());
        }

        private IEnumerator EndIntroAndStartPagesRoutine()
        {
            isIntroPlaying = false;
            isTransitioning = true;

            if (guideText != null)
            {
                guideText.SetActive(false);
            }

            if (blackFadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeBlack(0f, 1f, introFadeDuration));
            }

            if (endingIntroPanel != null)
            {
                endingIntroPanel.SetActive(false);
            }

            currentPageImage.gameObject.SetActive(true);
            currentPageRect.anchoredPosition = Vector2.zero;

            if (currentPageCanvasGroup != null)
            {
                currentPageCanvasGroup.alpha = 1f;
            }

            PlayEndingBGM();

            if (blackFadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeBlack(1f, 0f, introFadeDuration));
            }

            if (guideText != null)
            {
                guideText.SetActive(true);
                guideText.transform.SetAsLastSibling();
            }

            isTransitioning = false;
        }

        private void GoNextPage()
        {
            int nextIndex = currentPageIndex + 1;

            if (nextIndex >= endingPages.Length)
            {
                StartCoroutine(ShowFinalImageWithBlackFadeRoutine());
                return;
            }

            StartCoroutine(TransitionPageRoutine(nextIndex));
        }

        private IEnumerator TransitionPageRoutine(int nextIndex)
        {
            isTransitioning = true;

            if (guideText != null)
            {
                guideText.SetActive(false);
            }

            PlayPageFlipSound();

            nextPageImage.sprite = endingPages[nextIndex];
            nextPageImage.gameObject.SetActive(true);

            currentPageRect.anchoredPosition = Vector2.zero;
            nextPageRect.anchoredPosition = new Vector2(screenWidth, 0f);

            if (currentPageCanvasGroup != null)
            {
                currentPageCanvasGroup.alpha = 1f;
            }

            if (nextPageCanvasGroup != null)
            {
                nextPageCanvasGroup.alpha = 0f;
            }

            float timer = 0f;

            while (timer < transitionDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / transitionDuration);
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                currentPageRect.anchoredPosition = Vector2.Lerp(
                    Vector2.zero,
                    new Vector2(-screenWidth, 0f),
                    smoothProgress
                );

                nextPageRect.anchoredPosition = Vector2.Lerp(
                    new Vector2(screenWidth, 0f),
                    Vector2.zero,
                    smoothProgress
                );

                if (currentPageCanvasGroup != null)
                {
                    currentPageCanvasGroup.alpha = Mathf.Lerp(1f, 0f, smoothProgress);
                }

                if (nextPageCanvasGroup != null)
                {
                    nextPageCanvasGroup.alpha = Mathf.Lerp(0f, 1f, smoothProgress);
                }

                yield return null;
            }

            FinishPageTransition(nextIndex);
        }

        private void FinishPageTransition(int nextIndex)
        {
            currentPageIndex = nextIndex;

            currentPageImage.sprite = endingPages[currentPageIndex];
            currentPageRect.anchoredPosition = Vector2.zero;

            if (currentPageCanvasGroup != null)
            {
                currentPageCanvasGroup.alpha = 1f;
            }

            nextPageImage.gameObject.SetActive(false);
            nextPageRect.anchoredPosition = new Vector2(screenWidth, 0f);

            if (nextPageCanvasGroup != null)
            {
                nextPageCanvasGroup.alpha = 0f;
            }

            if (guideText != null)
            {
                guideText.SetActive(true);
                guideText.transform.SetAsLastSibling();
            }

            isTransitioning = false;
        }

        private IEnumerator ShowFinalImageWithBlackFadeRoutine()
        {
            isTransitioning = true;
            isFinalFadePlaying = true;
            hasStoppedBgmOnFinalFade = false;

            if (guideText != null)
            {
                guideText.SetActive(false);
            }

            if (finalImageCanvasGroup == null)
            {
                StartEndingCredits();
                yield break;
            }

            if (blackFadeCanvasGroup != null)
            {
                blackFadeCanvasGroup.gameObject.SetActive(true);
                blackFadeCanvasGroup.transform.SetAsLastSibling();
                blackFadeCanvasGroup.alpha = 0f;

                Image blackImage = blackFadeCanvasGroup.GetComponent<Image>();

                if (blackImage != null)
                {
                    Color color = blackImage.color;
                    color.a = 1f;
                    blackImage.color = color;
                }

                yield return StartCoroutine(FadeBlack(0f, 1f, blackFadeDuration));

                PlayLaughSound();

                yield return new WaitForSeconds(blackHoldTime);
            }

            finalImageCanvasGroup.gameObject.SetActive(true);
            finalImageCanvasGroup.alpha = 1f;
            finalImageCanvasGroup.transform.SetAsLastSibling();

            if (finalImageRect != null)
            {
                finalImageRect.anchoredPosition = new Vector2(
                    finalImageOriginalPosition.x,
                    finalImageOriginalPosition.y + finalImageMoveDistance
                );
            }

            if (blackFadeCanvasGroup != null)
            {
                blackFadeCanvasGroup.transform.SetAsLastSibling();
            }

            if (currentPageImage != null)
            {
                currentPageImage.gameObject.SetActive(false);
            }

            if (nextPageImage != null)
            {
                nextPageImage.gameObject.SetActive(false);
            }

            PlayFinalBGM();

            Coroutine moveCoroutine = null;

            if (finalImageRect != null)
            {
                moveCoroutine = StartCoroutine(MoveFinalImageUpRoutine());
            }

            if (blackFadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeBlack(1f, 0f, blackFadeDuration));
            }

            isFinalFadePlaying = false;

            if (moveCoroutine != null)
            {
                yield return moveCoroutine;
            }

            if (guideText != null)
            {
                guideText.SetActive(true);
                guideText.transform.SetAsLastSibling();
            }

            isFinalImageShowing = true;
            isTransitioning = false;
        }

        private IEnumerator MoveFinalImageUpRoutine()
        {
            if (finalImageRect == null)
            {
                yield break;
            }

            Vector2 startPosition = new Vector2(
                finalImageOriginalPosition.x,
                finalImageOriginalPosition.y + finalImageMoveDistance
            );

            Vector2 endPosition = finalImageOriginalPosition;

            float timer = 0f;

            while (timer < finalImageMoveDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / finalImageMoveDuration);
                float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

                finalImageRect.anchoredPosition = Vector2.Lerp(
                    startPosition,
                    endPosition,
                    smoothProgress
                );

                yield return null;
            }

            finalImageRect.anchoredPosition = endPosition;
        }

        private IEnumerator ShowFinalTextRoutine()
        {
            isTransitioning = true;

            if (guideText != null)
            {
                guideText.SetActive(false);
            }

            if (finalTextOverlayCanvasGroup != null)
            {
                finalTextOverlayCanvasGroup.gameObject.SetActive(true);
                finalTextOverlayCanvasGroup.transform.SetAsLastSibling();
                finalTextOverlayCanvasGroup.alpha = 0f;
            }

            if (finalCenterText != null)
            {
                finalCenterText.gameObject.SetActive(true);
                finalCenterText.text = finalCenterMessage;
                finalCenterText.transform.SetAsLastSibling();

                Color textColor = finalCenterText.color;
                textColor.a = 0f;
                finalCenterText.color = textColor;
            }

            float timer = 0f;

            while (timer < finalTextFadeDuration)
            {
                timer += Time.deltaTime;

                float progress = Mathf.Clamp01(timer / finalTextFadeDuration);

                if (finalTextOverlayCanvasGroup != null)
                {
                    finalTextOverlayCanvasGroup.alpha = Mathf.Lerp(
                        0f,
                        finalOverlayTargetAlpha,
                        progress
                    );
                }

                if (finalCenterText != null)
                {
                    Color textColor = finalCenterText.color;
                    textColor.a = Mathf.Lerp(0f, 1f, progress);
                    finalCenterText.color = textColor;
                }

                yield return null;
            }

            if (finalTextOverlayCanvasGroup != null)
            {
                finalTextOverlayCanvasGroup.alpha = finalOverlayTargetAlpha;
            }

            if (finalCenterText != null)
            {
                Color textColor = finalCenterText.color;
                textColor.a = 1f;
                finalCenterText.color = textColor;
            }

            isFinalTextShowing = true;
            isTransitioning = false;
        }

        private void StartEndingCredits()
        {
            if (isCreditStarted)
            {
                return;
            }

            isCreditStarted = true;
            isTransitioning = true;
            isFinalImageShowing = false;
            isFinalTextShowing = false;

            if (guideText != null)
            {
                guideText.SetActive(false);
            }

            if (endingCreditController != null)
            {
                endingCreditController.PlayCredits();
            }
            else
            {
                Debug.LogWarning("EndingCreditController가 연결되지 않았습니다. 타이틀 씬으로 이동합니다.");
                SceneManager.LoadScene(titleSceneName);
            }
        }

        private IEnumerator FadeBlack(float startAlpha, float endAlpha, float duration)
        {
            if (blackFadeCanvasGroup == null)
            {
                yield break;
            }

            blackFadeCanvasGroup.gameObject.SetActive(true);
            blackFadeCanvasGroup.transform.SetAsLastSibling();

            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
                blackFadeCanvasGroup.alpha = alpha;

                if (isFinalFadePlaying &&
                    !hasStoppedBgmOnFinalFade &&
                    startAlpha < endAlpha &&
                    alpha >= bgmStopFadeAlpha)
                {
                    StopEndingBGM();
                    hasStoppedBgmOnFinalFade = true;
                }

                yield return null;
            }

            blackFadeCanvasGroup.alpha = endAlpha;
        }

        private void PlayPageFlipSound()
        {
            PlaySFX(pageFlipClip, pageFlipVolume);
        }

        private void PlayTypingSound()
        {
            PlaySFX(typingClip, typingVolume);
        }

        private void PlayLaughSound()
        {
            PlaySFX(laughClip, laughVolume);
        }

        private void PlayEndingBGM()
        {
            if (bgmAudioSource == null)
            {
                Debug.LogWarning("BGM AudioSource가 연결되지 않았습니다.");
                return;
            }

            if (endingBgmClip == null)
            {
                Debug.LogWarning("Ending Bgm Clip이 연결되지 않았습니다.");
                return;
            }

            bgmAudioSource.clip = endingBgmClip;
            bgmAudioSource.loop = loopBgm;
            bgmAudioSource.volume = AudioVolumeData.MasterVolume * AudioVolumeData.BGMVolume;
            bgmAudioSource.Play();
        }

        private void PlayFinalBGM()
        {
            if (bgmAudioSource == null)
            {
                return;
            }

            if (finalBgmClip == null)
            {
                Debug.LogWarning("Final Bgm Clip이 연결되지 않았습니다.");
                return;
            }

            bgmAudioSource.Stop();
            bgmAudioSource.clip = finalBgmClip;
            bgmAudioSource.loop = loopBgm;
            bgmAudioSource.volume =
                finalBgmVolume *
                AudioVolumeData.MasterVolume *
                AudioVolumeData.BGMVolume;

            bgmAudioSource.Play();
        }

        private void StopEndingBGM()
        {
            if (bgmAudioSource == null)
            {
                return;
            }

            bgmAudioSource.Stop();
        }

        private void PlaySFX(AudioClip clip, float volume)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(
                clip,
                volume * AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume
            );
        }
    }
}