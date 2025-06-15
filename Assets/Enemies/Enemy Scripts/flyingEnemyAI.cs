using UnityEngine;

public class flyingEnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    [SerializeField] public float speed = 2f;
    [SerializeField] public float stopDist = 0.5f;
    [SerializeField] float moveWait = 2f;
    int currentPointIndex = 0;

    [SerializeField] float waitTimer = 0f;
    bool isWaiting = false;

    void Start()
    {
        ShufflePoints();
    }

    void Update()
    {
        patrolNextArea();
    }

    void patrolNextArea()
    {
        if (patrolPoints.Length == 0)
            return;
        //Shuffler Fisher-Yates


        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                //currentPointIndex = Random.Range(0, patrolPoints.Length);
                currentPointIndex++;
                if (currentPointIndex >= patrolPoints.Length)
                {
                    currentPointIndex = 0;
                    ShufflePoints();
                }
            }
        }
        else
        {
            Transform target = patrolPoints[currentPointIndex];
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            Vector3 direction = target.position - transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation * Quaternion.Euler(0, 180, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            if (Vector3.Distance(transform.position, target.position) <= stopDist)
            {
                isWaiting = true;
                waitTimer = moveWait;
            }
        }
    }

    void ShufflePoints()
    {
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            int randIndex = Random.Range(i, patrolPoints.Length);
            Transform temp = patrolPoints[i];
            patrolPoints[i] = patrolPoints[randIndex];
            patrolPoints[randIndex] = temp;
        }
    }

 
}
