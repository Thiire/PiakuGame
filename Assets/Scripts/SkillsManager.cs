using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillsManager : MonoBehaviour
{
    [Header("SkillsInstance")]
    public static SkillsManager SharedInstance;

    [Header("Skills")]
    [Space]
    [Header("Movement")]
    public bool Jump = false;
    public bool DoubleJump = false;
    public bool WallRide = false;
    [Header("Teleportation")]
    public bool Teleport = false;
    [Header("Weapons")]
    public bool Sword = false;
    public bool SwordComboAttack = false;
    public bool SwordThrow = false;

    void Awake()
    {
        SharedInstance = this;
    }

    void Update()
    {

    }
}
