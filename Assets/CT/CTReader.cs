using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

public class CTReader : MonoBehaviour
{
    public TextAsset ct;
    public ComputeShader slicer;
    public GameObject oo;
    public GameObject sliderH, sliderV;
    public GameObject quadH, revQuadH, quadV, revQuadV;
    int kernel;

    [HideInInspector]
    public GameObject bottomBackLeft, bottomBackRight, topBackLeft,
        topBackRight, bottomFrontLeft, bottomFrontRight, topFrontLeft,
        topFrontRight, center;
    [HideInInspector]
    public float ctLength, ctDepth;
    [HideInInspector]
    public Vector3 ctCenter;
    [HideInInspector]
    public byte[] ct_bytes;

    private float minx, maxx, miny, maxy, minz, maxz, width, height, depth;
    private NRRD nrrd;

    public void Init()
    {
        nrrd = new NRRD(ct_bytes);

        kernel = slicer.FindKernel("CSMain");
        var buf = new ComputeBuffer(nrrd.data.Length, sizeof(float));
        buf.SetData(nrrd.data);
        slicer.SetBuffer(kernel, "data", buf);
        slicer.SetInts("dims", nrrd.dims);
        PointCloud(nrrd);
    }

    private void PointCloud(NRRD nrrd)
    {
        DummyTransformHandler dummyHandler = oo.GetComponent<DummyTransformHandler>();
        dummyHandler.GoToZero();

        float lengthDirection = nrrd.lengthDirection, lengthSize = nrrd.dims[2];
        ctLength = Math.Abs(lengthDirection * lengthSize);
        float depthDirection = nrrd.depthDirection, depthSize = nrrd.dims[1];
        ctDepth = Math.Abs(depthDirection * depthSize);
        ctCenter = new Vector3(
            -1 * (nrrd.origin.x + ((int)Math.Ceiling((double)(nrrd.dims[0] / 2) - 1) * nrrd.scale.x)),
            nrrd.origin.y + ((int)Math.Ceiling((double)(nrrd.dims[1] / 2) - 1) * nrrd.scale.y),
            nrrd.origin.z + ((int)Math.Ceiling((double)(nrrd.dims[2] / 2) - 1) * nrrd.scale.z)
            );

        ComputeMinMaxFloats(nrrd);

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

        width = maxz - minz;
        height = maxy - miny;
        depth = maxx - minx;
        Vector3 quadLocalScaleH = new Vector3(depth, height, 1f),
            revQuadLocalScaleH = new Vector3(-depth, height, 1f),
            quadLocalScaleV = new Vector3(width, height, 1f),
            revQuadLocalScaleV = new Vector3(width, -height, 1f);
        quadH.transform.localScale = quadLocalScaleH * 0.0025f;
        revQuadH.transform.localScale = revQuadLocalScaleH * 0.0025f;
        quadV.transform.localScale = quadLocalScaleV * 0.0025f;
        revQuadV.transform.localScale = revQuadLocalScaleV * 0.0025f;

        dummyHandler.ChangeTransform(new Vector3(0.941f, 0.75f, 1.729f),
            new Vector3(0f, 90f, 0f),
            new Vector3(0.0025f, 0.0025f, 0.0025f));
    }

    public void ComputeOffsets()
    {
        foreach (var pt in GetPoints())
        {
            pt.GetComponent<AutoAlign>().ComputeOffset();
        }
    }

