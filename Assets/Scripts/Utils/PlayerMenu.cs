using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    public TMPro.TMP_InputField navigationInputField;

    public Button navigationSubmitButton;

    public GameObject menuWindowRoot;

    public SceneLoader sceneLoader;

    private bool isOpen = false;

    void Awake()
    {
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
            Toggle(!isOpen);
        }
    }

    private void Toggle(bool enabled)
    {
        isOpen = enabled;
        if (menuWindowRoot != null)
        {
            menuWindowRoot.SetActive(isOpen);
        }
        // Show cursor when menu is open, hide when closed
        if (isOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
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
