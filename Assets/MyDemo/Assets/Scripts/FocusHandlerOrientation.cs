using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;


public class FocusHandlerOrientation : MonoBehaviour
{
    public GameObject screwPrefab;
    private bool onfocus = false;
    public GameObject ScrewVisualizer;
    private bool create;
    public GameObject HandPlaneScrew;

    private void Awake()
    {
        create = true;
    }   

    private void Update() 
    {
        Vector3 pos = HandPlaneScrew.GetComponent<HandPlaneScrew>().getNormal();
        Debug.Log(pos);
        Vector3 FirstPoint = ScrewSceneController.AddScrewPoint;
        Vector3 SecondPoint = FirstPoint + 0.1f * pos; 
        Vector3 p1 = ScrewSceneController.LerpByDistance(FirstPoint, SecondPoint, -0.1f);
        Vector3 p2 = ScrewSceneController.LerpByDistance(SecondPoint, FirstPoint, -0.1f);
        ScrewVisualizer = CreateCylinderBetweenPoints(p1, p2, create);
        create = false;
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