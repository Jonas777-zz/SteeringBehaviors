using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBoid : MonoBehaviour {

    public float MAX_AVOIDANCE = 20;
    public float MAX_ALIGNMENT = 10;
    public float MAX_COHESION = 15;
    public float MAX_SEPARATION = 30;
    public float MAX_CHASE = 17;
    public float MAX_EVADE = 20;

    public float boidDetectionRange = 3;
    public float obstacleDetectionRange = 2;
    public float separationRange = 1;

    public Vector2 minBounds, maxBounds;
    public float minVelocity = 1;
    public float maxVelocity = 4;

    public Vector2 vel;

    public GameObject newGreen;

    List<GameObject> obstacles, walls, friends, predators, prey;

    // Use this for initialization
    void Start()
    {
        obstacles = new List<GameObject>();
        walls = new List<GameObject>();
        friends = new List<GameObject>();
        predators = new List<GameObject>();
        prey = new List<GameObject>();
        foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
        {
            //Debug.Log(obj.name);

            if (obj.name.Contains("Obstacle"))
            {
                obstacles.Add(obj);
            }
            else if (obj.name.Contains("Walls"))
            {
                walls.Add(obj.transform.GetChild(0).gameObject);
                walls.Add(obj.transform.GetChild(1).gameObject);
                walls.Add(obj.transform.GetChild(2).gameObject);
                walls.Add(obj.transform.GetChild(3).gameObject);
            }
            else if (obj.name.Contains("Red"))
            {
                friends.Add(obj);
            }
            else if (obj.name.Contains("Blue"))
            {
                predators.Add(obj);
            }
            else if (obj.name.Contains("Red"))
            {
                prey.Add(obj);
            }
        }

        Vector2 position = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));

        transform.position = position;

        vel = new Vector2((Random.value - 0.5f) * maxVelocity, (Random.value - 0.5f) * maxVelocity);

        GetComponent<Rigidbody>().velocity = vel;

        velClamp();

        //transform.GetChild(0).Rotate(90.0f, 0.0f, 0.0f);
        transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 steering = Vector3.zero;
        steering += avoidWalls();
        steering += avoidObstacles();
        steering += alignment();
        steering += cohesion();
        //Debug.Log("sep force: " + separation());
        steering += separation();


        steering += chasePrey();
        steering += runFromPredators();

        GetComponent<Rigidbody>().velocity += steering * Time.deltaTime;


        velClamp();

        transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);

        transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);

    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Contains("Red"))
        {
            Debug.Log("collided with: " + col.gameObject.name);
            Debug.Log("at: " + col.gameObject.transform.position);
            Transform killed = col.gameObject.transform;
            prey.Remove(col.gameObject);

            Destroy(col.gameObject);

            Debug.Log("new green at: " + killed.position);
            Instantiate(newGreen, killed.position, Quaternion.identity);

        }
    }

    void velClamp()
    {
        if (GetComponent<Rigidbody>().velocity.magnitude > maxVelocity)
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * maxVelocity;
            //Debug.Log("velocity too big: " + GetComponent<Rigidbody>().velocity);
        }
        else if (GetComponent<Rigidbody>().velocity.magnitude < minVelocity)
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * minVelocity;
            //Debug.Log("velocity too small: " + GetComponent<Rigidbody>().velocity);

            if (GetComponent<Rigidbody>().velocity.magnitude == 0.0f)
            {
                GetComponent<Rigidbody>().velocity = new Vector2((Random.value - 0.5f) * maxVelocity, (Random.value - 0.5f) * maxVelocity);
                //Debug.Log("velocity hit 0, reset: " + GetComponent<Rigidbody>().velocity);
            }
        }
        GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, GetComponent<Rigidbody>().velocity.y, 0.0f);
    }

    Vector3 avoidWalls()
    {
        Vector3 avoid = Vector3.zero;
        int wallcount = 0;

        foreach (GameObject obj in walls)
        {
            Vector3 closest = obj.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (Vector3.Distance(closest, transform.position) <= 1.0f)
            {
                //Debug.Log(obj.name + " in range");
                avoid.x = transform.position.x - closest.x;
                avoid.y = transform.position.y - closest.y;

                wallcount++;
                //Debug.Log("avoidance force: " + avoid);
            }
        }

        if (wallcount == 0)
            return avoid;

        avoid.Normalize();
        avoid *= MAX_AVOIDANCE;

        return avoid;
    }

    Vector3 avoidObstacles()
    {
        Vector3 avoid = Vector3.zero;
        int obstaclecount = 0;

        foreach (GameObject obj in obstacles)
        {

            if (Vector3.Distance(obj.transform.position, transform.position) <= obstacleDetectionRange)
            {
                //Debug.Log(obj.name + " in range");
                avoid.x = transform.position.x - obj.transform.position.x;
                avoid.y = transform.position.y - obj.transform.position.y;

                obstaclecount++;
                //Debug.Log("avoidance force: " + avoid);
            }
        }

        if (obstaclecount == 0)
            return avoid;

        avoid.Normalize();
        avoid *= MAX_AVOIDANCE;

        return avoid;
    }

    Vector3 alignment()
    {
        Vector3 align = Vector3.zero;
        int neighbors = 0;

        foreach (GameObject obj in friends)
        {
            if (obj)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) <= boidDetectionRange && Vector3.Distance(obj.transform.position, transform.position) != 0.0f)
                {
                    align.x += obj.GetComponent<Rigidbody>().velocity.x;
                    align.y += obj.GetComponent<Rigidbody>().velocity.y;
                    neighbors++;
                }
            }
        }

        if (neighbors == 0)
            return align;

        align.x /= neighbors;
        align.y /= neighbors;
        align.Normalize();
        align *= MAX_ALIGNMENT;
        return align;
    }

    Vector3 cohesion()
    {
        Vector3 cohede = Vector3.zero;
        int neighbors = 0;

        foreach (GameObject obj in friends)
        {
            if (obj)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) <= boidDetectionRange && Vector3.Distance(obj.transform.position, transform.position) != 0.0f)
                {
                    cohede.x += obj.transform.position.x;
                    cohede.y += obj.transform.position.y;
                    neighbors++;
                }
            }
        }

        if (neighbors == 0)
            return cohede;

        cohede.x /= neighbors;
        cohede.y /= neighbors;
        cohede = new Vector3(cohede.x - transform.position.x, cohede.y - transform.position.y);
        cohede.Normalize();
        cohede *= MAX_COHESION;
        return cohede;
    }

    Vector3 separation()
    {
        Vector3 separate = Vector3.zero;
        int neighbors = 0;

        foreach (GameObject obj in friends)
        {
            if (obj)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) <= separationRange && Vector3.Distance(obj.transform.position, transform.position) != 0.0f)
                {
                    Vector3 temp = Vector3.zero;

                    temp.x += obj.transform.position.x - transform.position.x;
                    temp.y += obj.transform.position.y - transform.position.y;

                    separate += temp;

                    neighbors++;
                }
            }
        }

        if (neighbors == 0)
            return separate;

        //separate.x /= neighbors;
        //separate.y /= neighbors;
        separate.x *= -1;
        separate.y *= -1;
        separate.Normalize();

        separate *= MAX_SEPARATION;
        return separate;
    }

    Vector3 chasePrey()
    {
        Vector3 chase = Vector3.zero;
        int preycount = 0;

        foreach (GameObject obj in prey)
        {
            if (obj)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) <= boidDetectionRange)
                {
                    chase.x += obj.transform.position.x - transform.position.x;
                    chase.y += obj.transform.position.y - transform.position.y;

                    preycount++;
                }
            }
        }

        chase.Normalize();
        chase *= MAX_CHASE;
        return chase;
    }

    Vector3 runFromPredators()
    {
        Vector3 run = Vector3.zero;
        int predcount = 0;

        foreach (GameObject obj in predators)
        {
            if (obj)
            {
                if (Vector3.Distance(obj.transform.position, transform.position) <= boidDetectionRange)
                {
                    run.x += transform.position.x - obj.transform.position.x;
                    run.y += transform.position.y - obj.transform.position.y;

                    predcount++;
                }
            }
        }

        run.Normalize();
        run *= MAX_EVADE;
        return run;
    }
}
