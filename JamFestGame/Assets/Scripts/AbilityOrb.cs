using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityOrb : MonoBehaviour
{
    //public List<AbilityType> abilityPool = new List<AbilityType>();
    public AbilityType ability;
    public bool shuffleAbilities = true;


    public Sprite dash;
    public Sprite doubleJump;
    public Sprite glide;
    public Sprite teleport;
    public Sprite grapple;
    public Sprite speed;
    public Sprite hover;
    public Sprite shrink;

    public GameObject icon;
    private void Start()
    {
        Sprite icon2 = null;
        switch (ability)
        {
            case AbilityType.Dash:
                icon2 = dash;
                break;
            case AbilityType.DoubleJump:
                icon2 = doubleJump;
                break;
            case AbilityType.Glide:
                icon2 = glide;
                break;
            case AbilityType.Teleport:
                icon2 = teleport;
                break;
            case AbilityType.Grapple:
                icon2 = grapple;
                break;
            case AbilityType.SuperSpeed:
                icon2 = speed;
                break;
            case AbilityType.Hover:
                icon2 = hover;
                break;
            case AbilityType.Shrink:
                icon2 = shrink;
                break;
            default:
                icon2 = null;
                break;
         }
        print(icon);
        icon.GetComponent<SpriteRenderer>().sprite = icon2;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        SFXManager.Instance.Play(SFXManager.Instance.orbCollectClip);
        Abilities abilities = collision.gameObject.GetComponent<Abilities>();
        if (!abilities)
            return;

        if (abilities.HasAbility(ability))
        {
            if (shuffleAbilities)
                abilities.ResetAbilities();
            return;
        }

            abilities.AddAbility(ability);
        if (shuffleAbilities)
            abilities.ResetAbilities();


        /* 
        List<AbilityType> availableAbilities = new List<AbilityType>(abilityPool);

        AbilityType rolledAbility = AbilityType.None;
        bool found = false;

        while (availableAbilities.Count > 0 && !found)
        {
            int randomIndex = Random.Range(0, availableAbilities.Count);
            rolledAbility = availableAbilities[randomIndex];

            if (!abilities.HasAbility(rolledAbility))
            {
                abilities.AddAbility(rolledAbility);
                found = true;
            }
            else
            {
                // Remove the already owned ability and try again
                availableAbilities.RemoveAt(randomIndex);
            }
        }

        // Optionally, handle the case where no new ability could be given
        if (!found)
        {
            Debug.Log("Player already has all abilities in the pool.");
        }
        */
        Destroy(gameObject);
    }

}


