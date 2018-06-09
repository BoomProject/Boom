using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour {

    public float spd;
    void Update() {
        transform.localEulerAngles = transform.localEulerAngles + Vector3.up * spd * Time.deltaTime;
    }
}
