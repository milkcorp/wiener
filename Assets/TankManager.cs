using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour {

    public GameObject tankPrefab;
    public authSample auth;

    public async void CreateTank(){
        GameObject tankObj = Instantiate<GameObject>(tankPrefab, Vector3.zero, Quaternion.identity);
        tankObj.SetActive(false);
        Tank tank = tankObj.GetComponent<Tank>();

        authSample.TankDna dna = await auth.CreateTank();

        tank.wheelsizeL = dna.wheelLeft;
        tank.wheelsizeR = dna.wheelRight;
        tank.wheelVelocity = dna.cannonSpeed;
        tank.balletSize = dna.bulletSize;
        tank.balletVelocity = dna.speed * 50;
        tank.balletInterval = dna.interval;
        tank.baseColor = dna.baseColor;
        tank.wheelColoer = dna.wheelColor;

        tank.SetStatus();
        tankObj.SetActive(true);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
