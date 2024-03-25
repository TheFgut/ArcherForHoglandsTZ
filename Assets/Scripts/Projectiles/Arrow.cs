using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private Transform raycastOrigin;

    [SerializeField] private float _damage;

    public float damage { get => _damage; }

    [SerializeField] private float _speed;
    public float speed { get => _speed; }
    [SerializeField] private float _gravity;
    public float gravity { get => _gravity; }

    [SerializeField] private float lifetime = 10;

    public void GoToFly()
    {
        transform.parent = null;
        movePerSecond = transform.TransformDirection(new Vector3(_speed, 0, 0));
        StartCoroutine(flyRoutine());
    }

    private Vector3 movePerSecond;
    private IEnumerator flyRoutine()
    {
        do
        {
            movePerSecond.y -= _gravity * Time.deltaTime;
            Vector3 movePerFrame = movePerSecond * Time.deltaTime;
            transform.position += movePerFrame;
            transform.rotation = Quaternion.FromToRotation(Vector3.right, movePerSecond.normalized);

            lifetime -= Time.deltaTime;

            GameObject hitObj;
            if (collisionsChecks(movePerFrame, out hitObj))
            {
                transform.parent = hitObj.transform;
                break;
            }
            
            if (lifetime < 0) Destroy(gameObject);
            yield return new WaitForEndOfFrame();
        } while (true);

        
    }

    private bool collisionsChecks(Vector3 movePerFrame, out GameObject hitObj)
    {
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin.position, movePerFrame, movePerFrame.magnitude + 0.01f);
        if(hit)
        {
            if(hit.collider.tag == "enemy")
            {
                hit.collider.GetComponent<IKillable>().Hurt(this);
            }
            hitObj = hit.collider.gameObject;
            return true;
        }
        hitObj = null;
        return false;
    }

    public override string ToString()
    {
        return string.Format("Type-arrow. Damage:{0}", damage);
    }
}
