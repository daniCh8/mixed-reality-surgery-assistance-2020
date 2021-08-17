using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreBoxCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponentsInChildren<BoxCollider>(true) != null)
        {
            BoxCollider[] boxColliders = GetComponentsInChildren<BoxCollider>(true);
            CapsuleCollider[] capColliders = GetComponentsInChildren<CapsuleCollider>(true);
            for (int i = 0; i < boxColliders.Length; i++)
            {
                for (int j = 0; j < boxColliders.Length; j++)
                {
                    Physics.IgnoreCollision(boxColliders[i], boxColliders[j]);
                }
                for (int j = 0; j < capColliders.Length; j++)
                {
                    Physics.IgnoreCollision(boxColliders[i], capColliders[j]);
                }
            }

        }
    }
}