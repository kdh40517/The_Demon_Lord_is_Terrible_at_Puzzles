using UnityEngine;
using UnityEngine.SceneManagement;

namespace LunasisGame
{
    public class TitleSceneController : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene("02_StoryScene");
        }

        public void OpenOptions()
        {
            Debug.Log("옵션 버튼 클릭");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}