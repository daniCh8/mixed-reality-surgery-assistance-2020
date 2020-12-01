using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;


public class FocusHandlerVisualizer : MonoBehaviour, IMixedRealityFocusHandler
{
    public GameObject screwPrefab;
    private bool onfocus = false;
    public GameObject ScrewVisualizer;
    private bool create ;
    private bool check;

    private void Awake()
    {
        create = true;
    }   

    private void Update() {
        //Debug.Log(onfocus);
        if (onfocus== true)
        {
            ScrewVisualizer.SetActive(true);
            foreach(var source in MixedRealityToolkit.InputSystem.DetectedInputSources)
            {
                // Ignore anything that is not a hand because we want articulated hands
                if (source.SourceType == Microsoft.MixedReality.Toolkit.Input.InputSourceType.Hand)
                {
                    foreach (var p in source.Pointers)
                    {

                        if (p is IMixedRealityNearPointer)
                        {
                            // Ignore near pointers, we only want the rays
                            continue;
                        }
                        if (p.Result != null)
                        {
                            var pos = p.Result.Details.Point;
                            Vector3 p1 = ScrewSceneController.LerpByDistance(ScrewSceneController.AddScrewPoint, pos, -0.1f);
                            Vector3 p2 = ScrewSceneController.LerpByDistance(pos, ScrewSceneController.AddScrewPoint, -0.1f);
                            ScrewVisualizer = CreateCylinderBetweenPoints(p1, p2, create);
                            create = false;
                            // Debug.Log("Collider state" + ScrewVisualizer.GetComponent<Collider>().enabled);
                        }
                    }
                }
            }
        }
        else
        {
            // ScrewVisualizer.SetActive(false);

        }
    }

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {
        if (ScrewSceneController.AddingScrewFirstIndicator == false && ScrewSceneController.AddingScrewSecondIndicator == true)
        {
            onfocus = true;
            // Debug.Log("On Bone Focus");
            ScrewVisualizer.SetActive(true);
        }
    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {
        if (ScrewSceneController.AddingScrewFirstIndicator == false && ScrewSceneController.AddingScrewSecondIndicator == false && onfocus == true)
        {
            // Debug.Log("Leaving Bone Focus");
            create = true;
            onfocus = false;
            ScrewVisualizer.SetActive(false);

        }
        else 
        {
            onfocus = false;
            // ScrewVisualizer.SetActive(false);
        }

        // ScrewVisualizer.SetActive(false);

    }

    private GameObject CreateCylinderBetweenPoints(Vector3 start, Vector3 end, bool create)
    {
        var offset = end - start;
        var scale = new Vector3(0.01F, offset.magnitude / 2.0f, 0.01F);
        var position = start + (offset / 2.0f);

        if (create){
            var cylinder = Instantiate(screwPrefab, position, Quaternion.identity);
            cylinder.transform.up = offset;
            cylinder.transform.localScale = scale;
            cylinder.GetComponent<Collider>().enabled = false;

        return cylinder;
        }
        else
        {
            ScrewVisualizer.transform.up = offset;
            ScrewVisualizer.transform.localScale = scale;
            ScrewVisualizer.transform.position = position;
            ScrewVisualizer.GetComponent<Collider>().enabled = false;

            return ScrewVisualizer;
        }
    }
}