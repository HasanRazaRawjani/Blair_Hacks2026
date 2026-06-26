using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;
    public float maxHealth = 100f;
    public float currentHealth;
    bool isDead = false;
    public System.Action<float> onHealthChanged;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }   

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = currentHealth / 100;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        onHealthChanged?.Invoke(currentHealth);
        if (currentHealth <= 0) Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    public void Die()
    {
        isDead = true;
        //disable PlayerMovement; disable MouseLook; show death screen UI
    }

}
