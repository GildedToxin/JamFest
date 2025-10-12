using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateKeyIcons(Dictionary<AbilityType, string> abilityLetters)
    {
        foreach(Transform child in transform.GetChild(0))
        {
            if(abilityLetters.ContainsKey(child.GetComponent<HUDKey>().ability))
            {
                var test = abilityLetters[child.GetComponent<HUDKey>().ability].ToUpper() + "_KEY";
                HUDKEY1 abilityData = Resources.Load<HUDKEY1>(test);
                print(abilityData);
                child.GetComponent<HUDKey>().startingIcon = abilityData.upIcon;
                child.GetComponent<HUDKey>().pressedIcon = abilityData.downIcon;
                child.GetComponent<HUDKey>().keyCode = abilityLetters[child.GetComponent<HUDKey>().ability];
                child.GetComponent<HUDKey>().SetKey();  
            }
            else
            {
                child.GetComponent<HUDKey>().keyCode = "";
                child.GetComponent<HUDKey>().RemoveKey();
            }
            
        }
    }
}
