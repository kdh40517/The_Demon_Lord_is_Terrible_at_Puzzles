using System.Collections;
using UnityEngine;

namespace LunasisGame
{
    public class ButtonSlideIn : MonoBehaviour
    {
        [Header("버튼들")]
        [SerializeField] private RectTransform startButton;
        [SerializeField] private RectTransform optionButton;
        [SerializeField] private RectTransform quitButton;

        [Header("위치")]
        [SerializeField] private float startY = -120f;
        [SerializeField] private float optionY = -250f;
        [SerializeField] private float quitY = -380f;

        [Header("등장 설정")]
        [SerializeField] private float hiddenY = -900f;
        [SerializeField] private float moveSpeed = 1500f;
        [SerializeField] private float delayBetweenButtons = 0.15f;
        [SerializeField] private float startDelay = 0.8f;

        private void Start()
        {
            HideButtons();
            StartCoroutine(ShowButtonsRoutine());
        }

        private void HideButtons()
        {
            SetButtonY(startButton, hiddenY);
            SetButtonY(optionButton, hiddenY);
            SetButtonY(quitButton, hiddenY);
        }

        private IEnumerator ShowButtonsRoutine()
        {
            yield return new WaitForSeconds(startDelay);

            yield return StartCoroutine(MoveButton(startButton, startY));

            yield return new WaitForSeconds(delayBetweenButtons);

            yield return StartCoroutine(MoveButton(optionButton, optionY));

            yield return new WaitForSeconds(delayBetweenButtons);

            yield return StartCoroutine(MoveButton(quitButton, quitY));
        }

        private IEnumerator MoveButton(RectTransform button, float targetY)
        {
            while (!Mathf.Approximately(button.anchoredPosition.y, targetY))
            {
                Vector2 position = button.anchoredPosition;

                position.y = Mathf.MoveTowards(
                    position.y,
                    targetY,
                    moveSpeed * Time.deltaTime
                );

                button.anchoredPosition = position;

                yield return null;
            }
        }

        private void SetButtonY(RectTransform button, float y)
        {
            Vector2 position = button.anchoredPosition;
            position.y = y;
            button.anchoredPosition = position;
        }
    }
}