using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;

public class ScalePivot : MonoBehaviour
{
    private GameObject pivot;
    private GameObject screwparent;

    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 position_pivot;

    private bool selected;

    private Vector3 direction;


    private void Start()
    {
        //returns world space direction that points along the direction of the screw orientation
        direction = this.transform.TransformDirection(Vector3.up);

        // get parameters of screw with resp to World Space
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;
        originalScale = this.transform.lossyScale;
        float screwlength = Mathf.Max(originalScale.x, originalScale.y, originalScale.z);

        screwparent = this.transform.parent.gameObject; //current parent of the screw

        //create a new parent for the screw
        pivot = new GameObject("Pivot");

        pivot.transform.position = originalPosition; // will be adapdet for every screw
        pivot.transform.rotation = originalRotation;
        pivot.transform.localScale = originalScale;
        pivot.transform.SetParent(screwparent.transform, true);

        // add bounds control to pivot 
        pivot.gameObject.AddComponent<BoundsControl>();
        pivot.gameObject.GetComponent<BoundsControl>().enabled = false;
        // pivot.gameObject.AddComponent<ScaleConstraint>();
        pivot.gameObject.AddComponent<ScrewScaleConstraint>();



        //transform the screw, align its end point closest to the lat/med plate to the center of the pivot objec
        GameObject latPlate = GameObject.Find("Plate_Lat");
        GameObject medPlate = GameObject.Find("Plate_Med");

        //set the child position of the screw to its corresponding plate
        //using the fact that the lat plate is closer to the origin in z direction than the med plate
        Vector3 ep1 = gameObject.transform.position + gameObject.transform.up * gameObject.transform.localScale.y / 2;//in world space
        Vector3 ep2 = gameObject.transform.position - gameObject.transform.up * gameObject.transform.localScale.y / 2;
        if (gameObject.tag == "Lat")
        {
            double d1 = (ep1.z - latPlate.transform.position.z);
            double d2 = (ep2.z - latPlate.transform.position.z);
            if (d1 < d2)
            {
                this.transform.localPosition = new Vector3(0, -1, 0);
                pivot.transform.position = pivot.transform.position + direction * screwlength; // adjust position of pivot such that center of pivot lies at one end of the screw



            }
            else
            {
                this.transform.localPosition = new Vector3(0, 1, 0);
                pivot.transform.position = pivot.transform.position - direction * screwlength; // adjust position of pivot such that center of pivot lies at one end of the scre



            }
        }
        else
        {
            double d1 = (ep1.z - medPlate.transform.position.z);
            double d2 = (ep2.z - medPlate.transform.position.z);
            if (d1 < d2)
            {
                this.transform.localPosition = new Vector3(0, 1, 0);
                pivot.transform.position = pivot.transform.position - direction * screwlength; // adjust position of pivot such that center of pivot lies at one end of the scre


            }
            else
            {
                this.transform.localPosition = new Vector3(0, -1, 0);
                pivot.transform.position = pivot.transform.position + direction * screwlength; // adjust position of pivot such that center of pivot lies at one end of the scre

            }
        }

        position_pivot = pivot.transform.position;
        this.transform.localRotation = Quaternion.identity;
        this.transform.localScale = new Vector3(1, 1, 1);


        //set pivot as parent
        this.transform.SetParent(pivot.transform, false);

        //adjust handles size by de/reactivating pivot gameobject
        pivot.SetActive(false);
        pivot.SetActive(true);

        // such that box is ignored by collision detection?
        pivot.gameObject.GetComponentInChildren<BoxCollider>().enabled = false;
    }

    private void Update()
    {
        selected = this.GetComponent<OnTrigger>().selectedFlag;

        if (selected)
        {
            if (pivot.gameObject.GetComponent<BoundsControl>().isActiveAndEnabled == true) // boundscontrol of pivot is already enabled
            {
                pivot.transform.position = position_pivot;//adjust the position of the pivot once the scaling handles are released as the scale constraint changes the position of the screw
            }
            else
            {
                //enable boundscontrol of pivot             
                pivot.gameObject.GetComponent<BoundsControl>().enabled = true;
                //remove bounds control of screw
                this.gameObject.GetComponent<BoundsControl>().enabled = false;


                //rescale handles
                pivot.SetActive(false);
                pivot.SetActive(true);
            }




        }
        else
        {
            if (pivot.gameObject.GetComponent<BoundsControl>().isActiveAndEnabled == true) // boundscontrol of pivot is still enabled
            {
                // remove boundscontrol  of pivot
                pivot.gameObject.GetComponent<BoundsControl>().enabled = false;

                // enable Bounds Control of screw again
                //this.gameObject.GetComponent<BoundsControl>().enabled = true;
            }

        }

    }


}