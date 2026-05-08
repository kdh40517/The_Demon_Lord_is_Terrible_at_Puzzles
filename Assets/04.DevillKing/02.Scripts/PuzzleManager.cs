using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DH
{
    public class PuzzleManager : MonoBehaviour
    {
        public static PuzzleManager Instance;

        [Header("연결된 노드")]
        public List<Note> connectedNotes = new List<Note>();

        [Header("선 연결 설정")]
        public LineRenderer lineRenderer;
        public LineRenderer dragLineRenderer;

        [Header("효과음 설정")]
        public AudioSource sfxPlayer;
        public AudioClip[] instrumentSounds;

        private Note lastHoveredNote;
        private bool isDrawing = false;

        void Awake() { Instance = this; }

        public void StartDrawing(Note firstNote)
        {
            if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

            connectedNotes.Clear();
            AddNoteToPath(firstNote);
            PlayInstrumentSound(firstNote.instrumentType);
            UpdateLine();
            isDrawing = true;
        }

        void Update()
        {
            if (isDrawing && connectedNotes.Count > 0)
            {
                Vector3 startPos = connectedNotes[connectedNotes.Count - 1].transform.position;
                startPos.z -= 1f;

                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
                Vector3 endPos = Camera.main.ScreenToWorldPoint(mousePos);
                endPos.z -= 1f;

                dragLineRenderer.positionCount = 2;
                dragLineRenderer.SetPosition(0, startPos);
                dragLineRenderer.SetPosition(1, endPos);
            }
        }

        public void OnNoteEnter(Note enteredNote)
        {
            if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;

            lastHoveredNote = enteredNote;

            if (connectedNotes.Count == 0) return;

            if (connectedNotes.Count >= 2 && connectedNotes[connectedNotes.Count - 2] == enteredNote)
            {
                Note lastNoteToRemove = connectedNotes[connectedNotes.Count - 1];

                CanvasGroup cg = lastNoteToRemove.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;

                connectedNotes.RemoveAt(connectedNotes.Count - 1);
                UpdateLine();

                Debug.Log("되돌리기 완료! 현재 연결 개수: " + connectedNotes.Count);
                return;
            }

            if (connectedNotes.Contains(enteredNote)) return;

            Note lastNote = connectedNotes[connectedNotes.Count - 1];

            if (lastNote.instrumentType == enteredNote.instrumentType || enteredNote.instrumentType == 99 || lastNote.instrumentType == 99)
            {
                int distanceX = Mathf.Abs(lastNote.x - enteredNote.x);
                int distanceY = Mathf.Abs(lastNote.y - enteredNote.y);

                if (distanceX <= 1 && distanceY <= 1)
                {
                    AddNoteToPath(enteredNote);
                    PlayInstrumentSound(enteredNote.instrumentType);
                    UpdateLine();
                }
            }
        }

        void AddNoteToPath(Note note)
        {
            connectedNotes.Add(note);
            CanvasGroup cg = note.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0.5f;
        }

        void UpdateLine()
        {
            lineRenderer.positionCount = connectedNotes.Count;
            for (int i = 0; i < connectedNotes.Count; i++)
            {
                lineRenderer.SetPosition(i, connectedNotes[i].transform.position);
            }
        }

        public void EndDrawing(Note releasedNote)
        {
            isDrawing = false;
            dragLineRenderer.positionCount = 0;

            if (connectedNotes.Count >= 3 && releasedNote == connectedNotes[connectedNotes.Count - 1])
            {
                Debug.Log("🎉 퍼즐 성공! 악기 파괴!");

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CheckWinCondition();
                }

                foreach (Note note in connectedNotes)
                {
                    BoardManager.Instance.board[note.x, note.y] = null;
                    Destroy(note.gameObject);
                }
                StartCoroutine(ApplyGravity());
            }
            else
            {
                Debug.Log("❌ 취소됨: 마지막 블록이 아니거나 개수 부족");
                foreach (Note note in connectedNotes)
                {
                    CanvasGroup cg = note.GetComponent<CanvasGroup>();
                    if (cg != null) cg.alpha = 1f;
                }
            }

            connectedNotes.Clear();
            lineRenderer.positionCount = 0;
        }

        IEnumerator ApplyGravity()
        {
            int width = BoardManager.Instance.width;
            int height = BoardManager.Instance.height;
            float cellSize = BoardManager.Instance.cellSize;
            float spacing = BoardManager.Instance.spacing;
            Note[,] board = BoardManager.Instance.board;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        for (int upperY = y + 1; upperY < height; upperY++)
                        {
                            if (board[x, upperY] != null)
                            {
                                Note fallingNote = board[x, upperY];
                                board[x, y] = fallingNote;
                                board[x, upperY] = null;
                                fallingNote.y = y;

                                StartCoroutine(MoveBlock(fallingNote, x, y, cellSize, spacing, width, height));
                                break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board[x, y] == null)
                    {
                        BoardManager.Instance.SpawnNote(x, y, 800f);
                        StartCoroutine(MoveBlock(board[x, y], x, y, cellSize, spacing, width, height));
                    }
                }
            }
        }

        IEnumerator MoveBlock(Note note, int x, int y, float cellSize, float spacing, int width, int height)
        {
            float startX = -(width * cellSize + (width - 1) * spacing) / 2f + cellSize / 2f;
            float startY = -(height * cellSize + (height - 1) * spacing) / 2f + cellSize / 2f;

            Vector2 targetPos = new Vector2(startX + x * (cellSize + spacing), startY + y * (cellSize + spacing));
            RectTransform rect = note.GetComponent<RectTransform>();

            while (Vector2.Distance(rect.anchoredPosition, targetPos) > 1f)
            {
                if (note == null) yield break;
                rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * 10f);
                yield return null;
            }
            rect.anchoredPosition = targetPos;
        }

        public void PlayInstrumentSound(int instrumentIndex)
        {
            if (sfxPlayer != null && instrumentIndex < instrumentSounds.Length)
            {
                AudioClip soundToPlay = instrumentSounds[instrumentIndex];
                if (soundToPlay != null)
                {
                    sfxPlayer.PlayOneShot(soundToPlay);
                }
            }
        }
    }
}