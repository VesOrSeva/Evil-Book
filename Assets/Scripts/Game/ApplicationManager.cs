using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    public void ExitGame()
    {
        Application.Quit();
    }
}