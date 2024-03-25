using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Archer : MonoBehaviour
{
    [SerializeField] private Animator archerAnimator;

    [SerializeField] private ArrowGenerator arrowGenerator;
    [SerializeField] private Aimer aimer;
    [SerializeField] private Transform rotTransform;

    [SerializeField] private float intervalBetweenShots;
    // Start is called before the first frame update
    void Start()
    {
        behaviourRoutine = StartCoroutine(loockingForTargetsBehaviour());
        arrowGenerator.Init();
    }

    private TargetPool _targetPool;
    [Inject]
    public void Init(TargetPool targetPool)
    {
        _targetPool = targetPool;
    }

    private Coroutine behaviourRoutine;

    private IEnumerator loockingForTargetsBehaviour()
    {
        float targetSearchIntervalPending = 0;
        do
        {
            targetSearchIntervalPending += Time.deltaTime;
            Quaternion targetRot = Quaternion.Euler(0, 0, 0);
            if (Quaternion.Angle(rotTransform.localRotation, targetRot) > 1)
            {
                rotTransform.transform.localRotation = Quaternion.Lerp(rotTransform.transform.localRotation,
                    targetRot, Time.deltaTime * 3);
            }

            if(targetSearchIntervalPending > 0.2f)
            {
                targetSearchIntervalPending = 0;
                GameObject enemy = _targetPool.getNearestEnemy(Fraction.Zombie, transform.position);
                if (enemy != null && aimer.isAimPossible(enemy.transform.position, arrowGenerator.nextArrowToShoot))
                {
                    IKillable killable = enemy.GetComponent<IKillable>();
                    AttackTarget(killable);
                    break;
                }
            }


            yield return new WaitForEndOfFrame();
        } while (true);
    }

    public void AttackTarget(IKillable target)
    {
        if(behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
        }

        target.onDie += SearchForAnotherTarget;
        behaviourRoutine = StartCoroutine(shootingBehaviour(target));
    }

    private IEnumerator shootingBehaviour(IKillable target)
    {
        do
        {
            if (!target.isAlive)
            {
                break;
            }
            float rotAngle;

            if (aimer.isAimPossible(target.transform.position,arrowGenerator.nextArrowToShoot, out rotAngle))
            {
                Quaternion targetRot = Quaternion.Euler(0, 0, rotAngle + 90);
                rotTransform.transform.rotation = Quaternion.Lerp(rotTransform.transform.rotation,
                   targetRot, Time.deltaTime * 3);

                if (aimer.isAimed(targetRot))
                {
                    ShootIfReady();
                }
            }
            else
            {
                target.onDie -= SearchForAnotherTarget;
                break;
            }



            yield return new WaitForEndOfFrame();
        } while (true);

        SearchForAnotherTarget();
    }



    public void SearchForAnotherTarget()
    {
        if (behaviourRoutine != null)
        {
            StopCoroutine(behaviourRoutine);
        }
        behaviourRoutine = StartCoroutine(loockingForTargetsBehaviour());
    }

    // manual
    //void Update()
    //{
    //    Camera cam = Camera.main;
    //    Vector3 cursorWorldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
    //    cursorWorldPoint.z = 0;

    //    float rotAngle;
    //    if (aimer.isAimPossible(cursorWorldPoint, out rotAngle))
    //    {
    //        rotTransform.transform.rotation = Quaternion.Euler(0, 0, rotAngle + 90);
    //    }

    //    if (Input.GetKeyUp(KeyCode.Mouse0))
    //    {
    //        ShootIfReady();
    //    }
    //}

    private Coroutine actionRoutine;

    private void ShootIfReady()
    {
        if (actionRoutine == null && readyToShoot)
        {
            readyToShoot = false;
            actionRoutine = StartCoroutine(ShootRoutine());
        }
    }

    [SerializeField]private Transform mainHandTransform;
    private Arrow arrowInHand;
    private Quaternion startArrowLocalRot;

    public void GetArrowFromArrowsBag()
    {
        arrowInHand = arrowGenerator.genArrow();
        arrowInHand.transform.parent = mainHandTransform;
        startArrowLocalRot = arrowInHand.transform.localRotation;
    }

    private bool readyToShoot = true;
    private IEnumerator ShootRoutine()
    {
        // Play the animation
        archerAnimator.SetTrigger("LoadArrow");
        

        while (arrowInHand == null)
        {
            yield return new WaitForEndOfFrame();
        }
        arrowInHand.transform.localPosition = Vector3.zero;
        float coef = 0;
        do
        {
            coef += Time.deltaTime * 3;
            arrowInHand.transform.localRotation = coef < 0.5f ? Quaternion.Lerp(startArrowLocalRot, Quaternion.Euler(0, 0, -260), coef * 2) :
                Quaternion.Lerp(Quaternion.Euler(0, 0, -280), Quaternion.Euler(0, 0, 0), (coef - 0.5f) * 2);
            yield return new WaitForEndOfFrame();
        } while (coef < 1);
        arrowInHand.GoToFly();

        yield return new WaitForSeconds(intervalBetweenShots);
        readyToShoot = true;
        arrowInHand = null;
        actionRoutine = null;

    }


    [System.Serializable]
    private class ArrowGenerator
    {
        [SerializeField] private Dropdown arrowChangeDropdown;
        [SerializeField] private Arrow[] arrowPrefabs;
        private int choosedArrow;

        public void Init()
        {
            arrowChangeDropdown.onValueChanged.AddListener(setChoosedArrowId);
        }
        public void setChoosedArrowId(int id)
        {
            choosedArrow = id;
        }
        public Arrow nextArrowToShoot {  get { return arrowPrefabs[choosedArrow]; } }

        public Arrow genArrow()
        {
            Arrow genedArrow = Instantiate(arrowPrefabs[choosedArrow]);
            genedArrow.transform.Rotate(0,0,-90);
            return genedArrow;
        }

    }
}

