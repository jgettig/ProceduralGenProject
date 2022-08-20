# Procedural Generation Project

<img src="/screenshot/3D/19-August-2022_175815.png" alt="drawing" width="1000"/>

In this tech demo, I experimented with procedural generation to create a voxel-style map for a RimWorld- or Stonehearth-esque colony management game. Multiple steps of the process are documented below.

# Builds and Script Files
## Builds
To see the current state of the project, download the builds here:

- [Windows](Builds/Windows)
- [macOS](Builds/macOS.app)

### Controls
The controls for the program are as follows:

* Move: W, A, S, D keys
* Ascend: Space
* Descend: L Ctrl
* Move Fast: L Shift (Hold)
* Unlock Mouse: Esc
* Take Screenshot: F2 (Saves to screenshot folder in game directory)

## Script Files
These are all relevant script files to the end product of the demo. Nested items are under the script that primarily references/instantiates them.

- Main Game Controller: [CubeTiler.cs](CubeTestingScripts/CubeTiler.cs)
  - Individual Voxel Controller: [Cube.cs](CubeTestingScripts/Cube.cs)
  - Map Object: [FlatMap.cs](NoiseTestingScripts/FlatMap.cs)
    - Noise Generator: [GetNoise.cs](NoiseTestingScripts/GetNoise.cs)
