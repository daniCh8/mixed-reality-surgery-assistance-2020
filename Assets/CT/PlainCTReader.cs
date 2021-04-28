using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlainCTReader : MonoBehaviour
{
    /*
    public TextAsset ct;
    public ComputeShader slicer;
    public float ctLength, ctDepth;
    public Vector3 ctCenter;
    int kernel;

    void Start()
    {
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
    }

    public void Slice(Vector3 orig, Vector3 dx, Vector3 dy, Texture2D result)
    {
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
        result.ReadPixels(new Rect(0, 0, rtex.width - 32, rtex.height - 32), 16, 16);
        result.Apply();
    }

    public Vector3 TransformWorldCoords(Vector3 p)
    {
        return GetComponent<Transform>().InverseTransformPoint(p);
    }
}

public class NRRD
{
    readonly public Dictionary<String, String> headers = new Dictionary<String, String>();
    readonly public float[,,] data;
    readonly public int[] dims;

    readonly public float lengthDirection;
    readonly public float depthDirection;

    readonly public Vector3 origin = new Vector3(0, 0, 0);
    readonly public Vector3 scale = new Vector3(1, 1, 1);

    public NRRD(TextAsset asset)
    {
        using (var reader = new BinaryReader(new MemoryStream(asset.bytes)))
        {
            for (string line = reader.ReadLine(); line.Length > 0; line = reader.ReadLine())
            {
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
            if (headers.ContainsKey("space origin"))
            {
                var origin = Array.ConvertAll(headers["space origin"].Substring(1, headers["space origin"].Length - 2).Split(','), v => float.Parse(v));
                this.origin = new Vector3(origin[0], origin[1], origin[2]);
            }
            if (headers.ContainsKey("space directions"))
            {
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
            data = new float[dims[0], dims[1], dims[2]];
            float[] dataBox = new float[dims[0] * dims[1] * dims[2]];
            Buffer.BlockCopy(mem.ToArray(), 0, dataBox, 0, data.Length * sizeof(float));
        }
    }
}

public static class BinaryReaderExtension
{
    public static string ReadLine(this BinaryReader reader)
    {
        var line = new StringBuilder();
        for (bool done = false; !done;)
        {
            var ch = reader.ReadChar();
            switch (ch)
            {
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
    }*/
}
