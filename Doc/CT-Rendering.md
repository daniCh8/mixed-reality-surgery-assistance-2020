# CT Rendering



## Input Data

DICOM files have a somewhat complicated file structure and figuring out what belongs to what seemed tricky, not to mention reading the files. In general, HoloLens2 now runs on an ARM64 architecture while also being Windows-based, so finding or compiling libraries is a huge pain (believe me, I tried ^^).

As an alternative, I found that from the MITK-GEM application we got from Thomas, you can right-click>Save the CT scan to NRRD (nearly raw raster data) format, which is pretty much the simplest possible format for raster data.
One issue: the NRRD format still allows for like a million different datatypes/encodings. Writing a reader to handle all of these is a bit of a pain which I had done at first, but then starting the application took like 30 seconds, because it had to convert all this data first. So instead I just decided that our application would require a very specific format an wrote a small converter in Python that could be used to convert any NRRD to the required format.

That format is:
- little-endian
- 4-byte float
- scaled such that values range from 0 to 1
- gzip encoded
- axis-aligned

The converter doesn't handle axis-alignedness or other axis-strangeness, so that will just be a requirement for now.
To run the converter, make sure you have the `pynrrd` library installed with pip, then call the script with the file as an argument.
The input file will be overwritten with the new format.



## Loading in Unity

For Unity to be able to correctly use this file, it must be given as a "binary asset" and MUST have the extension ".bytes".

The scripts and everything are in Assets/CT.

I've simplified the interaction of how to render slices of the CT since Tuesday, so now I feel it should be quite easy to create any slice method.
To load the CT itself, create a GameObject (I usually use a Cube) and add the CTReader script to it. To the script, add the "ct.bytes" file and the "slicer.compute" shader.
The transform of this GameObject will be set to the position and scale read from the NRRD file at runtime.
I would turn off the Mesh Renderer and add the Bounding Box script if you want to see the volume, e.g. for use with hand-slicing.
For slider-slicing you can leave it invisible.



## Slicing

The CTReader script has the method:
```public void Slice(Vector3 orig, Vector3 dx, Vector3 dy, Texture2D result)```

This method returns a render of a slice through the CT in the result texture.
"orig" is the point in 3D-space that is used as the center of the rendered slice.
"dx" and "dy" represent the direction and dimension of the rendered slices axes.
Note that these values are in the local transform of the CT, which ranges from -0.5 to 0.5 along each axis.

Simple example: a slice in the XY-plane (z=0) rendering the whole CT to the texture with "up" being in y-direction would use:
```
orig = (0, 0, 0)
dx = (1, 0, 0)
dy = (0, 1, 0)
```

I have created 2 scripts which handle this for different applications. Both will set the texture of the Object they are attached to. Both need to be passed the CT scan GameObject described above, as well as the width and height of the texture to be created.

1. `SliderSlice`: Slices based on the position of a PinchSlider. Value must range from -0.5 to 0.5. Axis must also be defined.
2. `HandSlice`: Slices based on hand position. Left or right hand can be chosen. A reference plane to indicate the current slicing plane can also be enabled.
