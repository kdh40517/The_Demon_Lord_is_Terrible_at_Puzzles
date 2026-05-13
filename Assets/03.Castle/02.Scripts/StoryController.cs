using System.Collections;
using TMPro;
using UnityEngine;

namespace TM
{
    public class StoryController : MonoBehaviour
    {
        [Header("퍼즐 연동")]
        [SerializeField] private DirectionalPuzzle directionalPuzzle;

        [Header("말풍선 UI")]
        [SerializeField] private GameObject speechBubble;
        [SerializeField] private TMP_Text speechText;

        [Header("악마 대사 목록")]
        [TextArea(2, 5)]
        [SerializeField] private string[] devilMessages;

        [Header("클리어 대사")]
        [SerializeField] private string clearMessage = "열렸다...!"; // 클리어 시 띄울 텍스트

        [Header("타이핑 설정")]
        [SerializeField] private float typingSpeed = 0.05f;

        private bool wasDevilWatching = false;
        private bool isDialogueActive = false;
        private bool isCleared = false; // 퍼즐 클리어 상태를 추적하는 플래그
        private Coroutine typingCoroutine;

        private void Start()
        {
            if (speechBubble != null)
            {
                speechBubble.SetActive(false);
            }
        }

        private void Update()
        {
            // 퍼즐이 연결되어 있지 않거나, 이미 클리어된 상태라면 아래 로직(악마 대사 띄우기/숨기기)을 실행하지 않음
            if (directionalPuzzle == null || isCleared) return;

            bool isCurrentlyWatching = directionalPuzzle.isDevilWatching;
            float remainingTime = directionalPuzzle.devilTimer;

            // 1. 악마가 눈을 감고 있고, 눈 뜨기까지 0.5초 이하로 남았을 때 대사 시작
            if (!isCurrentlyWatching && remainingTime <= 0.5f && !isDialogueActive)
            {
                ShowRandomDialogue();
                isDialogueActive = true;
            }

            // 2. 악마가 눈을 감는 순간(상태 변화) 말풍선 제거 및 플래그 리셋
            if (!isCurrentlyWatching && wasDevilWatching)
            {
                HideDialogue();
                isDialogueActive = false;
            }

            wasDevilWatching = isCurrentlyWatching;
        }

        private void ShowRandomDialogue()
        {
            if (devilMessages == null || devilMessages.Length == 0) return;

            if (speechBubble != null)
            {
                speechBubble.SetActive(true);
            }

            string randomMessage = devilMessages[Random.Range(0, devilMessages.Length)];

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeMessage(randomMessage));
        }

        // 📌 퍼즐 클리어 시 호출될 함수 (외부에서 실행)
        public void ShowClearMessage()
        {
            if (isCleared) return;
            isCleared = true; // 일반 악마 대사 로직 정지

            if (speechBubble != null)
            {
                speechBubble.SetActive(true);
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            // 클리어 대사 타이핑 시작
            typingCoroutine = StartCoroutine(TypeMessage(clearMessage));
        }

        private void HideDialogue()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            if (speechText != null)
            {
                speechText.text = string.Empty;
            }

            if (speechBubble != null)
            {
                speechBubble.SetActive(false);
            }
        }

        private IEnumerator TypeMessage(string message)
        {
            if (speechText == null) yield break;

            speechText.text = string.Empty;

            for (int i = 0; i < message.Length; i++)
            {
                speechText.text += message[i];
                yield return new WaitForSeconds(typingSpeed);
            }
        }
    }
}