using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour, IDamageable
{
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;

    [Header("Stats")]
    public float health = 100f;
    public float sightRange = 15f;
    public float attackRange = 10f;
    public float fieldOfView = 90f;
    public float timeBetweenAttacks = 2f;
    public float attackDamage = 10f;

    private bool alreadyAttacked;
    private Vector3 lastKnownPosition;
    private float losePlayerTimer;
    private float losePlayerDelay = 3f;

    private Ragdoll ragdoll;

    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;

    private void Awake()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
        if (!ragdoll) ragdoll = GetComponent<Ragdoll>();
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                LookForPlayer();
                break;
            case State.Chase:
                Chase();
                LookForPlayer();
                break;
            case State.Attack:
                Attack();
                LookForPlayer();
                break;
        }
    }

    private void LookForPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        bool inFOV = angle < fieldOfView * 0.5f;
        bool canSee = false;

        if (inFOV && distance < sightRange)
        {
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out RaycastHit hit, sightRange))
            {
                if (hit.transform.CompareTag("Player"))
                    canSee = true;
            }
        }

        if (canSee)
        {
            lastKnownPosition = player.position;
            losePlayerTimer = 0f;
            currentState = distance <= attackRange ? State.Attack : State.Chase;
        }
        else if (currentState != State.Patrol)
        {
            losePlayerTimer += Time.deltaTime;
            if (losePlayerTimer >= losePlayerDelay)
                currentState = State.Patrol;
        }
    }

    private void Patrol()
    {
        if (!agent.hasPath || agent.remainingDistance < 1f)
        {
            Vector3 randomDir = Random.insideUnitSphere * 10f + transform.position;
            if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
    }

    private void Chase()
    {
        agent.SetDestination(lastKnownPosition);
        FacePlayer();
    }

    private void Attack()
    {
        agent.SetDestination(transform.position);
        FacePlayer();

        if (!alreadyAttacked)
        {
            IDamageable dmg = player.GetComponent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage((int)attackDamage);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void FacePlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);
    }

    private void ResetAttack() => alreadyAttacked = false;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"{name} took {amount} damage. Remaining health: {health}");
        StartCoroutine(FlashOnHit());

        if (health > 0)
        {
            currentState = State.Chase;
            lastKnownPosition = player.position;
            return;
        }

        Die();
    }

    private IEnumerator FlashOnHit()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = Color.white;
        }
        yield return new WaitForSeconds(0.1f);
        foreach (var r in renderers)
        {
            if (r.material.HasProperty("_Color"))
                r.material.color = Color.red;
        }
    }

    private void Die()
    {
        if (agent != null) agent.enabled = false;
        if (animator != null) animator.enabled = false;
        if (ragdoll != null) ragdoll.ActivateRagdoll();

        Destroy(gameObject, 10f);
    }
}
