using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace SeoAhn
{
    public class StoryCutsceneCameraController : MonoBehaviour
    {
        [Header("¾¾³×¸Ó½Å Ä«¸Þ¶ó")]
        [SerializeField] private CinemachineCamera overviewCamera;
        [SerializeField] private CinemachineCamera heroCamera;
        [SerializeField] private CinemachineCamera bossCamera;

        [Header("¸»Ç³¼±")]
        [SerializeField] private GameObject heroSpeechBubble;
        [SerializeField] private GameObject bossSpeechBubble;

        [Header("ÁÜ ÈÄ ¸»Ç³¼± µîÀå ½Ã°£")]
        [SerializeField] private float speechBubbleDelay = 1.2f;

        private int currentStep;
        private Coroutine speechCoroutine;

        private void Start()
        {
            currentStep = 0;

            HideAllSpeechBubbles();
            ShowOverviewCamera();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                GoNextStep();
            }
        }

        private void GoNextStep()
        {
            currentStep++;

            if (currentStep > 3)
            {
                currentStep = 1;
            }

            if (currentStep == 1)
            {
                ShowBossSequence();
            }
            else if (currentStep == 2)
            {
                ShowHeroSequence();
            }
            else if (currentStep == 3)
            {
                ShowOverviewSequence();
            }
        }

        private void ShowBossSequence()
        {
            StopSpeechRoutine();

            HideAllSpeechBubbles();

            SetPriority(overviewCamera, 10);
            SetPriority(heroCamera, 10);
            SetPriority(bossCamera, 30);

            speechCoroutine = StartCoroutine(ShowSpeechAfterDelay(bossSpeechBubble));
        }

        private void ShowHeroSequence()
        {
            StopSpeechRoutine();

            HideAllSpeechBubbles();

            SetPriority(overviewCamera, 10);
            SetPriority(heroCamera, 30);
            SetPriority(bossCamera, 10);

            speechCoroutine = StartCoroutine(ShowSpeechAfterDelay(heroSpeechBubble));
        }

        private void ShowOverviewSequence()
        {
            StopSpeechRoutine();

            HideAllSpeechBubbles();

            ShowOverviewCamera();
        }

        private IEnumerator ShowSpeechAfterDelay(GameObject speechBubble)
        {
            yield return new WaitForSeconds(speechBubbleDelay);

            if (speechBubble != null)
            {
                speechBubble.SetActive(true);
            }
        }

        private void ShowOverviewCamera()
        {
            SetPriority(overviewCamera, 30);
            SetPriority(heroCamera, 10);
            SetPriority(bossCamera, 10);
        }

        private void HideAllSpeechBubbles()
        {
            if (heroSpeechBubble != null)
            {
                heroSpeechBubble.SetActive(false);
            }

            if (bossSpeechBubble != null)
            {
                bossSpeechBubble.SetActive(false);
            }
        }

        private void StopSpeechRoutine()
        {
            if (speechCoroutine != null)
            {
                StopCoroutine(speechCoroutine);
                speechCoroutine = null;
            }
        }

        private void SetPriority(CinemachineCamera cameraTarget, int priority)
        {
            if (cameraTarget == null)
            {
                return;
            }

            cameraTarget.Priority = priority;
        }
    }
}