using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SeoAhn
{
    public class StoryIntroController : MonoBehaviour
    {
        [Header("인트로 UI")]
        [SerializeField] private GameObject introOverlay;
        [SerializeField] private Image introOverlayImage;
        [SerializeField] private TMP_Text introText;
        [SerializeField] private GameObject nextGuideText;

        [Header("인트로 문구")]
        [TextArea(2, 5)]
        [SerializeField] private string[] introMessages;

        [Header("타이핑 속도")]
        [SerializeField] private float typingSpeed = 0.05f;

        [Header("타이핑 효과음")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip typingClip;
        [SerializeField] private int soundEveryCharacters = 3;
        [SerializeField] private float typingSoundVolume = 0.25f;

        [Header("페이드 설정")]
        [SerializeField] private float fadeDuration = 1.2f;

        [Header("컷씬 매니저")]
        [SerializeField] private GameObject storyCutsceneManager;

        [Header("컷씬 BGM")]
        [SerializeField] private AudioSource cutsceneBgmSource;

        private int currentMessageIndex;
        private bool isTyping;
        private bool canProceed;
        private bool isFading;
        private Coroutine typingCoroutine;

        private void Start()
        {
            currentMessageIndex = 0;

            if (storyCutsceneManager != null)
            {
                storyCutsceneManager.SetActive(false);
            }

            if (introOverlay != null)
            {
                introOverlay.SetActive(true);
            }

            if (introOverlayImage != null)
            {
                Color color = introOverlayImage.color;
                color.a = 1f;
                introOverlayImage.color = color;
            }

            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }

            if (cutsceneBgmSource != null)
            {
                cutsceneBgmSource.Stop();
            }

            ReplaceDynamicMessages();
            StartCurrentMessage();
        }

        private void Update()
        {
            if (isFading)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSpacePressed();
            }
        }

        private void OnSpacePressed()
        {
            if (isTyping)
            {
                CompleteCurrentMessage();
                return;
            }

            if (!canProceed)
            {
                return;
            }

            currentMessageIndex++;

            if (currentMessageIndex >= introMessages.Length)
            {
                StartCoroutine(FadeOutIntro());
                return;
            }

            StartCurrentMessage();
        }

        private void ReplaceDynamicMessages()
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "용사");

            if (introMessages != null && introMessages.Length > 2)
            {
                introMessages[2] =
                    "그의 이름은 " + playerName + "!!\n" +
                    "퍼즐을 좋아하는 소년이다!!";
            }

            if (introMessages != null && introMessages.Length > 3)
            {
                introMessages[3] =
                    playerName + "는(은) 공주의 낭군이 되기 위해\n" +
                    "마왕앞에 당당히 서게 되는데..";
            }
        }

        private void StartCurrentMessage()
        {
            if (introMessages == null || introMessages.Length == 0)
            {
                return;
            }

            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeMessage(introMessages[currentMessageIndex]));
        }

        private IEnumerator TypeMessage(string message)
        {
            isTyping = true;
            canProceed = false;

            introText.text = string.Empty;

            int soundCounter = 0;

            for (int i = 0; i < message.Length; i++)
            {
                introText.text += message[i];

                if (!char.IsWhiteSpace(message[i]))
                {
                    soundCounter++;

                    if (soundCounter >= soundEveryCharacters)
                    {
                        PlayTypingSound();
                        soundCounter = 0;
                    }
                }

                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            canProceed = true;

            ShowNextGuideText();
        }

        private void PlayTypingSound()
        {
            if (typingAudioSource == null || typingClip == null)
            {
                return;
            }

            typingAudioSource.PlayOneShot(typingClip, typingSoundVolume);
        }

        private void CompleteCurrentMessage()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            introText.text = introMessages[currentMessageIndex];
            isTyping = false;
            canProceed = true;

            ShowNextGuideText();
        }

        private void ShowNextGuideText()
        {
            if (nextGuideText != null)
            {
                nextGuideText.SetActive(true);
            }
        }

        private IEnumerator FadeOutIntro()
        {
            isFading = true;

            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }

            if (introText != null)
            {
                introText.gameObject.SetActive(false);
            }

            if (storyCutsceneManager != null)
            {
                storyCutsceneManager.SetActive(true);
            }

            if (cutsceneBgmSource != null && !cutsceneBgmSource.isPlaying)
            {
                cutsceneBgmSource.Play();
            }

            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

                if (introOverlayImage != null)
                {
                    Color color = introOverlayImage.color;
                    color.a = alpha;
                    introOverlayImage.color = color;
                }

                yield return null;
            }

            if (introOverlay != null)
            {
                introOverlay.SetActive(false);
            }

            isFading = false;
        }
    }
}