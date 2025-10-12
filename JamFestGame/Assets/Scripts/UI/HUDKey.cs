using System;
using UnityEngine;
using UnityEngine.UI; // <-- Correct namespace

public class HUDKey : MonoBehaviour
{
    public AbilityType ability;
    public Sprite defaultIcon;
    public Sprite startingIcon;
    public Sprite pressedIcon;
    public string keyCode;
    public GameObject key;

    private void Start()
    {
        defaultIcon = key.GetComponent<Image>().sprite;
    }
    public void Update()
    {
        KeyCode parsedKey;
        if (Enum.TryParse(keyCode, out parsedKey))
        {
            if (Input.GetKeyDown(parsedKey))
            {
                key.GetComponent<Image>().sprite = pressedIcon;
            }
            if (Input.GetKeyUp(parsedKey))
            {
                key.GetComponent<Image>().sprite = startingIcon;
            }
        }
        else
        {
            // Optional: log a warning if the keyCode is invalid
            Debug.LogWarning($"HUDKey: '{keyCode}' is not a valid KeyCode.");
        }
    }

    public void RemoveKey()
    {
        key.GetComponent<Image>().sprite = defaultIcon;
    }
    public void SetKey()
    {
        key.GetComponent<Image>().sprite = startingIcon;
    }
}
