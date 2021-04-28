using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

public class CTReader : MonoBehaviour
{
    public TextAsset ct;
    public ComputeShader slicer;
    public float ctLength, ctDepth;
    public Vector3 ctCenter;
    public GameObject oo;
    public GameObject sliderH;
    public GameObject sliderV;
    int kernel;

    public GameObject bottomBackLeft;
    public GameObject bottomBackRight;
    public GameObject topBackLeft;
    public GameObject topBackRight;
    public GameObject bottomFrontLeft;
    public GameObject bottomFrontRight;
    public GameObject topFrontLeft;
    public GameObject topFrontRight;
    public GameObject center;

    void Start() {
        Init();
    }

    public void Init()
    {
        var nrrd = new NRRD(ct);

        kernel = slicer.FindKernel("CSMain");
        var buf = new ComputeBuffer(nrrd.data.Length, sizeof(float));
        buf.SetData(nrrd.data);
        slicer.SetBuffer(kernel, "data", buf);
        slicer.SetInts("dims", nrrd.dims);

        float lengthDirection = nrrd.lengthDirection, lengthSize = nrrd.dims[2];
        ctLength = Math.Abs(lengthDirection * lengthSize);
        float depthDirection = nrrd.depthDirection, depthSize = nrrd.dims[1];
        ctDepth = Math.Abs(depthDirection * depthSize);
        ctCenter = new Vector3(
            -1 * (nrrd.origin.x + ((int)Math.Ceiling((double)(nrrd.dims[0] / 2) - 1) * nrrd.scale.x)),
            nrrd.origin.y + ((int)Math.Ceiling((double)(nrrd.dims[1] / 2) - 1) * nrrd.scale.y),
            nrrd.origin.z + ((int)Math.Ceiling((double)(nrrd.dims[2] / 2) - 1) * nrrd.scale.z)
            );

        int rounds = 2;
        float minx = float.MaxValue,
                maxx = float.MinValue,
                miny = float.MaxValue,
                maxy = float.MinValue,
                minz = float.MaxValue,
                maxz = float.MinValue;
        for (int i = 0; i < rounds; i++)
        {
            // i : 10 == x : dim --> x = dim*i/10
            int dx = (int)(nrrd.dims[0] * i / (rounds-1));
            for(int j = 0; j < rounds; j++)
            {
                int dy = (int)(nrrd.dims[1] * j / (rounds-1));
                for (int k = 0; k < rounds; k++)
                {
                    int dz = (int)(nrrd.dims[2] * k / (rounds-1));
                    float x = nrrd.origin.x + dx * nrrd.scale.x;
                    float y = nrrd.origin.y + dy * nrrd.scale.y;
                    float z = nrrd.origin.z + dz * nrrd.scale.z;
                    minx = Math.Min(minx, x);
                    maxx = Math.Max(maxx, x);
                    miny = Math.Min(miny, y);
                    maxy = Math.Max(maxy, y);
                    minz = Math.Min(minz, z);
                    maxz = Math.Max(maxz, z);
                }
            }
        }

        bottomBackLeft = CreateSphereFromPos(minx, miny, minz, "bottomBackLeft");
        bottomBackRight = CreateSphereFromPos(minx, miny, maxz, "bottomBackRight");
        topBackLeft = CreateSphereFromPos(minx, maxy, minz, "topBackLeft");
        topBackRight = CreateSphereFromPos(minx, maxy, maxz, "topBackRight");
        bottomFrontLeft = CreateSphereFromPos(maxx, miny, minz, "bottomFrontLeft");
        bottomFrontRight = CreateSphereFromPos(maxx, miny, maxz, "bottomFrontRight");
        topFrontLeft = CreateSphereFromPos(maxx, maxy, minz, "topFrontLeft");
        topFrontRight = CreateSphereFromPos(maxx, maxy, maxz, "topFrontRight");
        Vector3 ccct = FindCenter(minx, maxx, miny, maxy, minz, maxz);
        center = CreateSphereFromPos(ccct.x, ccct.y, ccct.z, "center");
        center.transform.localScale = new Vector3(0f, 0f, 0f);
        Debug.Log(1);
        /*
        sliderH.transform.position = FindCenter(minx, maxx, miny, maxy, minz, maxz);
        sliderH.transform.parent = oo.transform;
        sliderH.transform.localScale = new Vector3(400f, 400f, 400f);
        sliderH.transform.eulerAngles = new Vector3(0f, 90f, 0f);

        sliderV.transform.position = FindCenter(minx, maxx, miny, maxy, minz, maxz);
        sliderV.transform.parent = oo.transform;
        sliderV.transform.localScale = new Vector3(400f, 400f, 400f);
        sliderV.transform.eulerAngles = new Vector3(180f, 0f, 0f);
        */

        oo.transform.position = new Vector3(0.941f, 0.75f, 1.729f);
        oo.transform.eulerAngles = new Vector3(0f, 90f, 0f);
        oo.transform.localScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

        /*
        PinchSlider psH = sliderH.GetComponentInChildren<PinchSlider>();
        float sliderLength = (Math.Abs(bottomBackRight.transform.localPosition.z - bottomBackLeft.transform.localPosition.z))
            / (2 * sliderH.transform.localScale.z);
        psH.SliderStartDistance = sliderLength;
        psH.SliderEndDistance = -sliderLength;
        // 1 : .250 = x : 2*sliderLength --> x = .9725 / .250
        float newScaleX = (2 * sliderLength) / 0.250f;
        psH.transform.GetChild(0).transform.localScale = new Vector3(newScaleX, 1f, 1f);
        psH.transform.localPosition = new Vector3(bottomFrontLeft.transform.localPosition.x, 
            bottomFrontLeft.transform.localPosition.y, 
            psH.transform.localPosition.z);

        PinchSlider psV = sliderV.GetComponentInChildren<PinchSlider>();
        sliderLength = (Math.Abs(bottomFrontLeft.transform.localPosition.x - bottomBackLeft.transform.localPosition.x))
            / (2 * sliderV.transform.localScale.z);
        psV.SliderStartDistance = sliderLength;
        psV.SliderEndDistance = -sliderLength;
        newScaleX = (2 * sliderLength) / 0.250f;
        psV.transform.GetChild(0).transform.localScale = new Vector3(newScaleX, 1f, 1f);
        psV.transform.localPosition = new Vector3(psV.transform.localPosition.x,
            bottomFrontRight.transform.localPosition.y,
            bottomFrontRight.transform.localPosition.z);
        */
    }

