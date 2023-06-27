using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSped : MonoBehaviour
{
    //made by TheIrishSanta
    public Rigidbody b_rigidbody;
    public float m_Thrust = 1000;
    private GameObject shootPoint;

    private void Start()
    {
        shootPoint = GameObject.FindGameObjectWithTag("ShootPoint");
        b_rigidbody = this.GetComponent<Rigidbody>();
        b_rigidbody.AddForce(m_Thrust * shootPoint.transform.forward);
    }
    private void FixedUpdate()
    {
        Destroy(gameObject, 6f);
    }
}
