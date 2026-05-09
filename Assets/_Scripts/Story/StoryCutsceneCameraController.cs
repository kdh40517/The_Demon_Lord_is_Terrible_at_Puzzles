using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace SeoAhn
{
    // 스토리 컷씬 전체 진행을 담당하는 컨트롤러
    // 역할:
    // 1. 전체 화면 → 보스 → 히어로 → 보스 카메라 전환
    // 2. 말풍선 표시
    // 3. 보스 웃음 효과음 재생
    // 4. 컷씬 종료 후 AfterCutscene 장면 실행
    public class StoryCutsceneCameraController : MonoBehaviour
    {
        [Header("씨네머신 카메라")]
        [SerializeField] private CinemachineCamera overviewCamera; // 처음 전체 장면을 보여주는 카메라
        [SerializeField] private CinemachineCamera heroCamera;     // 히어로를 보여주는 카메라
        [SerializeField] private CinemachineCamera bossCamera;     // 보스를 보여주는 카메라

        [Header("말풍선")]
        [SerializeField] private GameObject heroSpeechBubble;       // 히어로 말풍선
        [SerializeField] private GameObject bossSpeechBubbleFirst;  // 첫 번째 보스 말풍선
        [SerializeField] private GameObject bossSpeechBubbleSecond; // 두 번째 보스 말풍선

        [Header("다음 안내 텍스트")]
        [SerializeField] private GameObject nextGuideText; // "스페이스바를 눌러 넘어가기!" 안내문

        [Header("보스 웃음 효과음")]
        [SerializeField] private AudioSource bossLaughAudioSource;      // 웃음소리를 재생할 AudioSource
        [SerializeField] private AudioClip bossLaughClip;               // 웃음소리 오디오 파일
        [SerializeField] private float bossLaughVolume = 0.12f;         // 웃음소리 볼륨
        [SerializeField] private float bossLaughToSpeechDelay = 0.3f;   // 웃음소리 후 말풍선 등장까지 대기 시간

        [Header("자동 보스 줌인 딜레이")]
        [SerializeField] private float firstBossZoomDelay = 2f; // 컷씬 시작 후 보스로 줌인하기 전 대기 시간

        [Header("줌 후 말풍선 등장 시간")]
        [SerializeField] private float speechBubbleDelay = 1.2f; // 카메라 이동 후 말풍선 등장까지 대기 시간

        [Header("컷씬 이후 장면")]
        [SerializeField] private GameObject afterCutsceneManager;     // 컷씬 종료 후 실행할 AfterCutscene 매니저
        [SerializeField] private float afterCutsceneStartDelay = 1f;  // 컷씬 종료 후 다음 장면 시작 전 딜레이

        private int currentStep;               // 현재 컷씬 진행 단계
        private bool canPressSpace;            // 스페이스바 입력 허용 여부
        private Coroutine speechCoroutine;     // 현재 실행 중인 말풍선 코루틴

        private void Start()
        {
            // 컷씬 시작 상태 초기화
            currentStep = 0;
            canPressSpace = false;

            // 후속 장면은 처음엔 꺼둠
            if (afterCutsceneManager != null)
            {
                afterCutsceneManager.SetActive(false);
            }

            // 말풍선과 안내문 초기 숨김
            HideAllSpeechBubbles();
            HideNextGuideText();

            // 처음엔 전체 화면 보여주기
            ShowOverviewCamera();

            // 잠시 후 보스 줌인 시작
            StartCoroutine(StartBossZoomAfterDelay());
        }

        private void Update()
        {
            // 아직 컷씬 진행 입력이 허용되지 않으면 종료
            if (!canPressSpace)
            {
                return;
            }

            // 스페이스바 입력 시 다음 단계 진행
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GoNextStep();
            }
        }

        private IEnumerator StartBossZoomAfterDelay()
        {
            // 전체 화면을 잠깐 보여준 뒤 첫 번째 보스 장면 시작
            yield return new WaitForSeconds(firstBossZoomDelay);

            currentStep = 1;
            ShowBossSequence(bossSpeechBubbleFirst);

            canPressSpace = true;
        }

        private void GoNextStep()
        {
            // 단계 전환 시 안내문 숨김
            HideNextGuideText();

            if (currentStep == 1)
            {
                // 첫 번째 보스 → 히어로
                currentStep = 2;
                ShowHeroSequence();
            }
            else if (currentStep == 2)
            {
                // 히어로 → 두 번째 보스
                currentStep = 3;
                ShowBossSequence(bossSpeechBubbleSecond, true);
            }
            else if (currentStep == 3)
            {
                // 마지막 보스 → AfterCutscene 장면
                StartCoroutine(StartAfterCutsceneSceneAfterDelay());
            }
        }

        private IEnumerator StartAfterCutsceneSceneAfterDelay()
        {
            // 더 이상 컷씬 입력은 받지 않음
            canPressSpace = false;

            // 현재 실행 중인 말풍선 코루틴만 정지
            StopSpeechRoutine();

            // 안내문만 숨김
            // 말풍선은 딜레이 동안 그대로 유지
            HideNextGuideText();

            // 마지막 장면 분위기를 위해 잠시 대기
            yield return new WaitForSeconds(afterCutsceneStartDelay);

            // 딜레이가 끝난 뒤 말풍선 정리
            HideAllSpeechBubbles();

            // AfterCutscene 시작
            if (afterCutsceneManager != null)
            {
                afterCutsceneManager.SetActive(true);
            }
        }

        private void ShowOverviewCamera()
        {
            // 전체 화면 카메라 활성화
            StopSpeechRoutine();

            SetPriority(overviewCamera, 30);
            SetPriority(heroCamera, 10);
            SetPriority(bossCamera, 10);

            HideAllSpeechBubbles();
        }

        private void ShowBossSequence(GameObject speechBubble, bool playLaughBeforeSpeech = false)
        {
            // 보스 카메라 장면 실행
            StopSpeechRoutine();
            HideAllSpeechBubbles();

            SetPriority(overviewCamera, 10);
            SetPriority(heroCamera, 10);
            SetPriority(bossCamera, 30);

            speechCoroutine = StartCoroutine(
                ShowSpeechAfterDelay(speechBubble, playLaughBeforeSpeech));
        }

        private void ShowHeroSequence()
        {
            // 히어로 카메라 장면 실행
            StopSpeechRoutine();
            HideAllSpeechBubbles();

            SetPriority(overviewCamera, 10);
            SetPriority(heroCamera, 30);
            SetPriority(bossCamera, 10);

            speechCoroutine = StartCoroutine(
                ShowSpeechAfterDelay(heroSpeechBubble));
        }

        private IEnumerator ShowSpeechAfterDelay(
            GameObject speechBubble,
            bool playLaughBeforeSpeech = false)
        {
            // 카메라 이동이 끝날 때까지 대기
            yield return new WaitForSeconds(speechBubbleDelay);

            // 보스 두 번째 장면이면 웃음소리 재생
            if (playLaughBeforeSpeech)
            {
                PlayBossLaugh();
                yield return new WaitForSeconds(bossLaughToSpeechDelay);
            }

            // 말풍선 표시
            if (speechBubble != null)
            {
                speechBubble.SetActive(true);
            }

            // 안내문 표시
            ShowNextGuideText();
        }

        private void PlayBossLaugh()
        {
            // 웃음소리 재생
            if (bossLaughAudioSource == null || bossLaughClip == null)
            {
                return;
            }

            bossLaughAudioSource.PlayOneShot(
                bossLaughClip,
                bossLaughVolume);
        }

        private void ShowNextGuideText()
        {
            // 스페이스바 안내문 표시
            if (nextGuideText != null)
            {
                nextGuideText.SetActive(true);
            }
        }

        private void HideNextGuideText()
        {
            // 스페이스바 안내문 숨김
            if (nextGuideText != null)
            {
                nextGuideText.SetActive(false);
            }
        }

        private void HideAllSpeechBubbles()
        {
            // 모든 말풍선 숨김
            if (heroSpeechBubble != null)
            {
                heroSpeechBubble.SetActive(false);
            }

            if (bossSpeechBubbleFirst != null)
            {
                bossSpeechBubbleFirst.SetActive(false);
            }

            if (bossSpeechBubbleSecond != null)
            {
                bossSpeechBubbleSecond.SetActive(false);
            }
        }

        private void StopSpeechRoutine()
        {
            // 현재 실행 중인 말풍선 코루틴 정지
            if (speechCoroutine != null)
            {
                StopCoroutine(speechCoroutine);
                speechCoroutine = null;
            }
        }

        private void SetPriority(CinemachineCamera cameraTarget, int priority)
        {
            // 씨네머신 카메라 우선순위 설정
            if (cameraTarget == null)
            {
                return;
            }

            cameraTarget.Priority = priority;
        }
    }
}