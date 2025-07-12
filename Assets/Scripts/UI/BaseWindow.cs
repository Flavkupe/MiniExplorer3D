

using UnityEngine;
using UnityEngine.UI;

public abstract class BaseWindow : MonoBehaviour
{
    public bool IsOpen => this.gameObject.activeSelf;

    public virtual void Open()
    {
        if (IsOpen)
        {
            return;
        }

        this.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        OnOpen();
    }
    public virtual void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        this.gameObject.SetActive(false);
        OnClose();
    }

    public virtual void Toggle(bool enabled)
    {
        if (this.IsOpen == enabled)
        {
            return;
        }

        if (enabled)
        {
            this.Open();
        }
        else
        {
            this.Close();
        }
    }

    public virtual void Toggle()
    {
        this.Toggle(!this.IsOpen);
    }

    protected virtual void OnOpen() {}
    protected virtual void OnClose() {}
}