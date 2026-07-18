using UnityEngine;

public class Goblin : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 60f;
        damage = 15;
        walkSpeed = 3f;
        runSpeed = 5f;
    }

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
    }
}
