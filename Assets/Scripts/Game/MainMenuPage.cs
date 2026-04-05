using BookGraph.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPage : OnPageContent
{
    [SerializeField] Button exitButton;

    public override void Initialize(RuntimeNode node, GameObject originalPage)
    {
        this.originalPage = originalPage.GetComponent<B_SpecialPage>();

        exitButton.onClick.AddListener(ExitGame);
    }

    public void ExitGame()
    {
        Debug.Log("Closing the application");
        ApplicationManager.Instance.ExitGame();
    }
}