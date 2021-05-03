using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isAutomatic;
    public float timeBetweenShots = 0.1f, heatPerShot = 1f;
    public GameObject muzzleFlash;
}
