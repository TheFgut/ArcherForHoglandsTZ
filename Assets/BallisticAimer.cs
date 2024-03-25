using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class BallisticAimer
{

    [SerializeField] private Transform turretTransform;

    [SerializeField] private float aimSpeed;
    [SerializeField] private float maxUpAngle;
    [SerializeField] private float maxDownAngle;

    [SerializeField] private float aimAngleAccurassy = 0.05f;



    private Coroutine aimRoutine;

    private MonoBehaviour turret;
    public void Init(MonoBehaviour turret)
    {
        this.turret = turret;
    }

    public void StartAimAtProcess(Vector3 targetPos, float projectileSpeed, float projectileGravity, aimedCallback callback)
    {
        isAimed = false;
        if (aimRoutine != null)
        {
            turret.StopCoroutine(aimRoutine);
            aimRoutine = null;
        }
        aimRoutine = turret.StartCoroutine(AimCoroutine(targetPos, projectileSpeed, projectileGravity, callback));
    }

    public void StopAimAtProcess()
    {
        if (aimRoutine != null)
        {
            turret.StopCoroutine(aimRoutine);
            aimRoutine = null;
        }
    }

    public bool isAimed { get; private set; }

    private float looseTargetTime = 2;
    private IEnumerator AimCoroutine(Vector3 targetPos, float projectileSpeed, float projectileGravity, aimedCallback callback)
    {
        float looseTargetTimer = looseTargetTime;
        do
        {
            bool possibleToAim;
            isAimed = Aim(targetPos, projectileSpeed, projectileGravity, out possibleToAim);
            if (!possibleToAim)
            {
                looseTargetTimer -= Time.deltaTime;
                if(looseTargetTimer <= 0)
                {
                    callback?.Invoke(false);
                    break;
                }
                continue;
            }
            looseTargetTimer = looseTargetTime;
            yield return new WaitForEndOfFrame();
        } while (true);
        isAimed = false;
        aimRoutine = null;
        yield break;
    }

    public bool Aim(Vector3 target,float pSpeed,float pGravity, out bool isPosiible)
    {
        Vector3 s0 = new Vector3();
        int canAim = solve_ballistic_arc(turretTransform.transform.position, pSpeed, target, pGravity, out s0);
        if (canAim == 1)
        {
            float aimAngl = Vector3.Angle(new Vector3(1, 0, 0), s0);
            aimAngl = s0.y < 0 ? -aimAngl : aimAngl;
            return AimAtAngle(aimAngl, out isPosiible);
        }
        isPosiible = false;
        return false;
    }

    public bool canAim(Vector3 target, float pSpeed, float pGravity)
    {
        Vector3 s0 = new Vector3();
        bool hasSolution = solve_ballistic_arc(turretTransform.transform.position, pSpeed, target, pGravity, out s0) == 1;
        if(hasSolution)
        {
            float aimAngl = Vector3.Angle(new Vector3(1, 0, 0), s0);
            aimAngl = s0.y < 0 ? -aimAngl : aimAngl;
            return aimAngl < maxUpAngle && aimAngl > maxDownAngle;
        }
        return false;
    }

    private bool AimAtAngle(float angle, out bool isPosiible)
    {

        if (angle < maxUpAngle && angle > maxDownAngle)
        {
            isPosiible = true;
            float currentZ = turretTransform.transform.localRotation.eulerAngles.z - 180;
            float dif = Mathf.Abs(currentZ - angle);
            if (dif > aimAngleAccurassy)
            {
                float changedZ = Mathf.Lerp(currentZ, angle, (aimSpeed / dif) * Time.deltaTime);
                turretTransform.transform.localRotation = Quaternion.Euler(0, 0, changedZ + 180);
                return false;
            }
            else
            {
                
                return true;
            }
        }
        isPosiible = false;
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

    public delegate void aimedCallback(bool success);
}
