using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;



public class EnemyAI : MonoBehaviour

{

    public NavMeshAgent ai;
    public List<Transform> destinations;
    public Animator aiAnim;
    public float walkSpeed, chaseSpeed, minIdleTime, maxIdleTime, idleTime, detectionDistance, catchDistance, 
        searchDistance, minChaseTime, maxChaseTime, minSearchTime, maxSearchTime, jumpscareTime;
    public bool walking, chasing, searching;
    public Transform player;
    Transform currentDest;
    Vector3 dest;
    public Vector3 rayCastOffset;
    public string deathScene = "New Scene";
    public float aiDistance;

    public GameObject hideText, stopHideText;

    // Stuck detection variables
    private Vector3 lastPosition;
    private float stuckThreshold = 2.0f; // Minimum distance to move in stuckCheckInterval
    private float stuckCheckInterval = 0.5f; // How often to check if stuck
    private float stuckTimer = 0f;
    private bool isStuck = false;
    private float unstuckWiggleRadius = 3.0f; // How far to adjust path when stuck
    private float pathAdjustmentDuration = 3.0f; // How long to follow adjusted path
    private bool isAdjustingPath = false;


    void Start()

    {

        walking = true;

        currentDest = destinations[Random.Range(0, destinations.Count)];

        lastPosition = transform.position;

    }

    void Update()

    {

        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;
        aiDistance = Vector3.Distance(player.position, this.transform.position);

        // Check if agent is stuck
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            CheckIfStuck();
            stuckTimer = 0f;
        }

        if (Physics.Raycast(transform.position + rayCastOffset, direction, out hit, detectionDistance))

        {

            if (hit.collider.gameObject.tag == "Player")

            {

                walking = false;

                StopCoroutine("stayIdle");
                StopCoroutine("searchRoutine");
                StartCoroutine("searchRoutine");

                searching = true;

            }

        }
        if (searching == true)
        {
            ai.speed = 0;
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("sprint");
            aiAnim.SetTrigger("search");
            if (aiDistance <= searchDistance)
            {
                StopCoroutine("stayIdle");
                StopCoroutine("chaseRoutine");
                StopCoroutine("searchRoutine");
                StartCoroutine("chaseRoutine");

                chasing = true;
                searching = false;
            }

        }

        if (chasing == true)

        {

            dest = player.position;
            ai.destination = dest;
            ai.speed = chaseSpeed;

            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("search");
            aiAnim.SetTrigger("sprint");

            if (aiDistance <= catchDistance)

            {

                player.gameObject.SetActive(false);

                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.ResetTrigger("search");
                hideText.SetActive(false);
                stopHideText.SetActive(false);
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");

                StartCoroutine(deathRoutine());

                chasing = false;

            }

        }

        if (walking == true)

        {

            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;

            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("search");
            aiAnim.SetTrigger("walk");

            if (ai.remainingDistance <= ai.stoppingDistance)

            {

                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("search");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;

                StopCoroutine("stayIdle");
                StartCoroutine("stayIdle");

                walking = false;

            }

        }

    }

    // Check if the agent is stuck by comparing current position to last position
    private void CheckIfStuck()
    {
        // Only check for stuck if  actually trying to move
        if ((walking || chasing) && ai.speed > 0 && ai.remainingDistance > ai.stoppingDistance)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            // If it hasn't moved enough, consider the agent stuck
            if (distanceMoved < stuckThreshold)
            {
                if (!isStuck)
                {
                    isStuck = true;
                    StartCoroutine(UnstuckRoutine());
                }
            }
            else
            {
                isStuck = false;
            }
        }

        lastPosition = transform.position;
    }

    public void stopChase()
    {
        walking = true;
        chasing = false;
        StopCoroutine("chaseRoutine");
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    IEnumerator stayIdle()

    {

        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);

        walking = true;

        currentDest = destinations[Random.Range(0, destinations.Count)];

    }
    IEnumerator searchRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minSearchTime, maxSearchTime));
        searching = false;
        walking = true;
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }

    IEnumerator chaseRoutine()

    {
        yield return new WaitForSeconds(Random.Range(minChaseTime, maxChaseTime));
        stopChase();

    }

    IEnumerator deathRoutine()

    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    // Handle getting unstuck by adjusting the path
    private IEnumerator UnstuckRoutine()
    {
        if (isAdjustingPath)
            yield break;

        isAdjustingPath = true;

        // Store original destination
        Vector3 originalDestination = ai.destination;

        // Create a temporary destination to unstuck the agent
        // Find a point some distance away from current position, but still on the NavMesh
        Vector3 randomDirection = Random.insideUnitSphere * unstuckWiggleRadius;
        randomDirection.y = 0;

        // Project the point onto the NavMesh
        NavMeshHit hit;
        Vector3 temporaryDestination;

        if (NavMesh.SamplePosition(transform.position + randomDirection, out hit, unstuckWiggleRadius, NavMesh.AllAreas))
        {
            temporaryDestination = hit.position;
        }
        else
        {
            // If we couldn't find a valid position, try a different direction
            randomDirection = -randomDirection;
            if (NavMesh.SamplePosition(transform.position + randomDirection, out hit, unstuckWiggleRadius, NavMesh.AllAreas))
            {
                temporaryDestination = hit.position;
            }
            else
            {
                // If all else fails, just stay put
                temporaryDestination = transform.position;
            }
        }

        // Set new temporary destination
        ai.destination = temporaryDestination;

        // Wait for path adjustment duration
        yield return new WaitForSeconds(pathAdjustmentDuration);

        // Return to original destination if still in the same state
        if ((walking || chasing) && ai.isActiveAndEnabled)
        {
            if (walking)
            {
                ai.destination = currentDest.position;
            }
            else if (chasing)
            {
                ai.destination = player.position;
            }
        }

        isAdjustingPath = false;
    }

}