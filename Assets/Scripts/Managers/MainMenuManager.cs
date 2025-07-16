using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

namespace Managers
{
    public class MainMenuManager : MonoBehaviour
    {
        // Call this from a UI button to start the game
        public void StartGame()
        {
            SceneManager.LoadScene("Area");
        }

        // Assign these in the Inspector
        public GameObject MainPage;
        public GameObject ChangelogPage;
        public GameObject AboutPage;

        public TextMeshProUGUI VersionText;

        private Dictionary<string, GameObject> pages;
        private string currentPage;

        private Changelist changelist;

        private void Start()
        {
            pages = new Dictionary<string, GameObject>
            {
                { "MainPage", MainPage },
                { "Changelog", ChangelogPage },
                { "About", AboutPage }
            };
            ShowPage("MainPage");

            try
            {
                changelist = ChangelistManager.Instance.LoadChangelist();

                var version = changelist?.sections?.Count > 0 
                    ? $"v{changelist.sections[0].version}"
                    : "v0.0.?";
                VersionText.text = version;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        // Call this to show a specific page by name
        public void ShowPage(string pageName)
        {
            foreach (var kvp in pages)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.SetActive(kvp.Key == pageName);
                }
            }
            currentPage = pageName;
        }

        // Convenience methods for UI buttons
        public void ShowMainPage() => ShowPage("MainPage");
        public void ShowChangelogPage() => ShowPage("Changelog");
        public void ShowAboutPage() => ShowPage("About");
    }
}
