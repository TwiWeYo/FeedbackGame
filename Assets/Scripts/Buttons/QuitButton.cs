using System;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Not attached to a button");
            return;
        }

        button.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        Application.Quit();
    }
}
