using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;



public class FlatMap
{
    private Color[] colorArray;
    private pixel[,] pixels;
    private Vector2[,] gradMap;
    private float[,] heightMap;

    //TODO move dimension definitions here once they are set in stone
    private int width;
    private int height;
    private int terrainTypes;

    private cutoffs co;

    private float scale = 0.03f;

    List<lake> lakes;
    List<mountain> mountains;

    //TODO: move cutoffs here once they are set in stone
    //TODO: move critical lake mass here once it is set in stone
    //TODO: move water quota here once it is set in stone

    public FlatMap(int _width, int _height, int _terrainTypes) {
        co.deepMountain = 0.366f;
        co.thinMountain = co.deepMountain + 0.025f;
        co.grassy = 0.655f;
        co.lakeSize = 300;
        co.mtnSize = 300;
        co.searchDist = 5;
        co.doSearch = true;
        co.rockyScale = 0.01f;

        width = _width;
        height = _height;
        terrainTypes = _terrainTypes;
        pixels = new pixel[height, width];
        colorArray = new Color[width * height];
        gradMap = new Vector2[height, width];
        lakes = new List<lake>();
        mountains = new List<mountain>();
        for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x){
                pixels[y,x] = new pixel();
                pixels[y,x].loc = new Vector2Int(x,y);
            }
        }

        /*
        float[,] kernel = new float[3,3] {
            {0, -1, 0},
            {-1, 5, -1},
            {0, -1, 0}
        };

        float[,] image = new float[3,3] {
            {7,7,6},
            {7,7,6},
            {6,6,4},
        };

        Debug.Log(convolve(kernel, image));
        */
    }

    public void reset() {
        Array.Clear(pixels, 0, pixels.Length);
        Array.Clear(colorArray, 0, colorArray.Length);
        lakes.Clear();
        mountains.Clear();
        for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x){
                pixels[y,x] = new pixel();
                pixels[y,x].loc = new Vector2Int(x,y);
            }
        }
    }

    public Color[] GetColors() {
        return colorArray;
    }

    public pixel GetPixelAt(int x, int y) {
        return pixels[y,x];
    }

    public void setPixelAt(int x, int y, terrainType ground) {
        pixels[y,x].terrain = ground;
    }

    // Generate full gradient map using given cutoff, scale, and offset parameters
    // Gradients in this map will point towards lower value (ie into mountains)
    public void  Generate(Vector2 offset) {
        // Generate basic Perlin noise map to start
        heightMap = GetNoise.Generate(width, height, scale, offset);
        getGradMap();

        // Loop through each location in heightMap and assign a color in colorMap based on the value in heightmap
        for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x){
                if (heightMap[y,x] <= co.deepMountain) pixels[y,x].terrain = terrainType.deepMountain;
                else if (heightMap[y,x] <= co.thinMountain) pixels[y,x].terrain = terrainType.thinMountain;
                else if (heightMap[y,x] <= co.grassy) pixels[y,x].terrain = terrainType.grassy;
                else pixels[y,x].terrain = terrainType.water;
            }
        }


        
        prelimFindMountains(co.searchDist, co.mtnSize, co.doSearch);
        bool regen = false;
        do {
            postFindMountains();
            generateMountainEdges();
            regen = smooth();
        } while (regen);
        generateRocky(co.rockyScale);
        regen = false;
        do{
            postFindMountains();
            regen = smooth();
        } while (regen);
        findLakes(co.lakeSize);
    }

    private void getGradMap() {
        float[,] Gx = new float[3,3] {
            {-1, 0, 1},
            {-2, 0, 2},
            {-1, 0, 1}
        };

        float[,] Gy = new float[3,3] {
            {1, 2, 1},
            {0, 0, 0},
            {-1, -2, -1}
        };

        for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {

                //Generate 3x3 sample image using 'extend' edge rule
                float[,] imageSample = new float[3,3];

                imageSample[1,1] = heightMap[y,x];
                if (y == 0) {
                    if (x == 0) imageSample[0,0] = heightMap[y,x];
                    else imageSample[0,0] = heightMap[y,x-1];

                    if (x == width-1) imageSample[0,2] = heightMap[y,x];
                    else imageSample[0,2] = heightMap[y,x+1];

                    imageSample[0,1] = heightMap[y,x];
                }
                else {
                    if (x == 0) imageSample[0,0] = heightMap[y-1,x];
                    else imageSample[0,0] = heightMap[y-1, x-1];

                    if (x == width-1) imageSample[0,2] = heightMap[y-1,x];
                    else imageSample[0,2] = heightMap[y-1, x+1];

                    imageSample[0,1] = heightMap[y-1,x];
                }

                if (y == height-1) {
                    if (x == 0) imageSample[2,0] = heightMap[y,x];
                    else imageSample[2,0] = heightMap[y,x-1];

                    if (x == width-1) imageSample[2,2] = heightMap[y,x];
                    else imageSample[2,2] = heightMap[y,x+1];

                    imageSample[2,1] = heightMap[y,x];
                }
                else {
                    if (x == 0) imageSample[2,0] = heightMap[y+1,x];
                    else imageSample[2,0] = heightMap[y+1,x-1];

                    if (x == width-1) imageSample[2,2] = heightMap[y+1,x];
                    else imageSample[2,2] = heightMap[y+1,x+1];

                    imageSample[2,1] = heightMap[y+1,x];
                }

                if(x == 0) imageSample[1,0] = heightMap[y, x];
                else imageSample[1,0] = heightMap[y, x-1];

                if (x == width-1) imageSample[1,2] = heightMap[y, x];
                else imageSample[1,2] = heightMap[y, x+1];

                //convolve with Gx and Gy
                float xGrad = convolve(Gx, imageSample);
                float yGrad = convolve(Gy, imageSample);
                gradMap[y,x] = new Vector2(-xGrad, -yGrad);
            }
        }
    }
    

    // Takes two 3x3 arrays arranged in [y,x] order and reterns their convolution
    private float convolve(float[,] kernel, float[,] image) {
        float acc = 0f;
        for(int i = 0; i < 3; ++i) {
            for(int j = 0; j < 3; ++j) {
                acc += kernel[i, 2-j] * image[2-i,j];
            }
        }
        return acc;
    }

    // update colorArray to reflect terrain types in pixels
    public Color[] updateColors() {
        int i = 0;
        
        for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x){
                terrainType curTerrain = pixels[y,x].terrain;
                switch (curTerrain) {
                    case terrainType.deepMountain:
                        colorArray[i] = Color.black;
                        break;
                    case terrainType.thinMountain:
                        colorArray[i] = Color.magenta;
                        break;
                    case terrainType.rocky:
                        colorArray[i] = Color.gray;
                        break;
                    case terrainType.grassy:
                        colorArray[i] = Color.green;
                        break;
                    case terrainType.water:
                        colorArray[i] = Color.blue;
                        break;
                }

                if (pixels[y,x].edge) colorArray[i] = Color.white;

                ++i;
            }
        }

        foreach(mountain m in mountains) {
            for (int j = 0; j < m.edge.Count; ++j) {
                Vector2Int pLoc = m.edge[j].loc;
                colorArray[pLoc.x + pLoc.y*width] = Color.Lerp(new Color(0.96f, 0.72f, 0.45f), new Color(0.286f, 0f, 0.42f), (float)j/(float)m.edge.Count);
            }
            Vector2Int COMLoc = m.COM.loc;
            colorArray[COMLoc.x + COMLoc.y*width] = Color.red;
        }

        foreach(lake l in lakes) {
            Vector2Int COMLoc = l.COM.loc;
            colorArray[COMLoc.x + COMLoc.y*width] = Color.cyan;
        }

        return colorArray;
    }
    
    // smooths out individual pixels to make a more organic feeling environment
    private bool smooth() {
        return smoothSingles() || smoothCorners();
    }

    private bool smoothSingles() {
        // will use order north, east, south, west (w)
        terrainType[] neighbors = new terrainType[4];

        bool changedEdge = false;

        for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x){
                terrainType thisTerrain = pixels[y,x].terrain;
                body[] neighborBodies = new body[4];
                //--------------------Check for any single pixel offshoots--------------------

                // find cardinal neighbors
                if(x == 0) neighbors[3] = terrainType.none;
                else {
                    neighbors[3] = pixels[y,x-1].terrain;
                    neighborBodies[3] = pixels[y,x-1].group;
                }

                if(x == width-1) neighbors[1] = terrainType.none;
                else {
                    neighbors[1] = pixels[y,x+1].terrain;
                    neighborBodies[1] = pixels[y,x+1].group;
                }

                if(y == height-1) neighbors[0] = terrainType.none;
                else {
                    neighbors[0] = pixels[y+1,x].terrain;
                    neighborBodies[0] = pixels[y+1,x].group;
                }

                if(y == 0) neighbors[2] = terrainType.none;
                else {
                    neighbors[2] = pixels[y-1,x].terrain;
                    neighborBodies[2] = pixels[y-1,x].group;
                }

                int sameType = 0;
                int[] counts = new int[terrainTypes];

                // count how many neighbors of same type
                for(int i = 0; i < neighbors.Length; ++i) {
                    if (neighbors[i] == terrainType.none) continue;
                    if (neighbors[i] == thisTerrain || (Terrains.rockyTypes.Contains(thisTerrain) && Terrains.mountainTypes.Contains(neighbors[i]))) sameType++;
                    counts[(int)neighbors[i]]++;
                }

                // force rocky pixels with 3 mountain neighbors to become mountain. This removes single pixel
                // jut-ins while preserving any rocky pixels on the edge of the map with only 1 same neighbor
                if (thisTerrain == terrainType.rocky && counts[(int)terrainType.deepMountain] + counts[(int)terrainType.thinMountain] >= 3) {
                    sameType = 0;
                }

                if (sameType < 2) {
                    int max = counts.Max();
                    terrainType type;
                    if (max < counts[(int)terrainType.deepMountain] + counts[(int)terrainType.thinMountain]) type = terrainType.thinMountain;
                    else type = (terrainType)mostIndex(counts);

                    if (pixels[y,x].terrain == terrainType.rocky) {
                        Debug.Log("changing rocky terrain into " + type + " at " + x + ", " + y);
                    }

                    pixels[y,x].terrain = type;
                    for (int i = 0; i < 4; ++i) {
                        if (neighbors[i] == pixels[y,x].terrain) {
                            if (pixels[y,x].grouped) {
                                pixels[y,x].group.Remove(pixels[y,x]);
                            }
                            if (pixels[y,x].edge || Terrains.rockyTypes.Contains(pixels[y,x].terrain)) {
                                changedEdge = true;
                            }
                            pixels[y,x].group = neighborBodies[i];
                            break;
                        }
                    }

                    if (pixels[y,x].group == null) {
                        pixels[y,x].grouped = false;
                        pixels[y,x].edge = false;
                    }
                    else {
                        pixels[y,x].group.Add(pixels[y,x]);
                    }
                    
                }

                if (pixels[y,x].terrain == terrainType.thinMountain) {
                    Debug.Log("Thin mountain found with " + sameType + " similar cardinal neighbors");
                }
            }
        }

        return changedEdge;

    }

    private bool smoothCorners() {

        
        bool changedEdge = false;

        // array of offsets from a tile being checked. Each subarray represents a possible rotation. the elements 
        // of each array are such that [0] and [1] should be mountain and [2] and [3] should be grassy
        Vector2Int[,] cornerChecks = new Vector2Int[4,4] {
            {new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(-1,1), new Vector2Int(1,-1)},
            {new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(1,1), new Vector2Int(-1,-1)},
            {new Vector2Int(0,-1), new Vector2Int(-1,0), new Vector2Int(1,-1), new Vector2Int(-1,1)},
            {new Vector2Int(0,1), new Vector2Int(-1,0), new Vector2Int(-1,-1), new Vector2Int(1,1)}
        };

         for(int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x){
                terrainType thisTerrain = pixels[y,x].terrain;

                //--------------------Check for hard corners--------------------
                thisTerrain = pixels[y,x].terrain;
                Vector2Int curLoc = pixels[y,x].loc;

                // only check hard corners for mountains
                if (Terrains.mountainTypes.Contains(thisTerrain) && !isOnEdge(curLoc)) {
                    for(int i = 0; i < 4; ++i) {
                        pixel[] checkLocs = new pixel[4];
                        for (int j = 0; j < 4; ++j) {
                            Vector2Int loc = curLoc + cornerChecks[i,j];
                            checkLocs[j] = pixels[loc.y, loc.x];
                        }
 
                        bool isCorner = Terrains.mountainTypes.Contains(checkLocs[0].terrain) && Terrains.mountainTypes.Contains(checkLocs[1].terrain);
                        isCorner = isCorner && !Terrains.mountainTypes.Contains(checkLocs[2].terrain) && !Terrains.mountainTypes.Contains(checkLocs[3].terrain);

                        if (isCorner) {
                            pixels[y,x].terrain = terrainType.rocky;
                            if (pixels[y,x].grouped) {
                                pixels[y,x].group.Remove(pixels[y,x]);
                                pixels[y,x].grouped = false;
                                changedEdge = true;
                                //Debug.Log("Removed sharp corner at " + x + ", " + y);
                            }
                            break;
                        }
                    }
                    
                }
            }
        }

        return changedEdge;
    }

    private bool isOutOfBounds(Vector2Int loc) {
        return loc.x < 0 || loc.y < 0 || loc.x >= width || loc.y >= height;
    }

    private bool isOnEdge(Vector2Int loc)
    {
        return loc.x == 0 || loc.y == 0 || loc.x == width - 1 || loc.y == height - 1;
    }

    private bool isMountainEdge(pixel q) {
        Vector2Int[] adjacents = {
            new Vector2Int(1,0),
            new Vector2Int(-1,0),
            new Vector2Int(0,1),
            new Vector2Int(0,-1)
        };

        if (!Terrains.mountainTypes.Contains(q.terrain)) return false;

        Vector2Int loc = q.loc;
        List<pixel> checks = new List<pixel>();
        
        foreach (Vector2Int dir in adjacents) {
            Vector2Int cur = loc + dir;
            if (!isOutOfBounds(cur)) {
                checks.Add(pixels[cur.y, cur.x]);
            }
        }

        bool grassNeighbor = false;
        foreach (pixel p in checks) {
            grassNeighbor |= p.terrain == terrainType.grassy;
        }

        return grassNeighbor;
    }

    private void prelimFindMountains(float searchDist, int minMtnSize, bool doSearch) {

        List<Vector2Int> checkList = getPointsInDist(searchDist);
        List<pixel> toDeep = new List<pixel>();

        if (doSearch) {
            //cull down thinMountains to remove strange jut-outs but retain bulkier mountains
            foreach(pixel p in pixels) {
                if(p.terrain == terrainType.thinMountain) {
                    bool rem = true;
                    Vector2Int pLoc = p.loc;
                    foreach (Vector2Int off in checkList) {
                        Vector2Int cLoc = pLoc + off;

                        if(isOutOfBounds(cLoc)) continue;

                        if (pixels[cLoc.y, cLoc.x].terrain == terrainType.deepMountain) {
                            rem = false;
                            break;
                        }
                    }
                    if (rem) p.terrain = terrainType.grassy;
                    else toDeep.Add(p);
                }
            }
        }

        foreach (pixel p in toDeep) p.terrain = terrainType.deepMountain;

        populateMountains();

        //Cull undersized mountains
        foreach (mountain mtn in mountains) {
            float rockNum = UnityEngine.Random.Range(0f, 1f);
            if (mtn.size < minMtnSize) {
                foreach (pixel p in mtn.points) {
                    p.terrain = terrainType.grassy;
                }
                mtn.Clear();
            }
        }

        //Remove culled mountains from list
        for(int i = 0; i < mountains.Count; ++i) {
            if(mountains[i].size == 0) {
                mountains.Remove(mountains[i]);
                --i;
            }
        }

        Debug.Log(mountains.Count + " mountains remain after culling");
    }



    private void postFindMountains() {
        clearMountains();
        
        populateMountains();

        //Find circularity of mountains (NOT REQUIRED BEFORE GENERATING ROCKY)
        foreach(mountain m in mountains) {
            float dist = 0;
            foreach(pixel p in m.points) {
                dist += Vector2.Distance(p.loc, m.COM.loc);
            }
            dist /= m.size;
            m.circularity = 1 / ((dist/MathF.Sqrt(m.size)) * (3*Global.rootPi/2));
        }
    }

    private void clearMountains() {
        foreach (mountain m in mountains) {
            m.Clear();
        }
        mountains.Clear();

    }

    private void populateMountains() {

        //Find each mountain and populate mountains with them
        foreach(pixel p in pixels) {
            mountain mtn = new mountain();
            Stack<pixel> toCheck = new Stack<pixel>();
            if(!p.grouped && Terrains.mountainTypes.Contains(p.terrain)) {
                toCheck.Push(p);
                p.grouped = true;
            }

            int iter = 0;

            Vector2Int COMLoc = new Vector2Int();

            while(toCheck.Count != 0) {
                pixel curPixel = toCheck.Pop();
                Vector2Int curLoc = curPixel.loc;
                COMLoc += curLoc;
                curPixel.group = mtn;
                mtn.Add(curPixel);

                pixel pixelN = new pixel();
                pixel pixelE = new pixel();
                pixel pixelS = new pixel();
                pixel pixelW = new pixel();

                if(curLoc.y != 0) pixelN = pixels[curLoc.y-1, curLoc.x]; 
                else pixelN.terrain = terrainType.none;
                
                if(curLoc.x != width-1) pixelE = pixels[curLoc.y, curLoc.x+1]; 
                else pixelE.terrain = terrainType.none;
                
                if(curLoc.y != height-1) pixelS = pixels[curLoc.y+1, curLoc.x]; 
                else pixelS.terrain = terrainType.none;
                
                if(curLoc.x != 0) pixelW = pixels[curLoc.y, curLoc.x-1]; 
                else pixelW.terrain = terrainType.none;
                
                
                
                if(Terrains.mountainTypes.Contains(pixelN.terrain)) {
                    if (!pixelN.grouped) {
                        toCheck.Push(pixelN);  
                        pixelN.grouped = true;
                    }
                }
                else if (pixelN.terrain == terrainType.grassy && !curPixel.edge) {
                    curPixel.edge = true;
                    mtn.unorderedEdge.Add(curPixel);
                } 

                if(Terrains.mountainTypes.Contains(pixelE.terrain)) {
                    if (!pixelE.grouped) {
                        toCheck.Push(pixelE);  
                        pixelE.grouped = true;
                    }
                }
                else if (pixelE.terrain == terrainType.grassy && !curPixel.edge) {
                    curPixel.edge = true;
                    mtn.unorderedEdge.Add(curPixel);
                } 

                if(Terrains.mountainTypes.Contains(pixelS.terrain)) {
                    if (!pixelS.grouped) {
                        toCheck.Push(pixelS);  
                        pixelS.grouped = true;
                    }
                }
                else if (pixelS.terrain == terrainType.grassy && !curPixel.edge) {
                    curPixel.edge = true;
                    mtn.unorderedEdge.Add(curPixel);
                } 

                if(Terrains.mountainTypes.Contains(pixelW.terrain)) {
                    if (!pixelW.grouped) {
                        toCheck.Push(pixelW);  
                        pixelW.grouped = true;
                    }
                }
                else if (pixelW.terrain == terrainType.grassy && !curPixel.edge) {
                    curPixel.edge = true;
                    mtn.unorderedEdge.Add(curPixel);
                } 

                iter++;
                if (iter > 10000) {
                    return;
                }
            }
            if (mtn.size != 0)
            {
                COMLoc /= mtn.size;
                mtn.COM = pixels[COMLoc.y, COMLoc.x];
                mountains.Add(mtn);
            }
        }
        Debug.Log("Found " + mountains.Count + " mountains");

    }



    //Given a set radius, returns the set of vectors from a theoretical center pixel
    //to all other pixels whose centers fall within a circle of radius r from its center
    private List<Vector2Int> getPointsInDist(float r) {
        int maxDist = (int)MathF.Floor(r);
        List<Vector2Int> points = new List<Vector2Int>((int)MathF.Pow(maxDist * 2 + 1, 2));
        for (int y = -maxDist; y <= maxDist; ++y) {
            for(int x = -maxDist; x <= maxDist; ++x) {
                if (x == 0 && y == 0) continue;
                Vector2Int cur = new Vector2Int(x, y);
                if (cur.magnitude <= r) points.Add(cur);
            }
        }
        return points;
    }


    //Gets edges of all mountains and changes appropriate variable attributes to match
    //Note: individual edges will be consecutive within the array. That is, each pixel in
    //      a mountain's edge List will be either adjacent to the pixels before and after it or
    //      on a different edge of the same mountain
    private void generateMountainEdges() {

        foreach (mountain m in mountains) {
            m.clearEdges();
        }

        
        Vector2Int[] checkDirs = new Vector2Int[8] {
            new Vector2Int(0,1),
            new Vector2Int(1,1),
            new Vector2Int(1,0),
            new Vector2Int(1,-1),
            new Vector2Int(0,-1),
            new Vector2Int(-1,-1),
            new Vector2Int(-1,0),
            new Vector2Int(-1,1)
        };
        
        foreach (mountain m in mountains) {
            
            pixel curPoint = null;
            foreach (pixel p in m.unorderedEdge) {
                if (isOnEdge(p.loc)) {
                    curPoint = p;
                    break;
                }
            }

            if(curPoint == null) {
                curPoint = m.points[0];
            }
            curPoint.edge = true;
            m.edge.Add(curPoint);
            int dirFrom = -1;

            int innerLoops = 0;
            int outerLoops = 0;

            //outer loop covers multiple disjoint edges (I don't expect a mountain with 20 edges)
            while (outerLoops < 20) {

                Vector2Int curLoc = curPoint.loc;
                Vector2Int prevLoc;

                //inner loop runs through single edge
                while (innerLoops < height * width) {

                    prevLoc = curLoc;
                    bool complete = false;

                    //Check all 8 points adjacent to curPoint to see if they are undiscovered edge points
                    for (int i = 0; i < checkDirs.Count(); ++i) {
                        if (i == dirFrom) continue;

                        Vector2Int checkLoc = curLoc + checkDirs[i];
                        if(isOutOfBounds(checkLoc)){
                            continue;
                        }
                        
                        pixel checkPt = pixels[checkLoc.y, checkLoc.x];
                        if (!Terrains.mountainTypes.Contains(checkPt.terrain)) 
                            continue;

                        Vector2Int NLoc = checkLoc + checkDirs[0];
                        Vector2Int ELoc = checkLoc + checkDirs[2];
                        Vector2Int SLoc = checkLoc + checkDirs[4];
                        Vector2Int WLoc = checkLoc + checkDirs[6];

                        //break if we find an edge that (1) isn't the one we've come from and (2) has already been marked as an edge
                        //Note: so far have not seen any arrangements that should break this 
                        //as added insurance, check that the sizes of each point's group are the same in case two mountains somehow have adjacent edges
                        if (checkPt.edge && checkPt.group.size == curPoint.group.size) {
                            complete = true;
                            break;
                        }

                        //check that checkPt is on the edge (cardinally adjacent to grass)
                        if((!isOutOfBounds(NLoc) && pixels[NLoc.y, NLoc.x].terrain == terrainType.grassy) || (!isOutOfBounds(ELoc) && pixels[ELoc.y, ELoc.x].terrain == terrainType.grassy) ||
                          (!isOutOfBounds(SLoc) && pixels[SLoc.y, SLoc.x].terrain == terrainType.grassy) || (!isOutOfBounds(WLoc) && pixels[WLoc.y, WLoc.x].terrain == terrainType.grassy)) {
                            
                            //add curPoint to mountain edge and update dirFrom to avoid early break
                            curPoint = checkPt;
                            curLoc = curPoint.loc;
                            curPoint.edge = true;
                            m.edge.Add(curPoint);
                            dirFrom = (i + 4) % 8;
                            break;
                        }
                    }

                    if (prevLoc == curLoc) break;

                    if (complete) break;

                    ++innerLoops;
                }

                //break if edge and unorderedEdge have same size
                if (m.edge.Count == m.unorderedEdge.Count) break;

                curPoint = null;

                //Search for non-added edge of mountain on map edge
                foreach (pixel p in m.unorderedEdge) {
                    if (!p.edge && isOnEdge(p.loc)) {
                        curPoint = p;
                        curPoint.edge = true;
                        m.edge.Add(curPoint);
                        dirFrom = -1;
                        break;
                    }
                }

                //if no more edge points, find any remaining point on edge
                if (curPoint == null) {
                    foreach (pixel p in m.unorderedEdge) {
                        if (!p.edge) {
                        curPoint = p;
                        curPoint.edge = true;
                        m.edge.Add(curPoint);
                        dirFrom = -1;
                        break;
                    }
                    }
                }

                ++outerLoops;
            }
        }
    }

    private void generateRocky(float scale) {
        foreach (mountain m in mountains) {
            Vector2 offset = new Vector2(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));
            float[] rockyNoise = GetNoise.Generate1D(m.edge.Count(), scale, offset);

            // Using Amanatides-Woo fast voxel traversal algorithm
            for(int i = 0; i < m.edge.Count(); ++i) {
                pixel p = m.edge[i];

                Vector2 cur = p.loc + new Vector2(.5f, .5f);
                Vector2Int v = Vector2Int.RoundToInt((1 + 3 * rockyNoise[i]) * gradMap[p.loc.y, p.loc.x].normalized);
                Vector2 end = cur + v;
                Vector2 step = new Vector2(v.x < 0 ? -1f : 1f, v.y < 0 ? -1f : 1f);

                float tMaxX = step.x/(2*(float)v.x);
                float tMaxY = step.y/(2*(float)v.y);
                float tDeltaX = Mathf.Abs(1f/(float)v.x);
                float tDeltaY = Mathf.Abs(1f/(float)v.y);

                List<pixel> toChange = new List<pixel>();

                while(cur != end && !isOutOfBounds(Vector2Int.FloorToInt(cur))) {
                    toChange.Add(pixels[(int)cur.y, (int)cur.x]);
                    if(tMaxX < tMaxY) {
                        tMaxX += tDeltaX;
                        cur.x += step.x;
                    }
                    else {
                        tMaxY += tDeltaY;
                        cur.y += step.y;
                    }
                }
                
                Vector2Int endInt = Vector2Int.FloorToInt(end);
                if (!isOutOfBounds(endInt)) toChange.Add(pixels[endInt.y, endInt.x]);

                foreach (pixel r in toChange) {
                    if (!Terrains.rockyTypes.Contains(r.terrain)) break;
                    r.terrain = terrainType.rocky;
                }
            }
        }
    }


    // Find each lake, cull lakes under critical mass
    private void findLakes(int minLakeSize) {
        foreach(pixel p in pixels) {
            lake l = new lake();
            Stack<pixel> toCheck = new Stack<pixel>();
            if(!p.grouped && p.terrain == terrainType.water) {
                toCheck.Push(p);
            }
            
            Vector2Int COMLoc = new Vector2Int();

            int iter = 0;

            while(toCheck.Count != 0) {
                pixel curPixel = toCheck.Pop();
                Vector2Int curLoc = curPixel.loc;
                COMLoc += curLoc;
                curPixel.grouped = true;
                curPixel.group = l;
                l.Add(curPixel);

                pixel pixelN = new pixel();
                pixel pixelE = new pixel();
                pixel pixelS = new pixel();
                pixel pixelW = new pixel();

                if(curLoc.y != height-1) pixelN = pixels[curLoc.y+1, curLoc.x]; 
                else pixelN.terrain = terrainType.none;
                
                if(curLoc.x != width-1) pixelE = pixels[curLoc.y, curLoc.x+1]; 
                else pixelE.terrain = terrainType.none;
                
                if(curLoc.y != 0) pixelS = pixels[curLoc.y-1, curLoc.x]; 
                else pixelS.terrain = terrainType.none;
                
                if(curLoc.x != 0) pixelW = pixels[curLoc.y, curLoc.x-1]; 
                else pixelW.terrain = terrainType.none;
                
                
                
                if(pixelN.terrain == terrainType.water && !pixelN.grouped) {
                    toCheck.Push(pixelN);
                    pixelN.grouped = true;
                }
                if(pixelE.terrain == terrainType.water && !pixelE.grouped) {
                    toCheck.Push(pixelE);
                    pixelE.grouped = true;
                }
                if(pixelS.terrain == terrainType.water && !pixelS.grouped) {
                    toCheck.Push(pixelS);
                    pixelS.grouped = true;
                }
                if(pixelW.terrain == terrainType.water && !pixelW.grouped) {
                    toCheck.Push(pixelW);
                    pixelW.grouped = true;
                }

                iter++;
                if (iter > 10000) {
                    return;
                }
            }
            if (l.size != 0)
            {
                COMLoc /= l.size;
                l.COM = pixels[COMLoc.y, COMLoc.x];
                lakes.Add(l);
            }
        }
        Debug.Log("Found " + lakes.Count + " lakes");

        foreach (lake l in lakes) {
            if (l.size < minLakeSize) {
                foreach (pixel p in l.points) {
                    p.terrain = terrainType.grassy;
                }
                l.Clear();
            }
        }

        for(int i = 0; i < lakes.Count; ++i) {
            if(lakes[i].size == 0) {
                lakes.Remove(lakes[i]);
                --i;
            }
        }

        
        Debug.Log(lakes.Count + " lakes remain after culling");

    }



    private int mostIndex(int[] arr) {
        int idx = 0;
        int most = 0;
        for(int i = arr.Length-1; i >= 0 ; --i) {
            if (arr[i] > most) {
                idx = i;
                most = arr[i];
            }
        }
        return idx;
    }
}
