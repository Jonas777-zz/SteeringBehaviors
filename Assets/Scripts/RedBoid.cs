using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBoid : MonoBehaviour
{
    //max forces for each steering component
    public float MAX_AVOIDANCE = 20;
    public float MAX_ALIGNMENT = 10;
    public float MAX_COHESION = 15;
    public float MAX_SEPARATION = 30;
    public float MAX_CHASE = 17;
    public float MAX_EVADE = 20;

    //detection ranges
    public float boidDetectionRange = 3;
    public float obstacleDetectionRange = 2;
    public float separationRange = 1;

    //randomized spawn position + velocity bounds
    public Vector2 minBounds, maxBounds;
    public float minVelocity = 1;
    public float maxVelocity = 4;

    public GameObject newRed;

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
            else if (obj.name.Contains("Green"))    //boids to run away from
            {
                predators.Add(obj);
            }
            else if (obj.name.Contains("Blue"))     //boids to chase
            {
                prey.Add(obj);
            }
        }

        Vector2 position = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));

        transform.position = position;

        GetComponent<Rigidbody>().velocity = new Vector2((Random.value - 0.5f) * maxVelocity, (Random.value - 0.5f) * maxVelocity);

        velClamp();

        transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
    }

    // Update is called once per frame
    void Update()
    {
        //sum the steering forces
        Vector3 steering = Vector3.zero;
        steering += avoidWalls();
        steering += avoidObstacles();
        steering += alignment();
        steering += cohesion();
        steering += separation();
        
        steering += chasePrey();
        steering += runFromPredators();

        //change the velocity
        GetComponent<Rigidbody>().velocity += steering * Time.deltaTime;

        //clamp between max values
        velClamp();

        //clamp position, rotation, and look where you're going
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
        transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);

    }

    //if prey is hit, kill it and make a new boid
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name.Contains("Blue"))
        {
            Debug.Log("collided with: " + col.gameObject.name);
            Debug.Log("at: " + col.gameObject.transform.position);
            Transform killed = col.gameObject.transform;
            prey.Remove(col.gameObject);

            Destroy(col.gameObject);

            Debug.Log("new red at: " + killed.position);
            Instantiate(newRed, killed.position, Quaternion.identity);
            
        }
    }

    //keep velocity within acceptable range
    void velClamp()
    {
        if (GetComponent<Rigidbody>().velocity.magnitude > maxVelocity)
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * maxVelocity;
        }
        else if (GetComponent<Rigidbody>().velocity.magnitude < minVelocity)
        {
            GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity.normalized * minVelocity;

            if (GetComponent<Rigidbody>().velocity.magnitude == 0.0f)   //if stopped, re-randomize velocity
            {
                GetComponent<Rigidbody>().velocity = new Vector2((Random.value - 0.5f) * maxVelocity, (Random.value - 0.5f) * maxVelocity);
            }
        }
        GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, GetComponent<Rigidbody>().velocity.y, 0.0f);
    }

    //steer away from the walls on the border of the game area
    Vector3 avoidWalls()
    {
        Vector3 avoid = Vector3.zero;
        int wallcount = 0;

        foreach (GameObject obj in walls)
        {
            Vector3 closest = obj.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (Vector3.Distance(closest, transform.position) <= 1.0f)
            {
                avoid.x = transform.position.x - closest.x;
                avoid.y = transform.position.y - closest.y;

                wallcount++;
            }
        }

        if (wallcount == 0)
            return avoid;

        avoid.Normalize();
        avoid *= MAX_AVOIDANCE;

        return avoid;
    }

    //steer away from the obstacles
    Vector3 avoidObstacles()
    {
        Vector3 avoid = Vector3.zero;
        int obstaclecount = 0;

        foreach (GameObject obj in obstacles)
        {

            if (Vector3.Distance(obj.transform.position, transform.position) <= obstacleDetectionRange)
            {
                avoid.x = transform.position.x - obj.transform.position.x;
                avoid.y = transform.position.y - obj.transform.position.y;

                obstaclecount++;
            }
        }

        if (obstaclecount == 0)
            return avoid;

        avoid.Normalize();
        avoid *= MAX_AVOIDANCE;

        return avoid;
    }

    //try to match alignment with friends in flock
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

    //group together with friends
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

    //don't group up too much with your friends
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

        separate.x *= -1;
        separate.y *= -1;
        separate.Normalize();

        separate *= MAX_SEPARATION;
        return separate;
    }

    //run after your prey
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

    //run away from predators
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
