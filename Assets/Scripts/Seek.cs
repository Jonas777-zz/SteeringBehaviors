using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : MonoBehaviour {
    public float SEEK_WEIGHT, ARRIVE_WEIGHT;

    //randomized spawn position + velocity bounds
    public Vector2 minBounds, maxBounds;
    public float minVelocity = 1;
    public float maxVelocity = 4;

    public GameObject target;

    public bool arrive;

	// Use this for initialization
	void Start () {

        Vector2 position = new Vector2(Random.Range(minBounds.x, maxBounds.x), Random.Range(minBounds.y, maxBounds.y));

        transform.position = position;

        GetComponent<Rigidbody>().velocity = new Vector2((Random.value - 0.5f) * maxVelocity, (Random.value - 0.5f) * maxVelocity);

        velClamp();

        transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 steering = Vector3.zero;

        steering += seek();

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

    Vector3 seek()
    {
        Vector3 seekVec = Vector3.zero;

        seekVec.x += target.transform.position.x - transform.position.x;
        seekVec.y += target.transform.position.y - transform.position.y;

        //float dist = Vector3.Distance(transform.position, target.transform.position);

        ////reduce force linearly if arriving based on distance
        //if (arrive && dist > 0 && dist <= 1.0f)
        //{
        //    seekVec *= Mathf.Pow(dist, 2);
        //    seekVec /= SEEK_WEIGHT;
        //    Debug.Log("arriving");
        //}

        seekVec.Normalize();
        seekVec *= SEEK_WEIGHT;
        return seekVec;
    }

    Vector3 arriveF()
    {
        Vector3 arriveVec = Vector3.zero;
        Vector3 desired = Vector3.zero;
        
        float dist = Vector3.Distance(transform.position, target.transform.position);

        if (arrive && dist < 2.0f)
        {
            desired = transform.position - target.transform.position;
            desired.Normalize();
            desired *= (dist / 2.0f);
        }

        desired *= SEEK_WEIGHT;

        return desired;
    }
}
