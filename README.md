### David's Road System is tool for procedural road generation.

https://github.com/AwixStudio/Road-System/assets/29355013/4377aab1-9dfa-4aff-bd0b-ac0935997fd2

### Road map:
- ~~Generate road mesh with bezier curve~~. (Added 1.0.0)
  - ~~specify number of lanes~~
  - ~~specify lines types (dashed, single, double)~~
  - ~~specify extra road side width~~
  - ~~specify green lane width~~
  - ~~specify road lane extensions/narrowings~~
  - ~~snap road to another road~~
  - ~~lock road to another road~~
  - ~~include asphalt material~~
- Generate crossroad mesh tool
- Parking generation 
- Pavements generation tool
  - fast pavement generation along road
  - generate curb between road and pavement
  - ability to deform shape of pavement with bezier curves
  - include pavement and curb mateiral
- Automatic randomized placing decals on road
  - include some types of decals (man holes, asphalt breaks, etc...)
- Generate street lamps along road
   - include few street lamps models
- Vehicles driving logic
   - collision detection
   - working on burst & jobs
- Traffic lights
- Generate roundbound mesh tool
- Pedestrain crossings
  - navmesh compatible
  - automatic placing pedestrian crossing sign
- Speed limit signs


### How to install:
1. In Unity open Package Manager. Window -> Package Manager.
2. Click the Plus button.
3. Select "add package from git URL"
4. https://github.com/AwixStudio/Road-System.git#main
5. Add.

### How to use:
1. GameObject -> RoadSystem -> Road.
2. Toggle gizmos ON.
3. Now you should be able to move gizmos and regenerate mesh (it should be visible if you assigned materials).
4. You can control lanes numbers, road sides etc... by RoadGenerator component assigned to object.
5. You can access extra curve operations by right click on BezierCurveMB component.![image](https://github.com/AwixStudio/Road-System/assets/29355013/b4bd17ca-2853-4aa4-b307-df56b7d2c4f6)
