using UnityEngine;

public class BasicPatrol : MonoBehaviour
{
    public float SpeedMultiplyer = 2f;
    private float patrolPointDistance = 1f;
    public float patrolPointDwellTime = 3f;
    
    int currentPatrolPointIndex;
    private Vector3 currentLocation;
    private Vector3 targetLocation;

    private float distanceToPatrolPoint;

    float timeSinceArrivedAtWaypoint = Mathf.Infinity;
    
    BasicInteraction currentInteraction;

    public GameObject patrolParent;


    public void Start()
    {
        currentInteraction = GetComponent<BasicInteraction>();
        currentPatrolPointIndex = 0;
        currentLocation = transform.position;
        targetLocation = GetPatrolPoint(currentPatrolPointIndex);
        distanceToPatrolPoint = Vector3.Distance(currentLocation, targetLocation);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < patrolParent.transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.DrawSphere(GetPatrolPoint(i), 0.3f);
            Gizmos.DrawLine(GetPatrolPoint(i), GetPatrolPoint(j));
        }
    }

    public int GetNextIndex(int i)
    {
        if(i + 1 == patrolParent.transform.childCount)
        {
            return 0;
        }
        return i + 1;
    }

    public Vector3 GetPatrolPoint(int i)
    {
        return patrolParent.transform.GetChild(i).position;
    }

    public void Update()
    {
        if (currentInteraction != null && currentInteraction.isCurrentConversation)
        {
            Wait();
            return;
        }
        
        targetLocation = GetPatrolPoint(currentPatrolPointIndex);
        currentLocation = transform.position;
        
        transform.LookAt(new Vector3(targetLocation.x, transform.position.y, targetLocation.z));
        
        distanceToPatrolPoint = Vector3.Distance(currentLocation, targetLocation);
        
        timeSinceArrivedAtWaypoint += Time.deltaTime;
        
        if (distanceToPatrolPoint < patrolPointDistance)
        {
            timeSinceArrivedAtWaypoint = 0;
            currentPatrolPointIndex = GetNextIndex(currentPatrolPointIndex);
        }
        
        if (timeSinceArrivedAtWaypoint >= patrolPointDwellTime)
        {
            transform.position = Vector3.MoveTowards(currentLocation,  targetLocation, SpeedMultiplyer * Time.deltaTime);
        }
    }
    
    public void Wait()
    {
        transform.position = Vector3.MoveTowards(currentLocation, currentLocation, 0);
    }
}
