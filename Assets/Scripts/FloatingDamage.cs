using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    [Header("General")]
    public Vector3 textMove;
    public float animationDuration = 1f;
    [Header("Different color for damage")]
    public Color hitNormalColor;
    public Color hitCriticalColor;
    public Color hitSpecialColor;

    private bool drawDamage = false;
    private TextMesh damageText;
    private Animator damageAnimator;
    private float AwakeTime = 0f;

    void Awake()
    {
        damageText = GetComponentInChildren<TextMesh>();
        damageAnimator = GetComponentInChildren<Animator>();
        gameObject.SetActive(false);
    }

    public void StartDamage(int damageType, int damage, Vector3 position)
    {
        transform.position = position + textMove;
        switch (damageType)
        {
            case 0:
                damageText.color = hitNormalColor;
                break;
            case 1:
                damageText.color = hitCriticalColor;
                break;
            default:
                damageText.color = hitSpecialColor;
                break;
        }
        damageText.text = damage.ToString();
        AwakeTime = 0f;
        gameObject.SetActive(true);
        drawDamage = true;
        damageAnimator.Play("Base Layer.FloatingText");
    }

    public void StopDamage()
    {
        drawDamage = false;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (drawDamage)
        {
            AwakeTime += Time.deltaTime;
            if (AwakeTime >= animationDuration)
            {
                StopDamage();
                return;
            }
            damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, Mathf.Lerp(1f, 0f, AwakeTime / animationDuration));
        }
    }
}