    private void ComputeMinMaxFloats(NRRD nrrd)
    {
        int rounds = 2;
        minx = float.MaxValue;
        maxx = float.MinValue;
        miny = float.MaxValue;
        maxy = float.MinValue;
        minz = float.MaxValue;
        maxz = float.MinValue;

        for (int i = 0; i < rounds; i++)
        {
            // i : 10 == x : dim --> x = dim*i/10
            int dx = (int)(nrrd.dims[0] * i / (rounds - 1));
            for (int j = 0; j < rounds; j++)
            {
                int dy = (int)(nrrd.dims[1] * j / (rounds - 1));
                for (int k = 0; k < rounds; k++)
                {
                    int dz = (int)(nrrd.dims[2] * k / (rounds - 1));
                    float x = -1 * (nrrd.origin.x + dx * nrrd.scale.x);
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
    }

    private GameObject CreateSphereFromPos(float x, float y, float z, String n)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(x, y, z);
        sphere.transform.localScale = new Vector3(0f, 0f, 0f);
        sphere.name = n;
        sphere.transform.parent = oo.transform;

        AutoAlign autoAlign = sphere.AddComponent<AutoAlign>();
        autoAlign.target = sliderH;

        return sphere;
    }

    private Vector3 FindCenter(float minx, float maxx, float miny, float maxy, float minz, float maxz)
    {
        float middlex = (maxx + minx) / 2,
            middley = (maxy + miny) / 2,
            middlez = (maxz + minz) / 2;

        return new Vector3(middlex, middley, middlez);
    }

    /*
    public Vector3 GetCenterOfCt(byte[] ctBytes)
    {
        var nrrd = new NRRD(ctBytes);

        DummyTransformHandler dummyHandler = oo.GetComponent<DummyTransformHandler>();
        dummyHandler.GoToZero();

        float lengthDirection = nrrd.lengthDirection, lengthSize = nrrd.dims[2];
        ctLength = Math.Abs(lengthDirection * lengthSize);
        float depthDirection = nrrd.depthDirection, depthSize = nrrd.dims[1];
        ctDepth = Math.Abs(depthDirection * depthSize);
        ctCenter = new Vector3(
            -1 * (nrrd.origin.x + ((int)Math.Ceiling((double)(nrrd.dims[0] / 2) - 1) * nrrd.scale.x)),
            nrrd.origin.y + ((int)Math.Ceiling((double)(nrrd.dims[1] / 2) - 1) * nrrd.scale.y),
            nrrd.origin.z + ((int)Math.Ceiling((double)(nrrd.dims[2] / 2) - 1) * nrrd.scale.z)
            );

        ComputeMinMaxFloats(nrrd);
        Vector3 ccct = FindCenter(minx, maxx, miny, maxy, minz, maxz);

        dummyHandler.RestoreBackup();
        return ccct;
    }
    */

    public GameObject[] GetPoints()
    {
        GameObject[] arr = new GameObject[] { bottomBackLeft, bottomBackRight, topBackLeft, topBackRight, bottomFrontLeft, bottomFrontRight, topFrontLeft, topFrontRight, center };
        return arr;
    }

    public bool NotReady()
    {
        return (center == null);
    }

    public void Slice(Vector3 orig, Vector3 dx, Vector3 dy, Texture2D result, bool disaligned, Vector4 bCol) {
        var rtex = new RenderTexture(result.width, result.height, 1);
        rtex.enableRandomWrite = true;
        rtex.Create();
        slicer.SetTexture(kernel, "slice", rtex);
        slicer.SetInts("outDims", new int[] { rtex.width, rtex.height });

        dx = dx * RtexConstants.SCALE;
        dy = dy * RtexConstants.SCALE;

        slicer.SetFloats("orig", new float[] { orig.x, orig.y, orig.z }); 
        slicer.SetFloats("dx", new float[] { dx.x, dx.y, dx.z });
        slicer.SetFloats("dy", new float[] { dy.x, dy.y, dy.z });
        slicer.SetFloats("borderColor", new float[] { bCol.x, bCol.y, bCol.z, bCol.w });
        slicer.Dispatch(kernel, (rtex.width + 7) / 8, (rtex.height + 7) / 8, 1);

        var oldRtex = RenderTexture.active;
        RenderTexture.active = rtex;
        result.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0);
        result.Apply();
        RenderTexture.active = oldRtex;
        rtex.Release();
    }

    public Vector3 TransformWorldCoords(Vector3 p) {
        return GetComponent<Transform>().InverseTransformPoint(p);
    }

    public Vector3 GetPositionFromSlider(float v, SliderSlice.Axis ax) {
        // (v+0.5) : 1 == x : delta
        Vector3 pos = new Vector3(center.transform.localPosition.x,
            center.transform.localPosition.y,
            center.transform.localPosition.z);
        switch (ax)
        {
            case SliderSlice.Axis.X:
                // depth
                pos.x = bottomFrontLeft.transform.localPosition.x +
                    ((bottomBackLeft.transform.localPosition.x - 
                    bottomFrontLeft.transform.localPosition.x) * (v + 0.5f));
                break;
            case SliderSlice.Axis.Y:
                // height
                pos.y = bottomFrontLeft.transform.localPosition.y +
                    ((topFrontLeft.transform.localPosition.y -
                    bottomFrontLeft.transform.localPosition.y) * (v + 0.5f));
                break;
            case SliderSlice.Axis.Z:
                // length
                pos.z = bottomFrontLeft.transform.localPosition.z +
                    ((bottomFrontRight.transform.localPosition.z -
                    bottomFrontLeft.transform.localPosition.z ) * (v + 0.5f));
                break;
        }
        return pos;
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

    public NRRD(byte[] bytes) {
        using (var reader = new BinaryReader(new MemoryStream(bytes))) {
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
                var origin = Array.ConvertAll(headers["space origin"].Substring(1, headers["space origin"].Length - 2).Split(','), v => float.Parse(v, CultureInfo.InvariantCulture));
                this.origin = new Vector3(origin[0], origin[1], origin[2]);
            }
            if (headers.ContainsKey("space directions")) {
                var scale = Array.ConvertAll(headers["space directions"].Split(), s => Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v, CultureInfo.InvariantCulture)));
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

            mem.Dispose();
            reader.Dispose();
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

public static class BorderColors
{
    public static readonly Vector4 YELLOW = new Vector4(1, 1, 0, 1);
    public static readonly Vector4 RED = new Vector4(1, 0, 0, 1);
    public static readonly Vector4 CYAN = new Vector4(0, 1, 1, 1);
}

static class RtexConstants
{
    public static readonly float SCALE = 0.001961119675f;
}