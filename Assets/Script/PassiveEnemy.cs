using System.Collections;
using UnityEngine;

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

    private int waypointIndex  = 0;
    private bool isWaiting     = false;
    private bool hasAlerted    = false;
    private float alertCooldown = 3f;
    private float alertTimer    = 0f;

    private AggressiveEnemy alertedEnemy = null;

    void Update()
    {
        if (currentState == State.Patrolling)
            Patrol();

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

    void Patrol()
    {
        if (waypoints.Length == 0 || isWaiting) return;

        Transform target = waypoints[waypointIndex];
        transform.position = Vector3.MoveTowards(
            transform.position, target.position, moveSpeed * Time.deltaTime);

        Vector3 dir = target.position - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            StartCoroutine(WaitRoutine());
    }

    IEnumerator WaitRoutine()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitAtWaypoint);
        waypointIndex = (waypointIndex + 1) % waypoints.Length;
        isWaiting = false;
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
                // ── Alert baru: pertama detect, ATAU agresif sedang balik ke asal ──
                alertedEnemy = nearest;
                currentState = State.Frozen;
                hasAlerted   = true;
                alertTimer   = 0f;

                nearest.ReceiveAlert(
                    player.position,
                    onStartedReturn: OnAggressiveStartedReturn
                );

                Debug.Log(aggressiveIsReturning
                    ? "[Passive] Player terdeteksi lagi! Agresif dialihkan saat ReturnToOrigin."
                    : "[Passive] Melihat player! Freeze & kirim alert ke agresif.");
            }
            else if (alertedEnemy != null)
            {
                // ── Update posisi player tiap frame selama masih kelihatan ──
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