using UnityEngine;

/// <summary>
/// Attach ke GameObject enemy bersama Animator component.
/// Di Animator Controller, buat parameter Trigger:
/// - "Walk"
/// - "Idle"
/// - "Run"    (untuk agresif)
/// - "Return" (untuk agresif)
/// 
/// Setiap trigger mengarah ke state animasi masing-masing.
/// Semua transition: Has Exit Time = false, Transition Duration = 0.1
/// </summary>
[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;

    // Nama trigger harus sama persis dengan di Animator Controller
    private const string WALK   = "Walk";
    private const string IDLE   = "Idle";
    private const string RUN    = "Run";
    private const string RETURN = "Return";

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayWalk()
    {
        ResetAllTriggers();
        animator.SetTrigger(WALK);
    }

    public void PlayIdle()
    {
        ResetAllTriggers();
        animator.SetTrigger(IDLE);
    }

    public void PlayRun()
    {
        ResetAllTriggers();
        animator.SetTrigger(RUN);
    }

    public void PlayReturn()
    {
        ResetAllTriggers();
        animator.SetTrigger(RETURN);
    }

    // Reset semua trigger agar tidak overlap
    void ResetAllTriggers()
    {
        animator.ResetTrigger(WALK);
        animator.ResetTrigger(IDLE);
        animator.ResetTrigger(RUN);
        animator.ResetTrigger(RETURN);
    }
}