using UnityEngine;

public class Wolf : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        maxHealth = 30f;
        damage = 5;
        walkSpeed = 4f;
        runSpeed = 8f;
        visionRange = 20f;
    }

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
    }
}
