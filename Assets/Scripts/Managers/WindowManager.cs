using UnityEngine;

public class WindowManager : MonoBehaviour
{
    public static WindowManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool IsAnyWindowOpen()
    {
        return DevWindow.Instance != null && DevWindow.Instance.IsOpen ||
               PlayerMenu.Instance != null && PlayerMenu.Instance.IsOpen ||
               InteractionWindow.Instance != null && InteractionWindow.Instance.IsOpen;
    }

    void Update()
    {
        // DevWindow: BackQuote (`)
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            var open = DevWindow.Instance?.IsOpen ?? false;
            CloseAllWindows();
            if (DevWindow.Instance != null)
            {
                DevWindow.Instance.Toggle(!open);
            }
        }

        // PlayerMenu: Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var open = PlayerMenu.Instance?.IsOpen ?? false;
            CloseAllWindows();
            if (PlayerMenu.Instance != null)
            {
                PlayerMenu.Instance.Toggle(!open);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllWindows();
        }
    }

    private void CloseAllWindows()
    {
        if (DevWindow.Instance != null)
        {
            DevWindow.Instance.Close();
        }
        if (PlayerMenu.Instance != null)
        {
            PlayerMenu.Instance.Close();
        }
        if (InteractionWindow.Instance != null)
        {
            InteractionWindow.Instance.Close();
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
