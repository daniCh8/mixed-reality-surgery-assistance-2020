using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

public class SliderSlice : MonoBehaviour {
    public CTReader ct;
    public int width, height;

    public PinchSlider slider;
    public enum Axis { X, Y, Z };
    public Axis axis;

    Texture2D tex;
    float currentVal = -1;

    void Start() {
        tex = new Texture2D(width, height);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    void Update() {
        if (currentVal != slider.SliderValue) {
            currentVal = slider.SliderValue;
            var val = currentVal - 0.5f;

            var orig = Vector3.zero;
            var dx = Vector3.zero;
            var dy = Vector3.zero;
            switch (axis) {
                case Axis.X:
                    orig = new Vector3(val, 0, 0);
                    dx = new Vector3(0, 0, 1);
                    dy = new Vector3(0, 1, 0);
                    break;
                case Axis.Y:
                    orig = new Vector3(val, 0, 0);
                    dx = new Vector3(0, 0, 1);
                    dy = new Vector3(-1, 0, 0);
                    break;
                case Axis.Z:
                    orig = new Vector3(0, 0, val);
                    dx = new Vector3(1, 0, 0);
                    dy = new Vector3(0, 1, 0);
                    break;
            }

            ct.Slice(orig, dx, dy, tex);
        }
    }
}
