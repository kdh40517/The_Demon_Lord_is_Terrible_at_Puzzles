using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    // 컷씬이 끝난 뒤, 검은 화면에서 후속 문구를 타이핑처럼 보여주는 컨트롤러입니다.
    public class AfterCutsceneIntroController : MonoBehaviour
    {
        [Header("후속 장면 UI")]
        [SerializeField] private GameObject afterOverlay;      // 검은 배경 오버레이
        [SerializeField] private TMP_Text afterText;           // 타이핑될 문구가 표시되는 텍스트
        [SerializeField] private GameObject nextGuideText;     // "스페이스바를 눌러 넘어가기!" 안내 텍스트

        [Header("후속 장면 문구")]
        [TextArea(2, 5)]
        [SerializeField] private string[] afterMessages;       // 순서대로 출력할 문장들

        [Header("타이핑 속도")]
        [SerializeField] private float typingSpeed = 0.05f;    // 글자 하나가 출력되는 간격

        [Header("타이핑 효과음")]
        [SerializeField] private AudioSource typingAudioSource; // 타이핑 효과음을 재생할 AudioSource
        [SerializeField] private AudioClip typingClip;          // 타이핑 효과음 파일
        [SerializeField] private int soundEveryCharacters = 3;  // 몇 글자마다 효과음을 낼지
        [SerializeField] private float typingSoundVolume = 0.25f; // 타이핑 효과음 볼륨

        [Header("다음 씬")]
        [SerializeField] private string nextSceneName = "99_LoadingScene"; // 후속 장면이 끝난 뒤 이동할 씬 이름

        private int currentMessageIndex;      // 현재 몇 번째 문장을 보여주는지
        private bool isTyping;                // 현재 타이핑 중인지
        private bool canProceed;              // 다음 문장으로 넘어갈 수 있는지
        private Coroutine typingCoroutine;    // 현재 실행 중인 타이핑 코루틴

        private void OnEnable()
        {
            // 이 오브젝트가 켜질 때마다 첫 문장부터 다시 시작합니다.
            currentMessageIndex = 0;

            if (afterOverlay != null)
            {
                afterOverlay.SetActive(true);
            }

            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }

            ReplaceDynamicMessages();
            StartCurrentMessage();
        }

        private void Update()
        {
            // 후속 장면은 스페이스바로만 진행합니다.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnSpacePressed();
            }
        }

        private void OnSpacePressed()
        {
            // 타이핑 중에 스페이스바를 누르면 현재 문장을 즉시 완성합니다.
            if (isTyping)
            {
                CompleteCurrentMessage();
                return;
            }

            // 아직 다음으로 넘어갈 수 없는 상태면 무시합니다.
            if (!canProceed)
            {
                return;
            }

            currentMessageIndex++;

            // 모든 문장이 끝나면 다음 씬으로 이동합니다.
            if (currentMessageIndex >= afterMessages.Length)
            {
                EndAfterCutscene();
                return;
            }

            StartCurrentMessage();
        }

        private void ReplaceDynamicMessages()
        {
            // {PLAYER}라고 적힌 부분을 타이틀에서 생성한 닉네임으로 바꿉니다.
            string playerName = PlayerPrefs.GetString("PlayerName", "용사");

            if (afterMessages == null)
            {
                return;
            }

            for (int i = 0; i < afterMessages.Length; i++)
            {
                afterMessages[i] = afterMessages[i].Replace("{PLAYER}", playerName);
            }
        }

        private void StartCurrentMessage()
        {
            // 출력할 문장이 없으면 아무것도 하지 않습니다.
            if (afterMessages == null || afterMessages.Length == 0)
            {
                return;
            }

            // 새 문장을 타이핑하는 동안 안내문은 숨깁니다.
            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeMessage(afterMessages[currentMessageIndex]));
        }

        private IEnumerator TypeMessage(string message)
        {
            // 문장을 한 글자씩 출력합니다.
            isTyping = true;
            canProceed = false;

            afterText.text = string.Empty;

            int soundCounter = 0;

            for (int i = 0; i < message.Length; i++)
            {
                afterText.text += message[i];

                // 공백이 아닌 글자 기준으로 일정 간격마다 타이핑 소리를 냅니다.
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
            // 타이핑 효과음이 연결되어 있으면 재생합니다.
            if (typingAudioSource == null || typingClip == null)
            {
                return;
            }

            typingAudioSource.PlayOneShot(typingClip, typingSoundVolume);
        }

        private void CompleteCurrentMessage()
        {
            // 타이핑 중인 문장을 즉시 끝까지 보여줍니다.
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            afterText.text = afterMessages[currentMessageIndex];
            isTyping = false;
            canProceed = true;

            ShowNextGuideText();
        }

        private void ShowNextGuideText()
        {
            // 문장이 끝났을 때 스페이스바 안내문을 보여줍니다.
            if (nextGuideText != null)
            {
                nextGuideText.SetActive(true);
            }
        }

        private void EndAfterCutscene()
        {
            // 후속 장면을 닫고 다음 씬으로 이동합니다.
            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }

            if (afterOverlay != null)
            {
                afterOverlay.SetActive(false);
            }

            SceneManager.LoadScene(nextSceneName);
        }
    }
}