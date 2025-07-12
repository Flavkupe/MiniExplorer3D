using UnityEngine;

public class InteractionWindow : BaseWindow
{
    public TMPro.TextMeshProUGUI ContentText;

    public TMPro.TextMeshProUGUI TitleText;

    public GameObject ScrollView;

    public ImagePanel ImagePanel;

    public static InteractionWindow Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        this.Close();
    }

    public void SetImage(LevelImage imageData, bool open = true)
    {
        if (imageData == null)
        {
            Debug.LogError("InteractionWindow: SetImage called with null LevelImage.");
            return;
        }

        this.ImagePanel.gameObject.SetActive(true);
        this.ScrollView.gameObject.SetActive(false);
        this.TitleText.gameObject.SetActive(false);

        if (open)
        {
            this.Open();   
        }

        if (this.ImagePanel != null)
        {
            this.ImagePanel.SetImage(imageData);
        }
    }

    public void SetText(LocationTextData textData, bool open = true)
    {
        if (textData == null)
        {
            Debug.LogError("InteractionWindow: SetText called with null LocationTextData.");
            return;
        }

        this.ImagePanel.gameObject.SetActive(false);
        this.ScrollView.gameObject.SetActive(true);
        this.TitleText.gameObject.SetActive(true);

        if (open)
        {
            this.Open();   
        }

        if (this.ContentText != null)
        {
            this.ContentText.text = textData.Text;
        }

        if (this.TitleText != null)
        {
            this.TitleText.text = textData.Title;
        }
    }
}
