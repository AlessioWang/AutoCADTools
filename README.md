# AutoCADTools

## Introduction
- This project is a pre-research of **RuralSewage project** for [Inst.AAA](https://github.com/Inst-AAA).
- AutoCADTools is a project which can help developer to develop AutoCAD extentions easier.
- Encapsulate some functions and apis from *Autodesk.AutoCAD.** to simplfy code.

## Dependences
- accoremgd
- AcCui
- Acdbmgd
- Acmgd
- AdWindows
- PresentationCore
- System.Windows.Forms
- WindowsBase

## How to Use
- **BasicTools Package**
  - AutoGeos: Create basic AutoCAD entities. 
  - AutoTools: Some basic entities operations.
  - GeoCalculator: [Some geomrtry algorithm](https://blog.csdn.net/zouzouol/article/details/82892849).
- **Demo Package**
  - A parkinglot drawing demo using BasicTools. 
  - Support commands:
    - SimpleLineParking
    - MultiLineParking
    - CalculateUnit
