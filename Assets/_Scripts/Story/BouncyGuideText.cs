using UnityEngine;

namespace SeoAhn
{
    public class BouncyGuideText : MonoBehaviour
    {
        [Header("통통 튀는 설정")]
        [SerializeField] private float bounceHeight = 10f;
        [SerializeField] private float bounceSpeed = 3f;

        private Vector3 startPosition;

        private void OnEnable()
        {
            startPosition = transform.localPosition;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;

            transform.localPosition = startPosition + new Vector3(0f, offset, 0f);
        }
    }
}