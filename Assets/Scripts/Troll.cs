public class Troll : BaseEnemy
{
    public Troll()
    {
        maxHealth = 300f;  // Un tanque
        damage = 50;       // Te puede matar de 2 golpes
        walkSpeed = 1.5f;  // Muy lentos caminando
        runSpeed = 3.5f;   // Apenas "corren"
        attackDistance = 2.5f; // Tienen más alcance por su tamaño
    }
}