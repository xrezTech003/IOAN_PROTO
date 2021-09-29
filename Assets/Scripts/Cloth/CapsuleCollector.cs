using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// CD: Finds controller colliders and adds them to the collider list on the cloth
/// </summary>
public class CapsuleCollector : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    /// <summary>
    /// FD: Called every frame but returns if not one frame in 400 
    /// </summary>
    void Update()
    {
        if (Time.frameCount%400 == 0)
        {
            GameObject[] capsules = GameObject.FindGameObjectsWithTag("projectile");
            CapsuleCollider[] colliders = new CapsuleCollider[capsules.Length];
            int index = 0;
            foreach (GameObject capsule in capsules) { colliders[index] = (capsule.GetComponent<CapsuleCollider>()); index++; }
            gameObject.GetComponentInChildren<Cloth>().capsuleColliders = colliders;
        }
    }

}
