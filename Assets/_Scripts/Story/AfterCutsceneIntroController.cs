using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    public class AfterCutsceneIntroController : MonoBehaviour
    {
        [Header("후속 장면 UI")]
        [SerializeField] private GameObject afterOverlay;
        [SerializeField] private TMP_Text afterText;
        [SerializeField] private GameObject nextGuideText;

        [Header("후속 장면 문구")]
        [TextArea(2, 5)]
        [SerializeField] private string[] afterMessages;

        [Header("타이핑 속도")]
        [SerializeField] private float typingSpeed = 0.05f;

        [Header("타이핑 효과음")]
        [SerializeField] private AudioSource typingAudioSource;
        [SerializeField] private AudioClip typingClip;
        [SerializeField] private int soundEveryCharacters = 2;
        [SerializeField] private float typingSoundVolume = 0.4f;

        [Header("다음 씬")]
        [SerializeField] private string nextSceneName = "99_LoadingScene";

        private int currentMessageIndex;
        private bool isTyping;
        private bool canProceed;
        private Coroutine typingCoroutine;

        private void OnEnable()
        {
            currentMessageIndex = 0;
            isTyping = false;
            canProceed = false;

            if (afterOverlay != null)
                afterOverlay.SetActive(true);

            if (nextGuideText != null)
                nextGuideText.SetActive(false);

            if (afterText != null)
                afterText.text = string.Empty;

            ReplaceDynamicMessages();
            StartCurrentMessage();
        }

        private void Update()
        {
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
                return;

            currentMessageIndex++;

            if (afterMessages == null || currentMessageIndex >= afterMessages.Length)
            {
                EndAfterCutscene();
                return;
            }

            StartCurrentMessage();
        }

        private void ReplaceDynamicMessages()
        {
            string playerName = PlayerPrefs.GetString("PlayerName", "용사");

            if (afterMessages == null)
                return;

            for (int i = 0; i < afterMessages.Length; i++)
            {
                if (!string.IsNullOrEmpty(afterMessages[i]))
                {
                    afterMessages[i] = afterMessages[i].Replace("{PLAYER}", playerName);
                }
            }
        }

        private void StartCurrentMessage()
        {
            if (afterMessages == null || afterMessages.Length == 0)
                return;

            if (afterText == null)
                return;

            if (nextGuideText != null)
                nextGuideText.SetActive(false);

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            typingCoroutine = StartCoroutine(TypeMessage(afterMessages[currentMessageIndex]));
        }

        private IEnumerator TypeMessage(string message)
        {
            isTyping = true;
            canProceed = false;

            afterText.text = string.Empty;

            if (string.IsNullOrEmpty(message))
            {
                isTyping = false;
                canProceed = true;
                ShowNextGuideText();
                yield break;
            }

            int soundCounter = 0;

            for (int i = 0; i < message.Length; i++)
            {
                char currentChar = message[i];
                afterText.text += currentChar;

                if (!char.IsWhiteSpace(currentChar))
                {
                    soundCounter++;

                    if (soundCounter >= Mathf.Max(1, soundEveryCharacters))
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
            if (typingAudioSource == null)
            {
                Debug.LogWarning("[AfterCutsceneIntroController] Typing Audio Source가 연결되지 않았습니다.");
                return;
            }

            if (typingClip == null)
            {
                Debug.LogWarning("[AfterCutsceneIntroController] Typing Clip이 연결되지 않았습니다.");
                return;
            }

            typingAudioSource.mute = false;
            typingAudioSource.loop = false;
            typingAudioSource.volume = 1f;
            typingAudioSource.PlayOneShot(typingClip, typingSoundVolume);
        }

        private void CompleteCurrentMessage()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            if (afterMessages == null || currentMessageIndex >= afterMessages.Length)
                return;

            if (afterText != null)
                afterText.text = afterMessages[currentMessageIndex];

            isTyping = false;
            canProceed = true;

            ShowNextGuideText();
        }

        private void ShowNextGuideText()
        {
            if (nextGuideText != null)
                nextGuideText.SetActive(true);
        }

        private void EndAfterCutscene()
        {
            if (nextGuideText != null)
                nextGuideText.SetActive(false);

            if (afterOverlay != null)
                afterOverlay.SetActive(false);

            SceneManager.LoadScene(nextSceneName);
        }
    }
}