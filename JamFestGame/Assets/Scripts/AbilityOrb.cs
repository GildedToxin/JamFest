using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AbilityOrb : MonoBehaviour
{
    //public List<AbilityType> abilityPool = new List<AbilityType>();
    public AbilityType ability;
    public bool shuffleAbilities = true;

    private void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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


