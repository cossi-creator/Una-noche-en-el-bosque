using UnityEngine;

public class Troll : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 300f;
        damage = 50;
        walkSpeed = 1.5f;
        runSpeed = 3.5f;
        attackDistance = 2.5f;
    }

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
    }
}
