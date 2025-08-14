using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class NPCFollower : MonoBehaviour
{
    /* ───────── Ziel & Basis­werte ───────── */
    [Header("Ziel, dem gefolgt wird")]
    public Transform target;

    [Header("Geschwindigkeiten (m/s)")]
    public float walkSpeed  = 1.5f;
    public float runSpeed   = 3.5f;
    public float crawlSpeed = 0.8f;

    [Header("Abstand & Basis-Rotation")]
    public float stopDistance   = 1f;
    public float yawOffsetMoving = 0f;   // Walk/Run/Crawl
    public float yawOffsetIdle   = 45f;  // Idle-Blick leicht versetzt
    public float turnSpeed       = 360f;

    [Header("Blickverhalten")]
    public bool lookWhileIdle = true;    // Idle auf Spieler schauen?
    public bool lookAwayHide  = true;    // Hide vom Spieler abwenden?

    /* ───────── interne Komponenten ─────── */
    CharacterController cc;
    Animator            anim;

    void Awake()
    {
        cc   = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!target) return;

        /* ---------- Distanz & Richtung zum Spieler ---------- */
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        int mode = anim.GetInteger("Mode");      // 0-4  (4 = Hide)
        bool isHide = (mode == 4);

        /* ---------- 1) ROTATION -------------------------------- */
        bool shouldLook =
              (mode != 0 && !isHide)              // Walk/Run/Crawl → anschauen
           || (mode == 0 && lookWhileIdle);       // Idle optional

        if (shouldLook && toTarget.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = isHide && lookAwayHide ? -toTarget : toTarget;

            float offset = (mode == 0) ? yawOffsetIdle : yawOffsetMoving;
            Quaternion lookRot = Quaternion.LookRotation(lookDir) *
                                 Quaternion.Euler(0f, offset, 0f);

            transform.rotation = Quaternion.RotateTowards(
                                     transform.rotation,
                                     lookRot,
                                     turnSpeed * Time.deltaTime);
        }

        /* ---------- 2) GESCHWINDIGKEIT ----------------------- */
        float speed = 0f;
        switch (mode)
        {
            case 1: speed = walkSpeed;  break;
            case 2: speed = runSpeed;   break;
            case 3: speed = crawlSpeed; break;
            /* 0 (Idle) & 4 (Hide) bleiben bei speed = 0 */
        }

        /* ---------- 3) BEWEGUNG ------------------------------ */
        if (speed > 0f && dist > stopDistance && !isHide)
        {
            Vector3 dir = transform.forward;
            cc.Move(dir * speed * Time.deltaTime);
        }

        /* ---------- 4) Animator-Blend ------------------------ */
        anim.SetFloat("Speed", speed);
    }
}
