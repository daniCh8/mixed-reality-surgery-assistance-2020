using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Linq;
using UnityEngine;

public class SliderSlice : MonoBehaviour {
    public CTReader ct;
    public int width, height;

    public PinchSlider slider;
    public enum Axis { X, Y, Z };
    public Axis axis;

    public enum ColorFlag { Emerald, Yellow, Rose }
    public ColorFlag colorTexture;

    Texture2D tex;
    float currentVal = -1;

    void Start() {
        Color textureColor = Color.black;
        switch (colorTexture)
        {
            case ColorFlag.Emerald:
                textureColor = new Color32(37, 160, 149, 1);
                break;
            case ColorFlag.Yellow:
                textureColor = new Color32(175, 162, 54, 1);
                break;
            case ColorFlag.Rose:
                textureColor = new Color32(233, 196, 188, 1);
                break;
        }
        tex = NewTexture(width, height, textureColor);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    private Texture2D NewTexture(int width, int height, Color color)
    {
        var texture = new Texture2D(width, height);
        Color[] pixels = Enumerable.Repeat(color, width * height).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
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
