using UnityEngine;
using UnityEngine.EventSystems;

namespace DH
{
    public class Note : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        [Header("조각 설정")]
        public NoteType instrumentType;

        // ★ 인스펙터에서 체크하는 독 스티커 (그대로 유지!)
        public bool isPoisoned = false;

        [Header("상태 효과 레이어")]
        // ★ 방금 만든 까만색 반투명 이미지를 여기에 넣습니다!
        public GameObject brokenOverlay;

        private bool _isBroken = false;

        // ★ 매니저가 isBroken = true; 라고 하는 순간 자동으로 까만 필터가 켜집니다!
        public bool isBroken
        {
            get => _isBroken;
            set
            {
                _isBroken = value;
                if (brokenOverlay != null) brokenOverlay.SetActive(_isBroken);
            }
        }

        [HideInInspector] public int x;
        [HideInInspector] public int y;

        public void OnPointerDown(PointerEventData eventData) { PuzzleManager.Instance.StartDrawing(this); }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Input.GetMouseButton(0)) PuzzleManager.Instance.OnNoteEnter(this);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            GameObject hoveredObj = eventData.pointerCurrentRaycast.gameObject;
            Note releasedNote = hoveredObj != null ? hoveredObj.GetComponent<Note>() : null;
            PuzzleManager.Instance.EndDrawing(releasedNote);
        }
    }
}