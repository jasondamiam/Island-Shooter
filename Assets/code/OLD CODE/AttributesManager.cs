using UnityEngine;


[CreateAssetMenu(fileName = "Attributes", menuName = "Attributesblock")]
public class AttributesManager : ScriptableObject
{
    // carakter names en text info
    public new string name;
    //health values
    public int health =  100;
    public int maxHealth;

    public int attack;
    public int damage = 10;

    // spell values
    public int maxSpellPoints = 100;
    public int currentSpellPoints;
    //Regenrate values of spell
    public int regenerationAmount = 1;
    public float regenerationInterval = 1f;

    //Stamina values
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegenRate = 10f; // Stamina regeneration rate per second
    public float sprintStaminaCost = 20f; // Stamina cost for sprintin

    public void TakeDamage(int damage)
    { 
        health -= damage;
    }
    public void DealDamage(GameObject target)
    {
        var atm = target .GetComponent<AttributesManager>();
        if (atm != null ) 
        { 
            atm.TakeDamage(attack);
        }
    }
}
