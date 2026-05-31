using UnityEngine;

public class GameSettings : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate =60;
        QualitySettings.vSyncCount = 0;
        Application.runInBackground = true;
        Screen.fullScreenMode = FullScreenMode.Windowed;
    }
}
