public class ArmoredGoblin : BaseEnemy
{
    public ArmoredGoblin()
    {
        maxHealth = 120f;  // El doble de duros que los normales
        damage = 20;       // Pegan un poco más fuerte
        walkSpeed = 3f;
        runSpeed = 5f;     // Misma velocidad
    }
}