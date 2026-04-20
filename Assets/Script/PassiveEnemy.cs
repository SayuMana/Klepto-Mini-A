using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PassiveEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] waypoints;
    public float moveSpeed      = 2f;
    public float waitAtWaypoint = 1.5f;

    public enum State { Patrolling, Frozen }
    public State currentState = State.Patrolling;

    [Header("Vision")]
    public float viewDistance = 8f;
    [Range(1f, 360f)]
    public float viewAngle = 90f;
    public LayerMask obstacleMask;

    [Header("References")]
    public Transform player;

    private int   waypointIndex  = 0;
    private bool  isWaiting      = false;
    private bool  hasAlerted     = false;
    private float alertCooldown  = 3f;
    private float alertTimer     = 0f;

    private AggressiveEnemy alertedEnemy = null;
    private Rigidbody rb;

    void Awake()
    {
        rb                = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity     = true;
        rb.interpolation  = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        CheckVision();

        if (hasAlerted)
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertCooldown)
            {
                hasAlerted = false;
                alertTimer = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        if (currentState == State.Patrolling && !isWaiting && waypoints.Length > 0)
        {
            MoveTowards(waypoints[waypointIndex].position);

            if (Vector3.Distance(transform.position, waypoints[waypointIndex].position) < 0.2f)
                StartCoroutine(WaitRoutine());
        }
        else
        {
            // Stop horizontal saat Frozen atau menunggu di waypoint
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }

    IEnumerator WaitRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitAtWaypoint);
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        isWaiting     = false;
    }

    void CheckVision()
    {
        if (player == null) return;

        if (CanSeeTarget(player))
        {
            AggressiveEnemy nearest = EnemyManager.Instance?.GetNearestAggressive(transform.position);
            if (nearest == null) return;

            bool aggressiveIsReturning = nearest.currentState == AggressiveEnemy.State.ReturnToOrigin;

            if (!hasAlerted || aggressiveIsReturning)
            {
                alertedEnemy = nearest;
                currentState = State.Frozen;
                hasAlerted   = true;
                alertTimer   = 0f;

                nearest.ReceiveAlert(
                    player.position,
                    onStartedReturn: OnAggressiveStartedReturn
                );

                Debug.Log(aggressiveIsReturning
                    ? "[Passive] Player terdeteksi lagi! Agresif dialihkan."
                    : "[Passive] Melihat player! Freeze & kirim alert ke agresif.");
            }
            else if (alertedEnemy != null)
            {
                alertedEnemy.UpdateLastKnownPosition(player.position);
            }
        }
    }

    void OnAggressiveStartedReturn()
    {
        alertedEnemy = null;
        currentState = State.Patrolling;
        Debug.Log("[Passive] Agresif sedang kembali ke asal. Lanjut patrol!");
    }

    // ─── GERAK VIA RIGIDBODY ─────────────────────────────────────────────
    void MoveTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        dir   = dir.normalized;

        Vector3 moveDir = SteerAroundObstacles(dir);

        rb.velocity = new Vector3(moveDir.x * moveSpeed, rb.velocity.y, moveDir.z * moveSpeed);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(moveDir.x, 0, moveDir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * 10f);
        }
    }

    // ─── WALL SLIDING STEERING ────────────────────────────────────────────
    Vector3 SteerAroundObstacles(Vector3 desiredDir)
    {
        float checkDist = 1.2f;

        if (Physics.Raycast(transform.position, desiredDir, out RaycastHit hit, checkDist, obstacleMask))
        {
            Vector3 wallNormal = hit.normal;
            wallNormal.y       = 0f;

            // Geser menyusuri dinding
            Vector3 slideDir = Vector3.ProjectOnPlane(desiredDir, wallNormal).normalized;

            if (!Physics.Raycast(transform.position, slideDir, checkDist * 0.8f, obstacleMask))
                return slideDir;

            Vector3 perpDir = wallNormal;
            if (!Physics.Raycast(transform.position, perpDir, checkDist * 0.8f, obstacleMask))
                return perpDir;

            return Vector3.zero;
        }

        return desiredDir;
    }

    public bool CanSeeTarget(Transform target)
    {
        Vector3 dirToTarget = target.position - transform.position;
        float dist          = dirToTarget.magnitude;

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dirToTarget.normalized);
        if (angle > viewAngle / 2f) return false;

        if (Physics.Raycast(transform.position, dirToTarget.normalized, dist, obstacleMask))
            return false;

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left  = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0,  viewAngle / 2f, 0) * transform.forward;
        Gizmos.color  = Color.green;
        Gizmos.DrawRay(transform.position, left  * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);

        if (waypoints == null) return;
        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}