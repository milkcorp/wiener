using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour {

    //balletのプレハブ
    public GameObject ballet;

    public GameObject canon;
    public Rigidbody shaft;
    public Transform wheelLeft;
    public Transform wheelRight;

    public Vector3 wheelVelocityVec;

    //from DNA
    public float wheelsizeL = 1f;
    public float wheelsizeR = 1f;
    public float wheelVelocity;

    public float balletVelocity = 100;
    public float balletInterval = 3;
    public float balletSize = 1;

    public Color baseColor;
    public Color wheelColoer;

    //
    public int killCount = 0;

    //
    private float counter;


	// Use this for initialization
	void Start () {

        //DNA設定
        wheelLeft.transform.localScale = new Vector3(
            wheelLeft.transform.localScale.x,
            wheelLeft.transform.localScale.y * wheelsizeL,
            wheelLeft.transform.localScale.z * wheelsizeL
        );

        wheelLeft.transform.localScale = new Vector3(
            wheelRight.transform.localScale.x,
            wheelRight.transform.localScale.y * wheelsizeR,
            wheelRight.transform.localScale.z * wheelsizeR
        );

        wheelVelocityVec = -transform.forward * wheelVelocity;

        canon.GetComponent<MeshRenderer>().material.color = baseColor;
        //wheelLeft.GetComponent<Material>().color = wheelColoer;

        wheelLeft.GetComponent<MeshRenderer>().material.SetColor("_Color", wheelColoer);
        wheelRight.GetComponent<MeshRenderer>().material.color = wheelColoer;
		
	}
	
	// Update is called once per frame
	void Update () {
        counter += Time.deltaTime;
        if(counter > balletInterval) {
            counter = 0;
            shoot();
        }

	}

    void FixedUpdate()
    {

        shaft.AddRelativeTorque(wheelVelocityVec * Time.fixedDeltaTime);
    }

    public void shoot() {
        GameObject b = Instantiate<GameObject>(ballet, transform.position + transform.forward, Quaternion.identity);
        b.transform.localScale *= balletSize;
        b.GetComponent<Rigidbody>().AddForce(transform.forward * balletVelocity);
        b.GetComponent<Ballet>().tank = this;
    }


    void OnCollisionEnter(Collision collision)
    {
        Ballet b = collision.gameObject.GetComponent<Ballet>();
        if(b != null) {
            b.tank.OnKill();
            Dead();
        }
    }

    public void Dead() {
        Destroy(gameObject);
    }


    public void OnKill() {
        killCount++;
    }
}
