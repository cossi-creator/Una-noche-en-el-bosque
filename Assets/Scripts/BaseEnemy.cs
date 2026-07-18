using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    protected float currentHealth;
    public int damage = 10;

    [Header("Movimiento")]
    public float walkSpeed = 3f;
    public float runSpeed = 4f;

    [Header("Animaciones")]
    public string triggerAtaque = "attack01";

    [Header("Patrullaje")]
    public Transform[] waypoints;
    public float waitAtWaypointTime = 2f;
    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;

    [Header("Visión")]
    public float visionRange = 15f;
    public float visionAngle = 45f;
    public LayerMask obstacleLayer;
    public Transform player;

    [Header("Ataque")]
    public float attackDistance = 1.5f;
    public float timeBetweenAttacks = 1.5f;
    private float attackTimer = 0f;

    protected NavMeshAgent agent;
    protected Animator anim;

    protected virtual void Awake()
    {
        // Componentes
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // Auto-assign del player si no fue arrastrado en el inspector
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        // Seguridad: si agent es null, desactivar navegación para evitar NRE posteriores
        if (agent == null)
        {
            Debug.LogWarning($"{name}: NavMeshAgent no encontrado. Deshabilitando navegación.");
        }
    }

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;
        // Protecciones
        if (player == null)
        {
            // No hay player asignado: no intentar visión/ataque
            return;
        }

        CheckVisionAndAct();
        UpdateAnimations();
    }

    private void CheckVisionAndAct()
    {
        // Protecciones extra
        if (player == null) return;
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (player.position - rayOrigin).normalized;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= visionRange && !Physics.Raycast(rayOrigin, directionToPlayer, dist, obstacleLayer) && Vector3.Angle(transform.forward, directionToPlayer) < visionAngle)
        {
            ChaseAndAttack(dist);
        }
        else
        {
            Patrol();
        }
    }

    protected virtual void Patrol()
    {
        if (agent == null) return;
        if (waypoints == null || waypoints.Length == 0) return;

        agent.speed = walkSpeed;
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitAtWaypointTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                waitTimer = 0f;
            }
        }
    }

    protected virtual void ChaseAndAttack(float dist)
    {
        if (agent == null) return;
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (dist <= attackDistance && attackTimer >= timeBetweenAttacks)
        {
            if (anim != null) anim.SetTrigger(triggerAtaque);

            var playerScript = player.GetComponent<PlayerControllerFPS>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }

            attackTimer = 0f;
        }
    }

    protected virtual void UpdateAnimations()
    {
        if (agent == null || anim == null) return;
        bool isMoving = agent.velocity.magnitude > 0.1f;
        anim.SetBool("walk", isMoving && Mathf.Approximately(agent.speed, walkSpeed));
        anim.SetBool("run", isMoving && Mathf.Approximately(agent.speed, runSpeed));
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (anim != null) anim.SetTrigger("damage");
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        if (anim != null) anim.SetTrigger("dead");
        if (agent != null) agent.isStopped = true;

        // Desactivar colliders de forma segura en el objeto y en hijos
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
        {
            c.enabled = false;
        }

        this.enabled = false;
        Destroy(gameObject, 3f);
    }
}
