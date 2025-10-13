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
        if (!collision.CompareTag("Player"))
            return;

        Abilities abilities = collision.GetComponent<Abilities>();
        if (!abilities)
            return;

        if (SFXManager.Instance.orbCollectClip != null)
        {
            SFXManager.Instance.orbCollectClip.Play();
        }


        if (!abilities.HasAbility(ability))
            abilities.AddAbility(ability);

        if (shuffleAbilities)
            abilities.ResetAbilities();

        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
        Destroy(gameObject);
    }

}


