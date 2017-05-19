using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : MonoBehaviour {
    public float FLEE_WEIGHT;

    //randomized spawn position + velocity bounds
    public Vector2 minBounds, maxBounds;
    public float minVelocity = 1;
    public float maxVelocity = 4;

    public GameObject target;

    List<GameObject> walls;

    // Use this for initialization
    void Start()
    {
        walls = new List<GameObject>();
        foreach (GameObject obj in Object.FindObjectsOfType(typeof(GameObject)))
        {
            if (obj.name.Contains("Walls"))
            {
                walls.Add(obj.transform.GetChild(0).gameObject);
                walls.Add(obj.transform.GetChild(1).gameObject);
                walls.Add(obj.transform.GetChild(2).gameObject);
                walls.Add(obj.transform.GetChild(3).gameObject);
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
        Vector3 steering = Vector3.zero;

        steering += flee();
        steering += avoidWalls();

        //change the velocity
        GetComponent<Rigidbody>().velocity += steering * Time.deltaTime;

        //clamp between max values
        velClamp();

        //clamp position, rotation, and look where you're going
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
        transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);

        //detect mouse input to change target pos
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            mousePos.z = 0;

            target.transform.position = mousePos;
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

    //run away from the crosshair
    Vector3 flee()
    {
        Vector3 fleeVec = Vector3.zero;

        fleeVec.x += transform.position.x - target.transform.position.x;
        fleeVec.y += transform.position.y - target.transform.position.y;

        fleeVec.Normalize();
        fleeVec *= FLEE_WEIGHT;
        return fleeVec;
    }

    //steer away from the walls on the border of the game area
    Vector3 avoidWalls()
    {
        Vector3 avoid = Vector3.zero;
        int wallcount = 0;

        foreach (GameObject obj in walls)
        {
            Vector3 closest = obj.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            if (Vector3.Distance(closest, transform.position) <= 2.0f)
            {
                avoid.x = transform.position.x - closest.x;
                avoid.y = transform.position.y - closest.y;

                wallcount++;
            }
        }

        if (wallcount == 0)
            return avoid;

        avoid.Normalize();
        avoid *= 20;

        return avoid;
    }
}
