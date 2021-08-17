using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTrigger : MonoBehaviour
{
    private Color default_color;
    private Renderer rend;
    GameObject bone;
    GameObject bonemix;
    GameObject screwChild;
    GameObject pivot;
    GameObject screwobj;
    public Material selectedScrewMaterial;
    public bool selectedFlag;
    int counter;

    // Start is called before the first frame update
    void Start()
    {
        bone = GameObject.Find("Bone");
        bonemix = GameObject.Find("BoneMix");
        screwChild = GameObject.Find("screwChild");
        pivot = this.transform.parent.gameObject; //new

        rend = gameObject.GetComponent<Renderer>();
        default_color = rend.material.color;
    }


    public void OnTriggerEnter(Collider other)
    {
        //check null references
        if (!other.gameObject.transform.parent || !gameObject.transform.parent)
        {
            return;
        }
        //ignore collisions with handles
        if (other.gameObject.transform.parent.name == "rigRoot" || gameObject.transform.parent.name == "rigRoot")
        {
            return;
        }
        //ignore collisions with bone
        if (other.gameObject.transform.parent.transform.IsChildOf(bone.transform) || other.gameObject.transform == bonemix.transform)
        {
            return;
        }

        Debug.Log(gameObject.name + " was triggered by " + other.gameObject.name);

    }

    public void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.transform.parent || !gameObject.transform.parent)
        {
            return;
        }
        //ignore collisions with handles
        if (other.gameObject.transform.parent.name == "rigRoot" || gameObject.transform.parent.name == "rigRoot")
        {
            return;
        }
        //ignore collisions with bone
        if (other.gameObject.transform.parent.transform.IsChildOf(bone.transform) || other.gameObject.transform == bonemix.transform)
        {
            return;
        }
        else
        {
            rend.material.SetColor("_Color", Color.red);
        }
    }


    public void OnTriggerExit(Collider other)
    {
        //Debug.Log("No longer in contact with " + other.gameObject.name);

        // change default color to pink when screw is selected OnTriggerExit
        if (selectedFlag)
        {
            rend.material.SetColor("_Color", selectedScrewMaterial.color);
        }
        else
        {
            rend.material.SetColor("_Color", default_color);
        }
    }


}