using UnityEngine;
using UnityEngine.AI;

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats de Combate")]
    public float maxHealth = 100f;
    protected float currentHealth;
    public int damage = 10;

    [Header("Movimiento")]
    public float walkSpeed = 3f;
    public float runSpeed = 4f;

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

    [Header("Búsqueda y Memoria")]
    public float searchTime = 5f;
    protected float searchTimer = 0f;
    protected bool isSearchingPlayer = false;
    protected Vector3 lastKnownPosition;

    [Header("Ataque")]
    public float attackDistance = 1.5f;
    public float timeBetweenAttacks = 1.5f;
    protected float attackTimer = 0f;

    protected NavMeshAgent agent;
    protected bool seesPlayer = false;
    protected Animator anim;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth; // Inicializa la vida
    }

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;

        CheckVision();

        if (seesPlayer)
        {
            isSearchingPlayer = true;
            searchTimer = searchTime;
            lastKnownPosition = player.position;

            Chase();
        }
        else if (isSearchingPlayer)
        {
            SearchLastPosition();
        }
        else
        {
            Patrol();
        }

        UpdateAnimations();
    }

    protected virtual void Patrol()
    {
        if (waypoints.Length == 0)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.speed = walkSpeed;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            Transform puntoActual = waypoints[currentWaypointIndex];

            // Rotación suave hacia el waypoint
            transform.rotation = Quaternion.Slerp(transform.rotation, puntoActual.rotation, Time.deltaTime * 5f);

            if (waitTimer >= waitAtWaypointTime)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                waitTimer = 0f;
            }
        }

        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    protected virtual void Chase()
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            if (attackTimer >= timeBetweenAttacks)
            {
                Attack();
                attackTimer = 0f;
            }
        }
    }

    protected virtual void CheckVision()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
        Vector3 rayDestination = player.position + Vector3.up * 1.5f;
        Vector3 directionToPlayer = (rayDestination - rayOrigin).normalized;
        float distanceToPlayer = Vector3.Distance(rayOrigin, rayDestination);

        if (distanceToPlayer > visionRange)
        {
            seesPlayer = false;
            return;
        }

        bool wallBlocking = Physics.Raycast(rayOrigin, directionToPlayer, distanceToPlayer, obstacleLayer);

        if (!wallBlocking)
        {
            if (seesPlayer) return;

            float currentAngle = Vector3.Angle(transform.forward, (player.position - transform.position).normalized);
            if (currentAngle < visionAngle)
            {
                seesPlayer = true;
                return;
            }
        }

        seesPlayer = false;
    }

    protected virtual void SearchLastPosition()
    {
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(lastKnownPosition);

        bool reachedDestination = agent.remainingDistance <= agent.stoppingDistance;
        bool pathBlocked = agent.pathStatus == NavMeshPathStatus.PathPartial;

        if (!agent.pathPending && (reachedDestination || pathBlocked))
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0) isSearchingPlayer = false;
        }
    }

    protected virtual void Attack()
    {
        Debug.Log(gameObject.name + " atacó al jugador haciendo " + damage + " de daño.");
        if (anim != null) anim.SetTrigger("Atacar");
        // Listo para conectar tu futuro sistema de daño al jugador
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " recibió daño. Vida: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log(gameObject.name + " murió.");
        Destroy(gameObject); // O podés reproducir una animación de muerte
    }

    protected virtual void UpdateAnimations()
    {
        if (agent != null && anim != null)
        {
            bool isMoving = !agent.isStopped && !agent.pathPending && agent.remainingDistance > agent.stoppingDistance;
            bool isWalking = isMoving && agent.speed == walkSpeed;
            bool isChasing = isMoving && agent.speed == runSpeed;

            anim.SetBool("Caminando", isWalking);
            anim.SetBool("Persiguiendo", isChasing);
        }
    }
}