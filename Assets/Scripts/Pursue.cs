using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pursue : MonoBehaviour {
    public float PURSUE_WEIGHT;

    //randomized spawn position + velocity bounds
    public Vector2 minBounds, maxBounds;
    public float minVelocity = 1;
    public float maxVelocity = 4;

    public GameObject target;

    // Use this for initialization
    void Start()
    {

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

        steering += pursue();

        //change the velocity
        GetComponent<Rigidbody>().velocity += steering * Time.deltaTime;

        //clamp between max values
        velClamp();

        //clamp position, rotation, and look where you're going
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, transform.rotation.eulerAngles.z);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
        transform.LookAt(transform.position + GetComponent<Rigidbody>().velocity);

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

    //seek to where the target WILL be, not where it is
    Vector3 pursue()
    {
        Vector3 pursueVec = Vector3.zero;

        Vector3 targetPos = target.transform.position;

        targetPos += target.GetComponent<Rigidbody>().velocity;     //add velocity to predict movement

        pursueVec.x += targetPos.x - transform.position.x;
        pursueVec.y += targetPos.y - transform.position.y;

        pursueVec.Normalize();
        pursueVec *= PURSUE_WEIGHT;
        return pursueVec;
    }
}
