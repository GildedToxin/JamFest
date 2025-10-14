using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public bool refreshAbilitiesOnStart = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(refreshAbilitiesOnStart)
        FindAnyObjectByType<Abilities>().ResetAbilities();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateKeyIcons(Dictionary<AbilityType, string> abilityLetters)
    {
        foreach(Transform child in transform.GetChild(0))
        {
            var hudKey = child.GetComponent<HUDKey>();
            if (hudKey == null)
            {
                Debug.LogWarning($"HUDController: Child '{child.name}' does not have a HUDKey component.");
                continue;
            }

            if (abilityLetters.ContainsKey(hudKey.ability))
            {
                var test = abilityLetters[hudKey.ability].ToUpper() + "_KEY";
                HUDKEY1 abilityData = Resources.Load<HUDKEY1>(test);
                if(abilityData == null)
                {
                    Debug.LogWarning($"HUDController: Resource '{test}' not found.");
                    continue;
                }
                hudKey.startingIcon = abilityData.upIcon;
                hudKey.pressedIcon = abilityData.downIcon;
                hudKey.keyCode = abilityLetters[hudKey.ability];
                hudKey.SetKey();  
            }
            else
            {
                hudKey.keyCode = "";
                hudKey.RemoveKey();
            }
        }
    }
}
