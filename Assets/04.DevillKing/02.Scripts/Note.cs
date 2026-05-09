using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 이벤트를 위해 추가했어요!
namespace DH
{
    public class Note : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerUpHandler
    {
        public int instrumentType;
        public int x;
        public int y;

        public void OnPointerDown(PointerEventData eventData)
        {
            PuzzleManager.Instance.StartDrawing(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Input.GetMouseButton(0))
                PuzzleManager.Instance.OnNoteEnter(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            GameObject hoveredObj = eventData.pointerCurrentRaycast.gameObject;
            Note releasedNote = hoveredObj != null ? hoveredObj.GetComponent<Note>() : null;

            PuzzleManager.Instance.EndDrawing(releasedNote);
        }
    }
}