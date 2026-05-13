using UnityEngine;
using System.Collections;

namespace TM
{
    public class UIShaker : MonoBehaviour
    {
        [Header("흔들 UI (비워두면 이 스크립트가 붙은 오브젝트를 흔듦)")]
        public RectTransform targetUI;

        [Header("흔들림 설정")]
        public float shakeAmount = 20f; // 흔들리는 범위 (픽셀)
        public float shakeTime = 0.2f;  // 흔들리는 시간

        private Vector3 originalPos;
        private Coroutine shakeCoroutine;

        private void Start()
        {
            // 타겟을 지정 안 했으면 자기가 직접 흔들림
            if (targetUI == null)
            {
                targetUI = GetComponent<RectTransform>();
            }
            originalPos = targetUI.localPosition;
        }

        // 📌 퍼즐 틀렸을 때 이 함수를 실행!
        public void Shake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                targetUI.localPosition = originalPos; // 제자리로 돌려놓고 다시 시작
            }
            shakeCoroutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;

            while (elapsed < shakeTime)
            {
                // 랜덤한 X, Y 위치로 마구 움직임
                float x = Random.Range(-1f, 1f) * shakeAmount;
                float y = Random.Range(-1f, 1f) * shakeAmount;

                targetUI.localPosition = originalPos + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 다 흔들리면 원래 위치로 복귀
            targetUI.localPosition = originalPos;
        }
    }
}