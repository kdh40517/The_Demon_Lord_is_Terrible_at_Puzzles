using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SeoAhn
{
    public class DrawMarkEffect : MonoBehaviour
    {
        [Header("동그라미")]
        [SerializeField] private Image circleImage;

        [Header("X")]
        [SerializeField] private Image xLineImageA;
        [SerializeField] private Image xLineImageB;

        [Header("설정")]
        [SerializeField] private bool isCircle = true;
        [SerializeField] private float drawDuration = 0.35f;
        [SerializeField] private float xLineGap = 0.05f;

        private void Awake()
        {
            ResetMark();
        }

        public void PlayDraw()
        {
            gameObject.SetActive(true);

            StopAllCoroutines();
            ResetMark();

            StartCoroutine(DrawRoutine());
        }

        public void Hide()
        {
            StopAllCoroutines();
            ResetMark();
            gameObject.SetActive(false);
        }

        private void ResetMark()
        {
            if (circleImage != null)
            {
                circleImage.fillAmount = 0f;
            }

            if (xLineImageA != null)
            {
                xLineImageA.fillAmount = 0f;
                xLineImageA.gameObject.SetActive(true);
            }

            if (xLineImageB != null)
            {
                xLineImageB.fillAmount = 0f;
                xLineImageB.gameObject.SetActive(true);
            }
        }

        private IEnumerator DrawRoutine()
        {
            if (isCircle)
            {
                yield return DrawCircleRoutine();
            }
            else
            {
                yield return DrawXRoutine();
            }
        }

        private IEnumerator DrawCircleRoutine()
        {
            if (circleImage == null)
            {
                yield break;
            }

            float timer = 0f;

            while (timer < drawDuration)
            {
                timer += Time.deltaTime;

                circleImage.fillAmount = Mathf.Clamp01(timer / drawDuration);

                yield return null;
            }

            circleImage.fillAmount = 1f;
        }

        private IEnumerator DrawXRoutine()
        {
            if (xLineImageA == null || xLineImageB == null)
            {
                yield break;
            }

            float halfDuration = drawDuration * 0.5f;

            float timer = 0f;

            while (timer < halfDuration)
            {
                timer += Time.deltaTime;

                xLineImageA.fillAmount = Mathf.Clamp01(timer / halfDuration);

                yield return null;
            }

            xLineImageA.fillAmount = 1f;

            yield return new WaitForSeconds(xLineGap);

            timer = 0f;

            while (timer < halfDuration)
            {
                timer += Time.deltaTime;

                xLineImageB.fillAmount = Mathf.Clamp01(timer / halfDuration);

                yield return null;
            }

            xLineImageB.fillAmount = 1f;
        }
    }
}