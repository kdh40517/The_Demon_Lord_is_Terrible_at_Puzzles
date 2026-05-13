using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeoAhn
{
    // Unity 기본 스플래쉬가 끝난 뒤,
    // 팀 로고를 부드럽게 보여주고 효과음을 재생한 뒤 타이틀씬으로 넘어가는 스크립트입니다.
    public class TeamSplashController : MonoBehaviour
    {
        [Header("팀 로고")]
        [SerializeField] private CanvasGroup logoCanvasGroup; // 팀 로고 CanvasGroup

        [Header("효과음")]
        [SerializeField] private AudioSource audioSource; // 효과음 재생용 AudioSource
        [SerializeField] private AudioClip starSoundClip; // 로고가 나타날 때 재생할 별빛 효과음
        [SerializeField] private AudioClip childrenLaughClip; // 로고가 다 나온 뒤 재생할 아이들 웃음소리
        [SerializeField] private float sfxVolume = 1f; // 효과음 볼륨

        [Header("시간 설정")]
        [SerializeField] private float fadeInTime = 1f; // 로고가 나타나는 시간
        [SerializeField] private float stayTime = 1.5f; // 로고가 유지되는 시간
        [SerializeField] private float fadeOutTime = 1f; // 로고가 사라지는 시간
        [SerializeField] private float laughDelayAfterFadeIn = 0.1f; // 로고가 다 나온 뒤 웃음소리까지의 짧은 대기

        [Header("다음 씬")]
        [SerializeField] private string titleSceneName = "01_TitleScene"; // 넘어갈 타이틀씬 이름

        private void Start()
        {
            StartCoroutine(PlaySplashRoutine());
        }

        private IEnumerator PlaySplashRoutine()
        {
            if (logoCanvasGroup != null)
            {
                logoCanvasGroup.alpha = 0f;
            }

            PlaySFX(starSoundClip);

            yield return StartCoroutine(FadeLogo(0f, 1f, fadeInTime));

            yield return new WaitForSeconds(laughDelayAfterFadeIn);
            PlaySFX(childrenLaughClip);

            yield return new WaitForSeconds(stayTime);

            yield return StartCoroutine(FadeLogo(1f, 0f, fadeOutTime));

            SceneManager.LoadScene(titleSceneName);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (audioSource == null || clip == null)
            {
                return;
            }

            audioSource.PlayOneShot(
                clip,
                sfxVolume * AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume
            );
        }

        private IEnumerator FadeLogo(float startAlpha, float endAlpha, float duration)
        {
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);

                if (logoCanvasGroup != null)
                {
                    logoCanvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (logoCanvasGroup != null)
            {
                logoCanvasGroup.alpha = endAlpha;
            }
        }
    }
}