- Camera Controller: [CameraController.cs](CameraController.cs)
- 2D Map Testing Controller: [NoiseGen.cs](NoiseTestingScripts/NoiseGen.cs)
- Generation ADTs (or ADTs that don't themselves require extensive scripting): [ADTs.cs](NoiseTestingScripts/ADTs.cs)

# Development Process
This section outlines planning, development, and future planned features.

## Planning
The original goal of this project was to do the following:
* Familiarize myself with the Unity development environment and with developing my own projects from scratch
* Learn the basics of procedural generation, including:
  * Generation Processes:
    * Perlin noise generation
    * Wave function collapse generation
    * Rule-based generation based on the result of either of the two above
  * Post-Processing and Smoothing
* Practice documenting a development process throughout with intent to show other people snapshots of said process

Before starting the project, I created a one-page notes sheet with a list of goals for the development, what I thought I would learn, and sketches of minimum vs finished products. It is attached below:

![Planning Image](/screenshot/2022-08-20_18-21-1.png)

## Development
While the overall goal of this project was to create a map that fills a cube in space, the minimum goal was to create a slice two voxels deep that represents ground level and demonstrates basic mountain and lake/river generation. Because of this, the map is seeded from a 2D image, and generated based on that. While the current build only has the two-voxel slice, future iterations are unlikely to change this base generation.

In addition, while major changes to generation were easily documentable, throughout the process of refining the basic map generation, the smoothing and other post-processing steps were being refined. Since that process is much less easy to track, it will be covered separately.

As such, the Development section will be split into "2D," "Post-Processing," and "3D."

### 2D Development
The first step in development was to create an environment suitable to view the 2D map without requiring too much overhead work that might break during development. I opted to use a UI image colored based on the `FlatMap` object that would store the map data. I also created buttons that would regenerate and screenshot the map, and drew raw perlin noise onto the image to test that everything was working correctly.

![Base Noise Image](/screenshot/Plain_Noise/01-August-2022_210500.png)

Next, since this was the noise I intended to use as a basis for the terrain generation, I began to white out areas with higher values, leaving areas that could be used as mountains. In hindsight, it would have been better to use LOW values for lower terrain and HIGH values for higher terrain, but this works just as well in the long run. As part of the white out process, I made sure I could edit the values of cutoffs at runtime so as to see changes made in realtime without regenerating the map.

|<img src="/screenshot/Whited_Noise/01-August-2022_210537.png" alt="drawing" width="300"/>|<img src="/screenshot/Whited_Noise/01-August-2022_210544.png" alt="drawing" width="300"/>|<img src="/screenshot/Whited_Noise/01-August-2022_210554.png" alt="drawing" width="300"/>|
|---|---|---|

The next step was to add colored terrain. I decided that the initial product would have 4 terrain types: Mountain, Rocky Ground, Grassy Ground, and Water. With some experimentation on cutoff points, I was left with this product:

![Colored Noise Image](/screenshot/Colored/01-August-2022_213036.png)

Given this result as a basis, I felt ready to begin some post-processing work and developing the product into a state where it looked good. The first major change I wanted to make was removing small patches of water, since they didn't look very good. At the same time, I implemented some smoothing functionality, which I continued to develop throughout the 2D development process. The full post-processing cycle is outlined in the Post-Processing section.

|Before|After|
|:---:|:---:|
|<img src="/screenshot/Lake_Removal/02-August-2022_210453.png" alt="drawing" width="450"/>|<img src="/screenshot/Lake_Removal/02-August-2022_210458.png" alt="drawing" width="450"/>|

Next I decided to cull mountains that were undersized similarly to lakes. At the time, I thought it would look good if there was a chance that a culled mountain be replaced with a rocky patch. Later in development, I decided that even with enhanced post-processing those rocky patches would not look good and removed the feature. During this same design iteration I removed rocky generation surrounding mountains with the intention of generating them in a more unique and varied way using 1D Perlin noise later on. I also marked the center of mass of each mountain and lake during this iteration. While the feature was not used in this version of the demo, I expect to use both in the future, especially for mountains, to inform rules for 3D generation. 

Below are 3 samples of terrain generation after this iteration.

|<img src="/screenshot/Culled_Mountains_and_Marked_COMS/03-August-2022_161815.png" alt="drawing" width="300"/>|<img src="/screenshot/Culled_Mountains_and_Marked_COMS/03-August-2022_161817.png" alt="drawing" width="300"/>|<img src="/screenshot/Culled_Mountains_and_Marked_COMS/03-August-2022_161819.png" alt="drawing" width="300"/>|
|---|---|---|

Another problem I identified with the default perlin noise generation I had settled on was that mountain and rocky generation would often have solid 3-5 pixel wide straight offshoots. Adjusting the noise cutoff to a point where those offshoots no longer generated made for very undersized mountains, so I came up with a solution where mountain tiles, renamed `deepMountain`, would be generated with one noise cutoff, then tiles of a terrain type I named `thinMountain` would be generated with an offset cutoff. The `thinMountain` tiles would often generate in these straight line formations, but then any `thinMountain` tiles outside a defined distance of any `deepMountain` tiles would be removed. Remaining `thinMountain` tiles would then be converted into `deepMountain` tiles.

At the same time, I changed mountain generation to identify the pixels at the borders of mountains for use later in the new rocky generation.

Before and after images of this new generation are below. Magenta tiles in the images below are `thinMountain` tiles, and orange tiles are mountain borders.

|Before|After|
|:---:|:---:|
|<img src="/screenshot/Trimming_Offshoots/12-August-2022_184546.png" alt="drawing" width="450"/>|<img src="/screenshot/Trimming_Offshoots/12-August-2022_184548.png" alt="drawing" width="450"/>|
|<img src="/screenshot/Trimming_Offshoots/12-August-2022_184601.png" alt="drawing" width="450"/>|<img src="/screenshot/Trimming_Offshoots/12-August-2022_184603.png" alt="drawing" width="450"/>|

The final change before re-implementing rocky generation was ordering mountain edges consecutively. Prior to this point, mountain edges were found at the same time that mountain tiles were discovered and centers of mass were marked. However, this meant that the edges were always in an undefined order, and in order to carve out rocky ground using noise and have the result be a smooth mountain surface, they would need to be stored in consecutive order. Before and after pictures of edge ordering are below. The gradient used in marking the edge progresses from desaturated orange at the 'beginning' of the edge to dark purple at the 'end.' It was unimportant whether edges be marked in clockwise or counterclockwise order, so notice that the only ordering apparent is within each edge.

|Before|After|
|:---:|:---:|
|<img src="/screenshot/Edge_Ordering/17-August-2022_214346.png" alt="drawing" width="450"/>|<img src="/screenshot/Edge_Ordering/17-August-2022_214351.png" alt="drawing" width="450"/>|
|<img src="/screenshot/Edge_Ordering/17-August-2022_214359.png" alt="drawing" width="450"/>|<img src="/screenshot/Edge_Ordering/17-August-2022_214405.png" alt="drawing" width="450"/>|

Last, I re-implemented rocky generation. The goal here was to iterate around the edge of each mountain and carve into the rock generation in the direction of the steepest gradient towards the mountain. Since I had only generated the heightmap using perlin noise, and not a gradient map, I used the [Sobel operator](https://en.wikipedia.org/wiki/Sobel_operator) and the heightmap to approximate a gradient map. Then, I used a voxel traversal algorithm developed by [Amanatides and Woo](https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.42.3443&rep=rep1&type=pdf) to walk along the gradient by a distance generated by 1D perlin noise and turn all mountain tiles on that path into rocky tiles. The final result is shown below.


<img src="/screenshot/RockyGen_Noise/noise.01.png" alt="drawing" width="1000"/>

### Post Processing

The complete process of terrain generation and postprocessing follows the flowchart below. Each step is numbered and explained below the flowchart.

![image](https://user-images.githubusercontent.com/111656562/185766976-1837d555-b9d2-4e69-814b-5d6341151767.png)

\* "If Changes Made" refers to whether smooth has changed any pixels

1. Find Height and Gradient Maps
   * This is fairly self explanatory. The heightmap is generated via Perlin noise, while Gradient map is generated using the Sobel operator.
2. Preliminarily Find Mountains
   * This task does the following:
     * Turns `thinMountain` into `deepMountain` following the rules outlined in the 2D Development section.
     * Finds and stores the location, member pixels, center of mass, and unordered edge of each mountain.
     * Culls mountains that are undersized and turns their member pixels into grass.
3. Find Mountains
   * This task does the following:
     * Forgets all currently known mountains
     * Finds and stores the location, member pixels, center of mass, and unordered edge of each mountain.
     * Finds and stores the circularity of each mountain (this metric was not used in this demo but will be in future versions).
4. Find Mountain Edges
   * This task finds the ordered edge of each mountain, only ever searching through the unordered edge and adjacent pixels for efficiency.
5. Smooth
   * This task has two parts:
   * Smooth Singles
     * Finds any single pixels or pixels sticking out of/into a group that only have one neighbor in a cardinal direction of the same terrain type and removes them.
   * Smooth Corners
     * Finds any mountain corners that make up a 2x2 of pixels and removes the sharp corner.
6. Generate Rocky Areas
   * This task iterates through ordered mountain edges and carves in, revealing rocky ground using the method outlined in the 2D Development section.
7. Find Lakes
   * This task finds and stores the location, member pixels and center of mass of each lake. It also culls any lakes that are undersized and turns their pixels into grass.

### 3D Development

The process of 3D development for this project was much less involved than the 2D process, and most of it was adapted from two tutorials. First I made a script that could generate a cube object with 6 separate faces so that I could choose not to render any face that was against another voxel. To create the cube, I loosely followed [this](https://www.youtube.com/watch?v=lyDJPVp7Oc8) tutorial. I then created a script that, using a `FlatMap` object, instantiated cubes with textures dependent on terrain and placed them as needed to convert the 2D map into a 2 layer 3D map.

The only other scripting required was to move the camera. For that I loosely followed [this](https://www.youtube.com/watch?v=3MEvyGjAIxc&t=189s) tutorial, adding certain controls specific to this project and correcting mistakes made by the original programmer.

The end result is shown below.

<img src="/screenshot/3D/19-August-2022_175625.png" alt="drawing" width="1000"/>

<img src="/screenshot/3D/19-August-2022_175757.png" alt="drawing" width="1000"/>

## Future Features
I do not plan to be done with this demo quite yet. In the future I would like to add the following features:

### 2D Features
**Mountains**
* Split mountains into multiple submountains if they are below a circularity threshold. This will also help with 3D generation having multiple peaks
* More advanced smoothing
* Different types of mountain/rocky ground stones (Ie granite, slate, marble, etc)

**Lakes/water**
* River Generation
* A global minimum water amount
* Water specific smoothing

**Grassy**
* Thick vs light grass
* Naturally generating paths/walkways

### 3D Features
**Mountains**
* Mountain verticality using a heuristically weighted version of the Wave Function Collapse algorithm
* Mountain overhangs (possibly generated along a cubic bezier curve)

**Lakes/water**
* Lake/river depth (maybe using the original noise map?)

**Other**
* Underground generation
* Valleys in open areas
* Caves
* 3D grass textures