    private GameObject CreateSphereFromPos(float x, float y, float z, String n)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(x, y, z);
        sphere.transform.localScale = new Vector3(6f, 6f, 6f);
        sphere.name = n;
        sphere.transform.parent = oo.transform;
        return sphere;
    }

    private Vector3 FindCenter(float minx, float maxx, float miny, float maxy, float minz, float maxz)
    {
        float middlex = (maxx + minx) / 2,
            middley = (maxy + miny) / 2,
            middlez = (maxz + minz) / 2;

        return new Vector3(middlex, middley, middlez);
    }

    public void Slice(Vector3 orig, Vector3 dx, Vector3 dy, Texture2D result) {
        var rtex = new RenderTexture(result.width, result.height, 1);
        rtex.enableRandomWrite = true;
        rtex.Create();
        slicer.SetTexture(kernel, "slice", rtex);
        slicer.SetInts("outDims", new int[] { rtex.width, rtex.height });

        var scale = GetComponent<Transform>().localScale.normalized;
        scale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
        dx = Vector3.Scale(dx, scale / rtex.width);
        dy = Vector3.Scale(dy, scale / rtex.height);

        slicer.SetFloats("orig", new float[] { orig.x, orig.y, orig.z });
        slicer.SetFloats("dx", new float[] { dx.x, dx.y, dx.z });
        slicer.SetFloats("dy", new float[] { dy.x, dy.y, dy.z });
        slicer.Dispatch(kernel, (rtex.width + 7) / 8, (rtex.height + 7) / 8, 1);

        RenderTexture.active = rtex;
        result.ReadPixels(new Rect(0, 0, rtex.width-32, rtex.height-32), 16, 16);
        result.Apply();
    }

    public Vector3 TransformWorldCoords(Vector3 p) {
        return GetComponent<Transform>().InverseTransformPoint(p);
    }
}

public class NRRD {
    readonly public Dictionary<String, String> headers = new Dictionary<String, String>();
    readonly public float[] data;
    readonly public int[] dims;

    readonly public float lengthDirection;
    readonly public float depthDirection;

    readonly public Vector3 origin = new Vector3(0, 0, 0);
    readonly public Vector3 scale = new Vector3(1, 1, 1);

    public NRRD(TextAsset asset) {
        using (var reader = new BinaryReader(new MemoryStream(asset.bytes))) {
            for (string line = reader.ReadLine(); line.Length > 0; line = reader.ReadLine()) {
                if (line.StartsWith("#") || !line.Contains(":")) continue;
                var tokens = line.Split(':');
                var key = tokens[0].Trim();
                var value = tokens[1].Trim();
                headers.Add(key, value);
            }

            if (headers["dimension"] != "3") throw new ArgumentException("NRRD is not 3D");
            if (headers["type"] != "float") throw new ArgumentException("NRRD is not of type float");
            if (headers["endian"] != "little") throw new ArgumentException("NRRD is not little endian");
            if (headers["encoding"] != "gzip") throw new ArgumentException("NRRD is not gzip encoded");

            dims = Array.ConvertAll(headers["sizes"].Split(), s => int.Parse(s));
            if (headers.ContainsKey("space origin")) {
                var origin = Array.ConvertAll(headers["space origin"].Substring(1, headers["space origin"].Length - 2).Split(','), v => float.Parse(v));
                this.origin = new Vector3(origin[0], origin[1], origin[2]);
            }
            if (headers.ContainsKey("space directions")) {
                var scale = Array.ConvertAll(headers["space directions"].Split(), s => Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v)));
                if (scale[0][0] == 0 || scale[1][1] == 0 || scale[2][2] == 0) throw new ArgumentException("NRRD has 0 scale value");
                if (scale[0][1] != 0 || scale[1][0] != 0 || scale[2][0] != 0 ||
                    scale[0][2] != 0 || scale[1][2] != 0 || scale[2][1] != 0) throw new ArgumentException("NRRD is not axis-aligned");
                this.scale = new Vector3(scale[0][0], scale[1][1], scale[2][2]);
                depthDirection = scale[1][1];
                lengthDirection = scale[2][2];
            }

            var mem = new MemoryStream();
            using (var stream = new GZipStream(reader.BaseStream, CompressionMode.Decompress)) stream.CopyTo(mem);
            data = new float[dims[0] * dims[1] * dims[2]];
            Buffer.BlockCopy(mem.ToArray(), 0, data, 0, data.Length * sizeof(float));
        }
    }
}

public static class BinaryReaderExtension {
    public static string ReadLine(this BinaryReader reader) {
        var line = new StringBuilder();
        for (bool done = false; !done;) {
            var ch = reader.ReadChar();
            switch (ch) {
                case '\r':
                    if (reader.PeekChar() == '\n') reader.ReadChar();
                    done = true;
                    break;
                case '\n':
                    done = true;
                    break;
                default:
                    line.Append(ch);
                    break;
            }
        }
        return line.ToString();
    }
}