using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ThrowSwordColliders : MonoBehaviour
{
    private float throwDamage() => swordScript.throwDamage();
    private bool TriggerCalled() => swordScript.triggerCalled;
    private bool isThrown() => swordScript.isThrown;

    private ThrowSword swordScript;
    void Start()
    {
        swordScript = GetComponentInParent<ThrowSword>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isThrown() && !TriggerCalled() && other.tag == "Ennemy")
        {
            other.SendMessage("TakeDamage", throwDamage());
        }
    }

    void Update()
    {
    }
}
