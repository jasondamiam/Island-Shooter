using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FearFleeAgent : MonoBehaviour
{
    [Header("References")]
    public Transform player;                // target om van weg te rennen
    public Animator animator;               // optioneel: run/idle anims

    [Header("Perception")]
    public float detectRadius = 15f;        // binnen deze radius wordt player “eng”
    [Range(0f, 180f)] public float fov = 180f; // optioneel: gezichtsveld (half-angle)
    public bool requireLineOfSight = true;  // alleen rennen als we zicht hebben
    public LayerMask losObstacles = ~0;     // lagen die zicht blokkeren

    [Header("Flee Behavior")]
    public float fleeDistance = 12f;        // afstand van huidige positie naar vluchtpunt
    public float safeRadius = 18f;          // als speler verder is dan dit: kalmeren
    public float repathInterval = 0.35f;    // hoe vaak nieuwe bestemming kiezen tijdens vlucht
    public int fleeSamples = 8;           // hoeveel alternatieve punten proberen
    public float sampleSpread = 20f;        // extra willekeur rond basis vluchtrichting

    [Header("Wander (als veilig)")]
    public bool enableWander = true;
    public float wanderRadius = 6f;
    public float wanderInterval = 3f;

    [Header("Animation Params (opt)")]
    public string animBoolIsRunning = "isRunning";
    public string animFloatSpeed = "speed";

    private NavMeshAgent agent;
    private float nextRepathTime;
    private float nextWanderTime;

    private enum State { Calm, Flee }
    private State state = State.Calm;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!animator) animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool seesPlayer = dist <= detectRadius && InFOV() && (!requireLineOfSight || HasLOS());

        // FSM: Calm <-> Flee
        if (state == State.Calm)
        {
            if (seesPlayer)
            {
                state = State.Flee;
                PickFleeDestination();
                UpdateAnim(true);
            }
            else if (enableWander)
            {
                WanderTick();
            }
        }
        else // Flee
        {
            if (dist > safeRadius) // veilig genoeg: kalm worden
            {
                state = State.Calm;
                UpdateAnim(false);
            }
            else
            {
                if (Time.time >= nextRepathTime || ReachedDestination())
                {
                    PickFleeDestination();
                }
            }
        }

        UpdateAnimSpeed();
        FaceMoveDirection();
    }

    // ----------------- Core logic -----------------
    void PickFleeDestination()
    {
        nextRepathTime = Time.time + repathInterval;

        Vector3 myPos = transform.position;
        Vector3 away = (myPos - player.position);
        away.y = 0f;
        if (away.sqrMagnitude < 0.0001f) away = Random.insideUnitSphere; // edge case
        away.Normalize();

        // Basis vluchtpunt
        Vector3 target = myPos + away * fleeDistance;

        if (TrySampleOnNavmesh(target, out Vector3 navPos))
        {
            agent.SetDestination(navPos);
            return;
        }

        // Fallback: probeer ring van alternatieve punten rond “away”-richting
        Vector3 best = myPos;
        bool found = false;
        for (int i = 0; i < fleeSamples; i++)
        {
            // kleine willekeur rondom de “away” richting
            Vector2 rnd = Random.insideUnitCircle * Mathf.Deg2Rad * sampleSpread;
            Quaternion q = Quaternion.Euler(0f, rnd.x * Mathf.Rad2Deg, 0f);
            Vector3 dir = (q * away).normalized;
            Vector3 candidate = myPos + dir * fleeDistance;

            if (TrySampleOnNavmesh(candidate, out Vector3 cNav))
            {
                best = cNav;
                found = true;
                break;
            }
        }

        if (found) agent.SetDestination(best);
        else
        {
            // als alles faalt: zet een stapje achteruit
            Vector3 tiny = myPos + away * Mathf.Min(3f, fleeDistance * 0.25f);
            if (TrySampleOnNavmesh(tiny, out Vector3 tinyNav))
                agent.SetDestination(tinyNav);
        }
    }

    void WanderTick()
    {
        if (Time.time < nextWanderTime) return;
        nextWanderTime = Time.time + wanderInterval;

        Vector2 rnd = Random.insideUnitCircle.normalized * wanderRadius;
        Vector3 candidate = transform.position + new Vector3(rnd.x, 0f, rnd.y);

        if (TrySampleOnNavmesh(candidate, out Vector3 navPos))
            agent.SetDestination(navPos);
    }

    bool TrySampleOnNavmesh(Vector3 point, out Vector3 navPos)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            navPos = hit.position;
            return true;
        }
        navPos = point;
        return false;
    }

    bool ReachedDestination()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            return true;
        return false;
    }

    // ----------------- Perception helpers -----------------
    bool InFOV()
    {
        if (fov >= 179.9f) return true; // praktisch 360°
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        Vector3 fwd = transform.forward;
        return Vector3.Angle(fwd, toPlayer) <= fov;
    }

    bool HasLOS()
    {
        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 chest = player.position + Vector3.up * 1.2f;
        Vector3 dir = (chest - eye).normalized;
        float d = Vector3.Distance(eye, chest);

        if (Physics.Raycast(eye, dir, out RaycastHit hit, d, losObstacles, QueryTriggerInteraction.Ignore))
        {
            // alleen LOS als we echt de player raken/zien
            return hit.transform == player || hit.transform.IsChildOf(player);
        }
        return true; // niets geraakt: vrij zicht
    }

    // ----------------- Visual polish -----------------
    void FaceMoveDirection()
    {
        // Optioneel: laat model subtiel meedraaien met velocity
        Vector3 vel = agent.desiredVelocity;
        vel.y = 0f;
        if (vel.sqrMagnitude > 0.01f)
        {
            Quaternion look = Quaternion.LookRotation(vel.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 7f);
        }
    }

    void UpdateAnim(bool running)
    {
        if (!animator) return;
        if (!string.IsNullOrEmpty(animBoolIsRunning))
            animator.SetBool(animBoolIsRunning, running);
    }

    void UpdateAnimSpeed()
    {
        if (!animator || string.IsNullOrEmpty(animFloatSpeed)) return;
        float speed01 = Mathf.Clamp01(agent.velocity.magnitude / Mathf.Max(0.01f, agent.speed));
        animator.SetFloat(animFloatSpeed, speed01, 0.15f, Time.deltaTime);
    }

    // ----------------- Debug -----------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, safeRadius);
    }
}
