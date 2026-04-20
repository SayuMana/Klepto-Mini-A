using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class AggressiveEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed   = 4f;
    public float patrolSpeed = 2f;

    [Header("Waypoints")]
    public Transform[] waypoints;
    public float waitAtWaypoint = 1f;

    [Header("Behavior")]
    public float waitDuration = 3f;

    [Header("Vision — Capture")]
    public float viewDistance = 6f;
    [Range(1f, 360f)]
    public float viewAngle = 80f;
    public LayerMask obstacleMask;

    [Header("References")]
    public Transform player;
    public GameManager gameManager;

    public enum State { Standby, GoToWaypoint, Investigate, Waiting, ReturnToOrigin }
    public State currentState = State.Standby; // Mulai DIAM

    private Vector3    originPosition;
    private Quaternion originRotation;
    private Vector3    lastKnownPosition;
    private int        targetWaypointIndex = 0;
    private float      waitTimer           = 0f;

    private Rigidbody     rb;
    private System.Action onStartedReturnCallback;

    void Awake()
    {
        rb                = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity     = true;
        rb.interpolation  = RigidbodyInterpolation.Interpolate;

        originPosition = transform.position;
        originRotation = transform.rotation;

        // Mulai dengan X dan Z terkunci
        FreezeXZ();
    }

    // ─── Helper: aktif/nonaktif physics ──────────────────────────────────
    void SetKinematic(bool value)
    {
        rb.isKinematic = value;
        rb.useGravity  = !value;
    }

    // ─── Freeze/Unfreeze posisi X dan Z ──────────────────────────────────
    void FreezeXZ()
    {
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionZ |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
        rb.velocity = Vector3.zero;
        Debug.Log("[Aggressive] XZ Frozen.");
    }

    void UnfreezeXZ()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
        Debug.Log("[Aggressive] XZ Unfrozen.");
    }

    void OnEnable()  => EnemyManager.Instance?.RegisterAggressive(this);
    void OnDisable() => EnemyManager.Instance?.UnregisterAggressive(this);

    void Update()
    {
        switch (currentState)
        {
            case State.Standby:        HandleStandby();        break;
            case State.GoToWaypoint:   HandleGoToWaypoint();   break;
            case State.Investigate:    HandleInvestigate();    break;
            case State.Waiting:        HandleWaiting();        break;
            case State.ReturnToOrigin: HandleReturnToOrigin(); break;
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case State.GoToWaypoint:
                MoveTowards(waypoints[targetWaypointIndex].position, patrolSpeed);
                break;
            case State.Investigate:
                MoveTowards(lastKnownPosition, moveSpeed);
                break;
            case State.ReturnToOrigin:
                // Stop lebih awal agar tidak overshoot dan bergetar
                if (Vector3.Distance(transform.position, originPosition) > 0.15f)
                    MoveTowards(originPosition, patrolSpeed);
                else
                {
                    rb.velocity = Vector3.zero;
                    FreezeXZ();
                }
                break;
            default:
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                break;
        }
    }

    // ─── Dipanggil oleh PassiveEnemy ──────────────────────────────────────
    public void ReceiveAlert(Vector3 playerPosition,
                             System.Action onStartedReturn = null)
    {
        if (currentState == State.Investigate || currentState == State.Waiting) return;

        onStartedReturnCallback = onStartedReturn;
        lastKnownPosition       = playerPosition;

        // Aktifkan physics sebelum bergerak
        SetKinematic(false);
        UnfreezeXZ(); // Buka kunci X dan Z agar bisa bergerak

        // Cari waypoint terdekat → pergi ke sana dulu
        int nearest = GetNearestWaypointIndex();
        if (nearest >= 0)
        {
            targetWaypointIndex = nearest;
            currentState        = State.GoToWaypoint;
            Debug.Log($"[Aggressive] Alert! Menuju waypoint terdekat dulu: {waypoints[nearest].name}");
        }
        else
        {
            // Tidak ada waypoint → langsung kejar
            currentState = State.Investigate;
            Debug.Log($"[Aggressive] Alert! Langsung kejar player.");
        }
    }

    // ─── Update posisi player selama masih di vision cone pasif ──────────
    public void UpdateLastKnownPosition(Vector3 playerPosition)
    {
        lastKnownPosition = playerPosition;

        if (currentState == State.Waiting)
        {
            waitTimer    = 0f;
            currentState = State.Investigate;
            Debug.Log("[Aggressive] Player masih kelihatan saat Waiting! Kejar lagi.");
        }
    }

    // ─── STATE: STANDBY ───────────────────────────────────────────────────
    void HandleStandby()
    {
        CheckCapture();
    }

    // ─── STATE: GO TO WAYPOINT ────────────────────────────────────────────
    void HandleGoToWaypoint()
    {
        CheckCapture();

        if (waypoints.Length == 0) { currentState = State.Investigate; return; }

        float dist = Vector3.Distance(transform.position, waypoints[targetWaypointIndex].position);
        if (dist < 0.3f)
        {
            // Sampai di waypoint → sekarang kejar player
            currentState = State.Investigate;
            Debug.Log("[Aggressive] Sampai di waypoint. Sekarang kejar player!");
        }
    }

    // ─── STATE: INVESTIGATE ───────────────────────────────────────────────
    void HandleInvestigate()
    {
        CheckCapture();

        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.3f)
        {
            waitTimer    = 0f;
            currentState = State.Waiting;
            Debug.Log("[Aggressive] Tiba di lokasi player. Menunggu...");
        }
    }

    // ─── STATE: WAITING ───────────────────────────────────────────────────
    void HandleWaiting()
    {
        CheckCapture();
        waitTimer += Time.deltaTime;

        if (waitTimer >= waitDuration)
        {
            currentState = State.ReturnToOrigin;

            onStartedReturnCallback?.Invoke();
            onStartedReturnCallback = null;

            Debug.Log("[Aggressive] Selesai menunggu. Kembali ke posisi asal.");
        }
    }

    // ─── STATE: RETURN TO ORIGIN ──────────────────────────────────────────
    void HandleReturnToOrigin()
    {
        if (Vector3.Distance(transform.position, originPosition) < 0.15f)
        {
            rb.velocity        = Vector3.zero;
            transform.position = originPosition;
            transform.rotation = originRotation;
            FreezeXZ(); // Kunci X dan Z agar tidak bergetar
            currentState       = State.Standby;
            Debug.Log("[Aggressive] Kembali ke posisi asal. Standby.");
        }
    }

    // ─── CARI WAYPOINT TERDEKAT ───────────────────────────────────────────
    int GetNearestWaypointIndex()
    {
        int   nearest = -1;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDist) { minDist = dist; nearest = i; }
        }

        return nearest;
    }

    // ─── GERAK VIA RIGIDBODY ─────────────────────────────────────────────
    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        dir = dir.normalized;

        Vector3 moveDir = SteerAroundObstacles(dir);

        rb.velocity = new Vector3(moveDir.x * speed, rb.velocity.y, moveDir.z * speed);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(new Vector3(moveDir.x, 0, moveDir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * 10f);
        }
    }

    // ─── WALL SLIDING ─────────────────────────────────────────────────────
    Vector3 SteerAroundObstacles(Vector3 desiredDir)
    {
        float checkDist = 1.5f;

        if (Physics.Raycast(transform.position, desiredDir, out RaycastHit hit, checkDist, obstacleMask))
        {
            Vector3 wallNormal = hit.normal; wallNormal.y = 0f;
            Vector3 slideDir   = Vector3.ProjectOnPlane(desiredDir, wallNormal).normalized;

            if (!Physics.Raycast(transform.position, slideDir, checkDist * 0.8f, obstacleMask))
                return slideDir;

            if (!Physics.Raycast(transform.position, wallNormal, checkDist * 0.8f, obstacleMask))
                return wallNormal;

            return Vector3.zero;
        }

        return desiredDir;
    }

    // ─── CEK TANGKAP PLAYER ───────────────────────────────────────────────
    void CheckCapture()
    {
        if (player == null) return;
        if (CanSeeTarget(player))
        {
            Debug.Log("[Aggressive] PLAYER TERTANGKAP!");
            gameManager?.PlayerCaught();
        }
    }

    bool CanSeeTarget(Transform target)
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 left  = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0,  viewAngle / 2f, 0) * transform.forward;
        Gizmos.color  = new Color(1f, 0.3f, 0.3f);
        Gizmos.DrawRay(transform.position, left  * viewDistance);
        Gizmos.DrawRay(transform.position, right * viewDistance);

        if (waypoints == null) return;
        Gizmos.color = Color.red;
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