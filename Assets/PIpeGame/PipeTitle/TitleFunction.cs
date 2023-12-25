using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pipe.PipeTitle
{
    public class TitleFunction : MonoBehaviour
    {
        public void Continue()
        {
            SceneManager.LoadScene("Scenes/PlayGround");
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}