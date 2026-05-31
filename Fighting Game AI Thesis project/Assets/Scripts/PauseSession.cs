using UnityEngine;

public class PauseSession : MonoBehaviour
{
    public GameObject CanvasElement;
    public bool isPaused = true;

    void Awake()
    {
        CanvasElement.SetActive(true);
        Time.timeScale = 0f;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseToggle();
        }
    }


    void PauseToggle()
    {
        if(!isPaused)
        {
            CanvasElement.SetActive(true);
            Time.timeScale = 0f;
            isPaused = true;
        }
        else
        {
            CanvasElement.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
        }
    }
}
