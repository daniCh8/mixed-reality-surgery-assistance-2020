# [Mixed Reality Lab](http://www.vvz.ethz.ch/Vorlesungsverzeichnis/lerneinheit.view?lerneinheitId=139691&semkez=2020W&ansicht=KATALOGDATEN&lang=en) 2020, Course Project

- [Mixed Reality Lab 2020, Course Project](#mixed-reality-lab-2020-course-project)
  - [Team](#team)
  - [CT-Scans](#ct-scans)

## Team 
- **Dominik Alberto** ([@doalberto](https://github.com/doalberto))<br>doalbert@student.ethz.ch
- **Daniele Chiappalupi** ([@daniCh8](https://github.com/daniCh8))<br>dchiappal@student.ethz.ch
- **Jorel Elmiger** ([@elmigerj](https://github.com/elmigerj))<br>elmigerj@student.ethz.ch
- **Elena Iannucci** ([@eleiannu](https://github.com/eleiannu))<br>eiannucci@student.ethz.ch
- **Hamza Javed** ([@hamzajaved05](https://github.com/hamzajaved05))<br>javedh@student.ethz.ch

## CT-Scans
The medical scans we are provided with are in DICOM format. The pipeline we followed to use those scans is the following:
- We used [3D Slicer](https://www.slicer.org/) to turn them from binary format to [`nrrd`](https://en.wikipedia.org/wiki/Nrrd) format. Inside **Slicer**, we imported the *DICOM* folder containing the files, and saved the data in `nrrd` extension.
- `nrrd` format is a very broad one. To ease our computations, we allow only a specific `nrrd` setting in our project. To convert any `nrrd` file to the required settings, we used [this python script](/Assets/CT/convert.py) from an older version of the project. Note that this script will override the existing `nrrd` file.
- Finally, we can use the processed file inside our project. To do so, since in **Unity** we only work with *Text Assets*, we need to rename the `nrrd` file to have a `bytes` extension (i.e. from `ct.nrrd` to `ct.bytes`). This file can be then provided to a script inside **Unity** to read it. Scripts able to do so can be found in [`Assets/CT/`](/Assets/CT/).