using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    // ForestScene의 틀린그림찾기 퍼즐을 관리하는 스크립트입니다.
    // 정답/오답 효과, 퍼즐 페이드아웃, 클리어 이미지 페이드인, StageScene 복귀를 담당합니다.
    public class ForestDifferenceManager : MonoBehaviour
    {
        [Header("찾음 표시 원")]
        [SerializeField] private DrawMarkEffect[] foundCircleEffects; // 정답 동그라미 그리기 효과

        [Header("효과음")]
        [SerializeField] private AudioSource sfxAudioSource; // 효과음 재생용 AudioSource
        [SerializeField] private AudioClip correctClip; // 정답 효과음
        [SerializeField] private AudioClip wrongClip; // 오답 효과음
        [SerializeField] private float sfxVolume = 1f; // 효과음 볼륨

        // 👇 새롭게 추가된 BGM 교체 시스템!
        [Header("BGM 설정")]
        [SerializeField] private AudioSource bgmPlayer; // 평소에 배경음악을 틀고 있는 스피커
        [SerializeField] private AudioClip clearBGM;    // 클리어 시 틀어줄 BGM

        [Header("오답 표시")]
        [SerializeField] private RectTransform wrongMarkImage; // X 표시 위치
        [SerializeField] private DrawMarkEffect wrongMarkEffect; // X 그리기 효과
        [SerializeField] private float wrongMarkShowTime = 0.7f; // X 표시 유지 시간

        [Header("클리어 시 사라질 UI")]
        [SerializeField] private CanvasGroup puzzlePanelCanvasGroup; // GameImages
        [SerializeField] private CanvasGroup frameCanvasGroup; // Frame
        [SerializeField] private float transitionDuration = 1.5f; // 퍼즐이 사라지고 클리어 UI가 나타나는 시간

        [Header("클리어 UI")]
        [SerializeField] private CanvasGroup clearUIGroup; // ClearImage
        [SerializeField] private CanvasGroup clearTextUIGroup; // CLEAR! 이미지
        [SerializeField] private float returnDelay = 3f; // 클리어 화면이 나온 뒤 로딩씬으로 넘어가기 전 대기 시간

        [Header("씬 이동")]
        [SerializeField] private string loadingSceneName = "99_LoadingScene"; // 로딩씬 이름
        [SerializeField] private string stageSceneName = "03_StageScene"; // 돌아갈 스테이지씬 이름

        private bool[] foundStates; // 각 정답을 찾았는지 저장
        private int foundCount; // 찾은 정답 개수
        private bool isCleared; // 클리어 여부
        private Coroutine wrongMarkCoroutine; // 오답 X 표시 코루틴

        private void Start()
        {
            foundCount = 0;
            isCleared = false;

            foundStates = new bool[foundCircleEffects.Length];

            // 시작할 때 모든 정답 동그라미를 숨깁니다.
            for (int i = 0; i < foundCircleEffects.Length; i++)
            {
                if (foundCircleEffects[i] != null)
                {
                    foundCircleEffects[i].Hide();
                }
            }

            // 시작할 때 오답 X 표시를 숨깁니다.
            if (wrongMarkEffect != null)
            {
                wrongMarkEffect.Hide();
            }

            // 퍼즐 전체 이미지는 처음에 보이게 둡니다.
            if (puzzlePanelCanvasGroup != null)
            {
                puzzlePanelCanvasGroup.alpha = 1f;
                puzzlePanelCanvasGroup.interactable = true;
                puzzlePanelCanvasGroup.blocksRaycasts = true;
            }

            // 프레임도 처음에 보이게 둡니다.
            if (frameCanvasGroup != null)
            {
                frameCanvasGroup.alpha = 1f;
                frameCanvasGroup.interactable = true;
                frameCanvasGroup.blocksRaycasts = true;
            }

            // 클리어 배경 이미지는 처음에 숨깁니다.
            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 0f;
                clearUIGroup.interactable = false;
                clearUIGroup.blocksRaycasts = false;
            }

            // CLEAR! 이미지는 처음에 숨깁니다.
            if (clearTextUIGroup != null)
            {
                clearTextUIGroup.alpha = 0f;
                clearTextUIGroup.interactable = false;
                clearTextUIGroup.blocksRaycasts = false;
            }
        }

        public void ClickDifference(int index)
        {
            // 이미 클리어했다면 더 이상 클릭을 받지 않습니다.
            if (isCleared)
            {
                return;
            }

            // 잘못된 번호가 들어오면 무시합니다.
            if (index < 0 || index >= foundStates.Length)
            {
                return;
            }

            // 이미 찾은 정답이면 중복 처리하지 않습니다.
            if (foundStates[index])
            {
                return;
            }

            PlaySFX(correctClip);

            foundStates[index] = true;
            foundCount++;

            // 해당 정답 위치의 동그라미를 그리듯 표시합니다.
            if (foundCircleEffects[index] != null)
            {
                foundCircleEffects[index].PlayDraw();
            }

            // 모든 정답을 찾으면 클리어 처리합니다.
            if (foundCount >= foundStates.Length)
            {
                StartCoroutine(ClearRoutine());
            }
        }

        public void ClickWrongArea()
        {
            // 이미 클리어했거나 X 표시가 없으면 처리하지 않습니다.
            if (isCleared || wrongMarkImage == null || wrongMarkEffect == null)
            {
                return;
            }

            PlaySFX(wrongClip);

            // 클릭한 위치로 X 표시를 이동시킵니다.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                wrongMarkImage.parent as RectTransform,
                Input.mousePosition,
                null,
                out Vector2 mousePosition
            );

            wrongMarkImage.anchoredPosition = mousePosition;

            if (wrongMarkCoroutine != null)
            {
                StopCoroutine(wrongMarkCoroutine);
            }

            wrongMarkCoroutine = StartCoroutine(ShowWrongMarkRoutine());
        }

        private IEnumerator ShowWrongMarkRoutine()
        {
            // X를 슥슥 그리듯이 표시합니다.
            wrongMarkEffect.PlayDraw();

            yield return new WaitForSeconds(wrongMarkShowTime);

            wrongMarkEffect.Hide();
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxAudioSource == null || clip == null)
            {
                return;
            }

            sfxAudioSource.PlayOneShot(
                clip,
                sfxVolume * AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume
            );
        }

        private IEnumerator ClearRoutine()
        {
            isCleared = true;

            // 👇 클리어 음악 재생 로직! (기존 BGM 멈춤 -> 무한반복 해제 -> 클리어 음악 재생)
            if (bgmPlayer != null)
            {
                bgmPlayer.Stop();
                bgmPlayer.loop = false; // 클리어 음악은 딱 한 번만 나오도록 루프를 꺼줍니다.

                if (clearBGM != null)
                {
                    bgmPlayer.clip = clearBGM;
                    bgmPlayer.Play();
                }
            }

            // 퍼즐과 프레임은 사라지고, ClearImage와 CLEAR! 이미지는 같이 나타납니다.
            yield return StartCoroutine(FadePuzzleOutAndClearIn());

            yield return new WaitForSeconds(returnDelay);

            StageClearManager.SetForestClear();
            SceneTransitionData.SetNextScene(stageSceneName);

            SceneManager.LoadScene(loadingSceneName);
        }

        private IEnumerator FadePuzzleOutAndClearIn()
        {
            float timer = 0f;

            while (timer < transitionDuration)
            {
                timer += Time.deltaTime;

                float progress = timer / transitionDuration;

                float puzzleAlpha = Mathf.Lerp(1f, 0f, progress);
                float clearAlpha = Mathf.Lerp(0f, 1f, progress);

                if (puzzlePanelCanvasGroup != null)
                {
                    puzzlePanelCanvasGroup.alpha = puzzleAlpha;
                }

                if (frameCanvasGroup != null)
                {
                    frameCanvasGroup.alpha = puzzleAlpha;
                }

                if (clearUIGroup != null)
                {
                    clearUIGroup.alpha = clearAlpha;
                }

                if (clearTextUIGroup != null)
                {
                    clearTextUIGroup.alpha = clearAlpha;
                }

                yield return null;
            }

            if (puzzlePanelCanvasGroup != null)
            {
                puzzlePanelCanvasGroup.alpha = 0f;
                puzzlePanelCanvasGroup.interactable = false;
                puzzlePanelCanvasGroup.blocksRaycasts = false;
            }

            if (frameCanvasGroup != null)
            {
                frameCanvasGroup.alpha = 0f;
                frameCanvasGroup.interactable = false;
                frameCanvasGroup.blocksRaycasts = false;
            }

            if (clearUIGroup != null)
            {
                clearUIGroup.alpha = 1f;
            }

            if (clearTextUIGroup != null)
            {
                clearTextUIGroup.alpha = 1f;
            }
        }
    }
}