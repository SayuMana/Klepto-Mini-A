using UnityEngine;

public class AggressiveEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Behavior")]
    public float waitDuration = 3f;
    private float waitTimer = 0f;

    [Header("Vision — Capture")]
    public float viewDistance = 6f;
    [Range(1f, 360f)]
    public float viewAngle = 80f;
    public LayerMask obstacleMask;

    [Header("References")]
    public Transform player;
    public GameManager gameManager;

    public enum State { Standby, Investigate, Waiting, ReturnToOrigin }
    public State currentState = State.Standby;

    private Vector3 originPosition;
    private Quaternion originRotation;
    private Vector3 lastKnownPosition;

    private System.Action onStartedReturnCallback;

    void Awake()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;
    }

    void OnEnable()  => EnemyManager.Instance?.RegisterAggressive(this);
    void OnDisable() => EnemyManager.Instance?.UnregisterAggressive(this);

    void Update()
    {
        switch (currentState)
        {
            case State.Standby:        HandleStandby();        break;
            case State.Investigate:    HandleInvestigate();    break;
            case State.Waiting:        HandleWaiting();        break;
            case State.ReturnToOrigin: HandleReturnToOrigin(); break;
        }
    }

    // ─── Dipanggil oleh PassiveEnemy ──────────────────────────────────────
    // Sekarang juga menerima alert saat ReturnToOrigin
    public void ReceiveAlert(Vector3 playerPosition,
                            System.Action onStartedReturn = null)
    {
        // Terima alert dari Standby ATAU ReturnToOrigin
        if (currentState != State.Standby && currentState != State.ReturnToOrigin) return;

        lastKnownPosition       = playerPosition;
        onStartedReturnCallback = onStartedReturn;
        currentState            = State.Investigate;

        Debug.Log($"[Aggressive] Alert diterima! Berangkat ke {playerPosition}");
    }

    // ─── Update posisi player selama masih di vision cone pasif ──────────
    public void UpdateLastKnownPosition(Vector3 playerPosition)
    {
        if (currentState == State.Investigate || currentState == State.Waiting)
        {
            lastKnownPosition = playerPosition;

            if (currentState == State.Waiting)
            {
                waitTimer    = 0f;
                currentState = State.Investigate;
                Debug.Log("[Aggressive] Posisi player diupdate saat Waiting! Kejar lagi.");
            }
        }
    }

    // ─── STATE: STANDBY ───────────────────────────────────────────────────
    void HandleStandby()
    {
        CheckCapture();
    }

    // ─── STATE: INVESTIGATE ───────────────────────────────────────────────
    void HandleInvestigate()
    {
        CheckCapture();
        MoveTowards(lastKnownPosition);

        if (Vector3.Distance(transform.position, lastKnownPosition) < 0.3f)
        {
            waitTimer    = 0f;
            currentState = State.Waiting;
            Debug.Log("[Aggressive] Tiba di lokasi. Mulai menunggu.");
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

            // Beritahu pasif bahwa agresif mulai kembali → pasif boleh patrol
            onStartedReturnCallback?.Invoke();
            onStartedReturnCallback = null;

            Debug.Log("[Aggressive] Mulai kembali ke posisi asal. Pasif boleh jalan lagi.");
        }
    }

    // ─── STATE: RETURN TO ORIGIN ──────────────────────────────────────────
    void HandleReturnToOrigin()
    {
        MoveTowards(originPosition);

        if (Vector3.Distance(transform.position, originPosition) < 0.1f)
        {
            transform.position = originPosition;
            transform.rotation = originRotation;
            currentState       = State.Standby;
            Debug.Log("[Aggressive] Kembali ke posisi asal. Standby.");
        }
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

    void MoveTowards(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(
            transform.position, target, moveSpeed * Time.deltaTime);

        Vector3 dir = target - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
        }
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
    }
}