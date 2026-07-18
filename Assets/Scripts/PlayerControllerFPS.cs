using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerFPS : MonoBehaviour
{
    [Header("Stats")]
    [Tooltip("Vida máxima del jugador")]
    public int maxHealth = 400;
    [Tooltip("Daño base antes de buffs")]
    public int baseDamage = 20;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2f;
    public float jumpHeight = 1.6f;
    public float gravity = -9.81f;
    public float crouchHeight = 1.0f;
    public float standHeight = 2.0f;
    public float heightSmooth = 8f;

    [Header("Combat")]
    public Animator animator;
    [Tooltip("Collider trigger de la maza (isTrigger = true). Se activa por eventos de animación)")]
    public Collider weaponCollider;
    [Tooltip("LayerMask que identifica a los enemigos")]
    public LayerMask enemyLayer;
    [Tooltip("Porcentaje de daño reducido cuando bloquea (ej: 80 = reduce 80%)")]
    public int shieldBlockPercent = 80;

    [Header("Camera / Audio")]
    public Transform cameraPivot;
    public AudioSource audioSource;
    public AudioClip attackSfx;
    public AudioClip hitSfx;
    public AudioClip deathSfx;

    [Header("Mouse Look")]
    [Tooltip("Sensibilidad horizontal/vertical del mouse")]
    public float mouseSensitivity = 2f;
    [Tooltip("Límite de rotación vertical (arriba/abajo) en grados")]
    public float minPitch = -80f;
    public float maxPitch = 80f;
    [Tooltip("Si true, bloquea y oculta el cursor al iniciar")]
    public bool lockCursor = true;

    private float pitch = 0f;
    private CharacterController cc;
    private float verticalVelocity = 0f;
    private float currentSpeed = 0f;
    private bool isCrouching = false;
    private bool isRunning = false;
    private bool isBlocking = false;
    private bool isDead = false;

    private int currentHealth;
    private int strengthStacks = 0;
    [Tooltip("Porcentaje por stack (0.25 = +25%)")]
    public float perStackPercent = 0.25f;
    [Tooltip("Máximo de stacks acumulables")]
    public int maxStrengthStacks = 3;

    public int CurrentHealth => currentHealth;
    public int StrengthStacks => strengthStacks;
    public float DamageMultiplier => 1f + strengthStacks * perStackPercent;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (isDead) return;

        HandleMouseLook();
        HandleMovement();
        HandleCombat();
        ApplyGravity();
        UpdateAnimator();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Eje X (Izquierda/Derecha): Rota el cilindro entero del jugador
        transform.Rotate(Vector3.up * mouseX);

        // Eje Y (Arriba/Abajo): Acumula la rotación
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Aplicamos la rotación vertical SOLO al pivote (que contiene cámara y armas)
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        isCrouching = Input.GetKey(KeyCode.LeftControl);
        isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching && inputZ > 0.1f;

        float targetSpeed = walkSpeed;
        if (isRunning) targetSpeed = runSpeed;
        if (isCrouching) targetSpeed = crouchSpeed;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f);

        Vector3 move = (transform.forward * inputZ + transform.right * inputX).normalized;
        cc.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && cc.isGrounded && !isCrouching)
        {
            verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * heightSmooth);
        Vector3 center = cc.center;
        center.y = cc.height / 2f;
        cc.center = center;
    }

    void HandleCombat()
    {
        // Click Izquierdo: Llama al Trigger de la maza que tienes en el Animator
        if (Input.GetButtonDown("Fire1") && !isBlocking)
        {
            if (animator != null) animator.SetTrigger("AttackMace");
            if (audioSource != null && attackSfx != null) audioSource.PlayOneShot(attackSfx);
        }

        // Click Derecho: Llama al Trigger del escudo que tienes en el Animator
        if (Input.GetButtonDown("Fire2"))
        {
            if (animator != null) animator.SetTrigger("AttackShield");
        }
    }

    void ApplyGravity()
    {
        if (cc.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        cc.Move(new Vector3(0, verticalVelocity, 0) * Time.deltaTime);
    }

    // --- LÓGICA DE DAÑO Y COLLIDERS INTACTA ---
    public void EnableWeaponHit()
    {
        if (weaponCollider != null) weaponCollider.enabled = true;
    }

    public void DisableWeaponHit()
    {
        if (weaponCollider != null) weaponCollider.enabled = false;
    }

    public void OnWeaponHitColliderEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        var baseEnemy = other.GetComponentInParent<BaseEnemy>();
        if (baseEnemy != null)
        {
            baseEnemy.TakeDamage(GetDamage());
        }
    }

    public int GetDamage()
    {
        return Mathf.CeilToInt(baseDamage * (1f + strengthStacks * perStackPercent));
    }

    public void HealToFull()
    {
        currentHealth = maxHealth;
        // Si más adelante agregas una animación de curar, la pones aquí
    }

    public void ApplyPermanentStrengthBuff(float percent)
    {
        ApplyPermanentStrengthBuff(percent, -1);
    }

    public void ApplyPermanentStrengthBuff(float percent, int maxStacksOverride)
    {
        int limit = (maxStacksOverride > 0) ? maxStacksOverride : maxStrengthStacks;
        if (strengthStacks >= limit) return;

        strengthStacks++;
        Debug.Log($"Ruina activada: stacks = {strengthStacks}, multiplicador = {1f + strengthStacks * percent}");
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        int finalDamage = amount;
        if (isBlocking)
        {
            finalDamage = Mathf.CeilToInt(amount * (100 - shieldBlockPercent) / 100f);
        }

        currentHealth -= finalDamage;

        // Aquí llamamos al parámetro exacto "Damage" de tu captura
        if (animator != null) animator.SetTrigger("Damage");

        if (audioSource != null && hitSfx != null) audioSource.PlayOneShot(hitSfx);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // Si luego agregas el parámetro "Dead" al animator, lo puedes descomentar:
        // if (animator != null) animator.SetBool("Dead", true);

        if (audioSource != null && deathSfx != null) audioSource.PlayOneShot(deathSfx);

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.JugadorMurio();
        }

        if (cc != null) cc.enabled = false;
    }

    // --- ANIMATOR AJUSTADO SOLO A TUS CAPTURAS ---
    void UpdateAnimator()
    {
        if (animator != null)
        {
            // Solo calcula la velocidad y actualiza el único parámetro de movimiento que tienes: MoveSpeed
            Vector3 horizontalVel = new Vector3(cc.velocity.x, 0, cc.velocity.z);
            float speed = horizontalVel.magnitude;
            animator.SetFloat("MoveSpeed", speed);
        }
    }
}