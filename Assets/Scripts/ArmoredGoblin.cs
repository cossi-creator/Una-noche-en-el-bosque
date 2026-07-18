using UnityEngine;

public class ArmoredGoblin : BaseEnemy
{
    protected override void Awake()
    {
        base.Awake();
        // Valores por defecto específicos de este tipo
        maxHealth = 120f;
        damage = 20;
        walkSpeed = 3f;
        runSpeed = 5f;
    }

    protected override void Start()
    {
        base.Start();
        // Asegurarse de que currentHealth refleje el maxHealth actualizado
        currentHealth = maxHealth;
    }
}
