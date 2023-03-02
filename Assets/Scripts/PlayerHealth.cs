using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("General")]
    [Range(0.5f, 3f)] public float regenerationRate = 1f;
    public bool regeneration = true;
    [Header("UI information")]
    public Text healthText;
    public Image healthBar;
    public Color fullHealth;
    public Color lowHealth;
    private float health = 100;
    public bool isDead { get; private set; }
    public Vector3 initialSpawnPoint;
    private float lerpSpeed = 4f;
    private TeleportPlayer tpPlayerScript;
    void Start()
    {
        tpPlayerScript = GetComponent<TeleportPlayer>();
    }

    public void killPlayer()
    {
        isDead = true;
        tpPlayerScript.resetPlayer(initialSpawnPoint);
        Invoke(nameof(revivePlayer), 0.5f);
    }

    public void revivePlayer()
    {
        health = 100;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (healthBar.fillAmount == 1) healthBar.fillAmount = 0.998f;
    }

    void Update()
    {
        health = (health <= 0) ? 0 : (regeneration) ? health + regenerationRate * Time.deltaTime : health;
        lerpSpeed = (health <= 0) ? 20 : 4;
        health = (health >= 100) ? 100 : health;
        healthBar.fillAmount = (healthBar.fillAmount >= 0.999f) ? 1 : Mathf.Lerp(healthBar.fillAmount, health / 100, (lerpSpeed * Time.deltaTime));
        healthText.text = ((int)(healthBar.fillAmount * 100)) + "%";
        healthText.color = Color.Lerp(lowHealth, fullHealth, healthBar.fillAmount / 1);
        healthBar.color = Color.Lerp(lowHealth, fullHealth, healthBar.fillAmount / 1);
        if (health == 0 && !isDead)
        {
            killPlayer();
        }
    }
}
