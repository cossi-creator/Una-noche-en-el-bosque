public class Goblin : BaseEnemy
{
    public Goblin()
    {
        maxHealth = 60f;   // Vida estándar
        damage = 15;       // Más daño que el lobo
        walkSpeed = 3f;
        runSpeed = 5f;     // Velocidad normal
    }
}