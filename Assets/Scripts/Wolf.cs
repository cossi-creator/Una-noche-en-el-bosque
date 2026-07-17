public class Wolf : BaseEnemy
{
    public Wolf()
    {
        maxHealth = 30f;   // Muy frágil
        damage = 5;        // Poco daño
        walkSpeed = 4f;
        runSpeed = 8f;     // Muy rápido
        visionRange = 20f; // Los lobos huelen/ven de más lejos
    }
}