[System.Serializable]
class Aimer
{
    [SerializeField] private float yCorrection;
    [SerializeField] private float xCorrection;
    [SerializeField] private Transform rotAxis;

    public bool isAimPossible(Vector3 target,Arrow arrowToShoot, out float aimAngle)
    {
        Vector3 s0 = new Vector3();
        int canAim = solve_ballistic_arc(rotAxis.position + new Vector3(xCorrection, yCorrection), arrowToShoot.speed, target, arrowToShoot.gravity, out s0);
        if (canAim == 1)
        {
            float aimAngl = Vector3.Angle(new Vector3(1, 0, 0), s0);
            aimAngl = s0.y < 0 ? -aimAngl : aimAngl;
            aimAngle = aimAngl;
            return true;
        }

        aimAngle = 0;
        return false;
    }

    public bool isAimed(Quaternion needRot)
    {
        
        return Quaternion.Angle(needRot, rotAxis.transform.rotation) < 5;
    }

    public bool isAimPossible(Vector3 target, Arrow arrowToShoot)
    {
        Vector3 s0 = new Vector3();
        int canAim = solve_ballistic_arc(rotAxis.position + new Vector3(0, yCorrection), arrowToShoot.speed, target, arrowToShoot.gravity, out s0);
        if (canAim == 1)
        {
            float aimAngl = Vector3.Angle(new Vector3(1, 0, 0), s0);
            aimAngl = s0.y < 0 ? -aimAngl : aimAngl;
            return true;
        }

        return false;
    }

    private int solve_ballistic_arc(Vector3 proj_pos, float proj_speed, Vector3 target, float gravity, out Vector3 s0)
    {

        s0 = Vector3.zero;

        Vector3 diff = target - proj_pos;
        Vector3 diffXZ = new Vector3(diff.x, 0f, diff.z);
        float groundDist = diffXZ.magnitude;

        float speed2 = proj_speed * proj_speed;
        float speed4 = proj_speed * proj_speed * proj_speed * proj_speed;
        float y = diff.y;
        float x = groundDist;
        float gx = gravity * x;

        float root = speed4 - gravity * (gravity * x * x + 2 * y * speed2);

        // No solution
        if (root < 0)
            return 0;

        root = Mathf.Sqrt(root);

        float lowAng = Mathf.Atan2(speed2 - root, gx);


        Vector3 groundDir = diffXZ.normalized;
        s0 = groundDir * Mathf.Cos(lowAng) * proj_speed + Vector3.up * Mathf.Sin(lowAng) * proj_speed;

        return 1;
    }
}
