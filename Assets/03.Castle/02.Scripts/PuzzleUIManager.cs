using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace TM
{
    public class PuzzleUIManager : MonoBehaviour
    {
        [Header("연결 설정")]
        public Transform arrowContainer;
        public GameObject arrowPrefab;

        [Header("화살표 스프라이트")]
        public Sprite upArrow;
        public Sprite downArrow;
        public Sprite leftArrow;
        public Sprite rightArrow;

        // --- NEW: Space bar icon sprite ---
        [Tooltip("스페이스바 아이콘 ( runic 'SPACE' key)")]
        public Sprite spaceBar;

        // 생성된 UI 오브젝트들을 관리
        private Queue<GameObject> arrowPool = new Queue<GameObject>();

        // 새 화살표 UI 생성 (오른쪽 끝에 추가됨)
        public void CreateArrowUI(KeyCode key)
        {
            GameObject newArrow = Instantiate(arrowPrefab, arrowContainer);
            Image img = newArrow.GetComponent<Image>();

            if (img != null)
            {
                img.sprite = GetSpriteForKey(key);
            }

            arrowPool.Enqueue(newArrow);
        }

        // 맨 앞의 화살표 UI 제거 (리스트가 왼쪽으로 밀림)
        public void RemoveFrontArrowUI()
        {
            if (arrowPool.Count > 0)
            {
                GameObject frontArrow = arrowPool.Dequeue();
                Destroy(frontArrow);
            }
        }

        // --- UPDATED: Handle KeyCode.Space ---
        private Sprite GetSpriteForKey(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.UpArrow: return upArrow;
                case KeyCode.DownArrow: return downArrow;
                case KeyCode.LeftArrow: return leftArrow;
                case KeyCode.RightArrow: return rightArrow;

                // New case to return the Space key sprite
                case KeyCode.Space: return spaceBar;

                default: return null;
            }
        }
    }
}