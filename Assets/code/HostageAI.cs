using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class HostageAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public Transform safeZone;

    [Header("Settings")]
    public float interactRange = 3f;
    public KeyCode followKey = KeyCode.E;

    private bool isFollowing = false;
    private bool reachedSafeZone = false;
    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (reachedSafeZone) return; // Stop once reached safe zone

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!isFollowing && distanceToPlayer <= interactRange)
        {
            if (Input.GetKeyDown(followKey))
            {
                isFollowing = true;
                Debug.Log("Hostage is now following the player.");
            }
        }

        if (isFollowing)
        {
            agent.stoppingDistance = 2f;
            agent.SetDestination(player.position); // Check if reached safe zone

            float distanceToSafeZone = Vector3.Distance(transform.position, safeZone.position);
            if (distanceToSafeZone <= 2f)
            {
                reachedSafeZone = true;
                agent.stoppingDistance = 0f;
                agent.SetDestination(transform.position); // Stop moving
                Debug.Log("Safe zone reached");

                // Notify RescueCount
                if(RescueCount.instance != null)
                {
                    RescueCount.instance.addRescuedHostage();
                }

                StartCoroutine(DisappearAfterDelay(2f)); // disappear after 2 seconds
            }
        }
    }
    IEnumerator DisappearAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);

        if (safeZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(safeZone.position, 10f);
        }
    }
}