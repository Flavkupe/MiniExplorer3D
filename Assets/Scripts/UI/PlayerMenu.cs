using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : BaseWindow
{
    public TMPro.TMP_InputField navigationInputField;

    public Button navigationSubmitButton;

    public SceneLoader sceneLoader;

    public static PlayerMenu Instance { get; private set; }

    void Awake()
    {
        Instance = this;

        Toggle(false);
        if (navigationSubmitButton != null)
        {
            navigationSubmitButton.onClick.AddListener(OnSubmitClicked);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Toggle();
        }
    }

    private void OnSubmitClicked()
    {
        if (navigationInputField != null && sceneLoader != null)
        {
            this.Toggle(false);
            string userInput = navigationInputField.text;
            sceneLoader.LoadWikipediaArticle(userInput);
        }
    }
}
