using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    public class StorySceneController : MonoBehaviour
    {
        [Header("스토리 텍스트")]
        [SerializeField] private TMP_Text storyText;

        [Header("스토리 내용")]
        [TextArea(2, 5)]
        [SerializeField] private string[] storyLines;

        [Header("타이핑 설정")]
        [SerializeField] private float typingSpeed = 0.05f;

        [Header("다음 씬")]
        [SerializeField] private string nextSceneName = "03_GameScene";

        private int currentLineIndex;
        private bool isTyping;
        private Coroutine typingCoroutine;

        private void Start()
        {
            currentLineIndex = 0;
            ShowCurrentLine();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                OnNextInput();
            }
        }

        private void OnNextInput()
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                storyText.text = storyLines[currentLineIndex];
                isTyping = false;
                return;
            }

            currentLineIndex++;

            if (currentLineIndex >= storyLines.Length)
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            ShowCurrentLine();
        }

        private void ShowCurrentLine()
        {
            if (storyLines == null || storyLines.Length == 0)
            {
                return;
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeLine(storyLines[currentLineIndex]));
        }

        private IEnumerator TypeLine(string line)
        {
            isTyping = true;
            storyText.text = string.Empty;

            for (int i = 0; i < line.Length; i++)
            {
                storyText.text += line[i];
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
        }
    }
}