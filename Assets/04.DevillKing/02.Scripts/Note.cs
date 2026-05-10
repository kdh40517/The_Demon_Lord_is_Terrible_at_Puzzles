using UnityEngine;
using UnityEngine.EventSystems;

namespace DH
{
    public class Note : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        [Header("조각 설정")]
        public NoteType instrumentType;
        public bool isPoisoned = false;
        public bool isBroken = false; // ★ 유고수 추가: 능력치가 사라진 고장 난 상태인지 확인!

        [HideInInspector]
        public int x;
        [HideInInspector]
        public int y;

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