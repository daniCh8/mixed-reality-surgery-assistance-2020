using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTrigger : MonoBehaviour
{
    private Material defaultMat;
    private Renderer rend;
    public Material collidingMaterial, selectedScrewMaterial;
    public bool selectedFlag;

    // Start is called before the first frame update
    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
        defaultMat = rend.material;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.transform.parent ||
            other.gameObject.transform.parent != gameObject.transform.parent)
        {
            return;
        }

        Debug.Log(gameObject.name + " was triggered by " + other.gameObject.name);
        rend.material = collidingMaterial;

    }

    public void OnTriggerStay(Collider other)
    {
        return;
    }


    public void OnTriggerExit(Collider other)
    {
        if (selectedFlag)
        {
            rend.material = selectedScrewMaterial;
        }
        else
        {
            rend.material = defaultMat;
        }
    }


}