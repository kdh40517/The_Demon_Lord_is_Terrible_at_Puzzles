using System.Collections;
using UnityEngine;

namespace SeoAhn
{
    // 클리어 도장이 "쿵!" 하고 찍히는 효과를 담당하는 스크립트입니다.
    public class StampPopEffect : MonoBehaviour
    {
        [Header("도장 애니메이션 설정")]
        [SerializeField] private float popDuration = 0.35f; // 도장 애니메이션 시간
        [SerializeField] private float overshootScale = 1.25f; // 잠깐 크게 튀는 크기

        [Header("도장 효과음")]
        [SerializeField] private AudioSource stampAudioSource; // 효과음 재생용 AudioSource
        [SerializeField] private AudioClip stampClip; // 도장 효과음
        [SerializeField] private float stampVolume = 1f; // 효과음 볼륨

        public void ShowInstantly()
        {
            // 이미 클리어된 스테이지는 애니메이션 없이 바로 표시
            gameObject.SetActive(true);
            transform.localScale = Vector3.one;
        }

        public void Hide()
        {
            // 클리어 안 된 스테이지는 숨김
            gameObject.SetActive(false);
            transform.localScale = Vector3.one;
        }

        public void PlayStamp()
        {
            gameObject.SetActive(true);

            StopAllCoroutines();

            // 도장 효과음 재생
            PlayStampSound();

            StartCoroutine(PopRoutine());
        }

        private void PlayStampSound()
        {
            if (stampAudioSource == null || stampClip == null)
            {
                return;
            }

            stampAudioSource.PlayOneShot(
                stampClip,
                stampVolume * AudioVolumeData.MasterVolume * AudioVolumeData.SFXVolume
            );
        }

        private IEnumerator PopRoutine()
        {
            float timer = 0f;

            transform.localScale = Vector3.zero;

            while (timer < popDuration)
            {
                timer += Time.deltaTime;

                float progress = timer / popDuration;
                float scale;

                if (progress < 0.7f)
                {
                    scale = Mathf.Lerp(
                        0f,
                        overshootScale,
                        progress / 0.7f
                    );
                }
                else
                {
                    scale = Mathf.Lerp(
                        overshootScale,
                        1f,
                        (progress - 0.7f) / 0.3f
                    );
                }

                transform.localScale = Vector3.one * scale;

                yield return null;
            }

            transform.localScale = Vector3.one;
        }
    }
}