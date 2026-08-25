using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button myButton;
    public GameObject MenuEmpty;
    public bool GameStart = false;

    void Start()
    {
        if (myButton != null)
        {
            // Register the method to be called when the button is pressed
            myButton.onClick.AddListener(OnButtonPressed);
        }
    }

    void OnButtonPressed()
    {
        Debug.Log("TextMeshPro Button was pressed!");
        GameStart = true;
        Destroy(MenuEmpty);

        // Optional: If you need to read or change the text on that button:
        TMP_Text buttonText = myButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            Debug.Log("Button Text is: " + buttonText.text);
        }

    }

    void OnDestroy()
    {
        // Good practice: remove the listener when the object is destroyed to prevent memory leaks
        if (myButton != null)
        {
            myButton.onClick.RemoveListener(OnButtonPressed);
        }
    }
}
