using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores information about one pixel in the map
public class pixel {
    public body group;
    public Vector2Int loc;
    public terrainType terrain;
    public bool grouped;
    public bool edge = false; //tracks if pixel is at edge of body, not of map
}

// help readability by assigning each terrain type an enum value
public enum terrainType : int {
    deepMountain = 0,
    thinMountain = 1,
    rocky = 2,
    grassy = 3,
    water = 4,
    none = -1
}

// stores cutoof points to generate terrain types
public struct cutoffs {
    public float deepMountain;
    public float thinMountain;
    public float grassy;
    public int lakeSize;
    public int mtnSize;
    public float searchDist;
    public bool doSearch;
    public float rockyScale;
}



//----------------------ADTS representing masses of pixels----------------------

public abstract class body {
    public List<pixel> points = new List<pixel>();
    public pixel COM;
    public int size = 0;

    public void Add(pixel p) {
        if (points.Contains(p)) return;
        points.Add(p);
        ++size;
    }

    public void Remove(pixel p) {
        points.Remove(p);
        --size;
    }

    //clears all members of body and all pixel memory related to body
    //but does not change any terrain
    public virtual void Clear() {
        foreach (pixel p in points) {
            p.group = null;
            p.grouped = false;
        }
        points.Clear();
        COM = null;
        size = 0;
    }
}

public class mountain : body {
    public List<pixel> edge = new List<pixel>();
    public List<pixel> unorderedEdge = new List<pixel>();
    public double circularity;

    //clears all members of body and all pixel memory related to body
    //but does not change any terrain
    public override void Clear() {
        foreach (pixel p in unorderedEdge) {
            p.edge = false;
        }
        foreach (pixel p in points) {
            p.group = null;
            p.grouped = false;
        }
        points.Clear();
        edge.Clear();
        unorderedEdge.Clear();
        COM = null;
        size = 0;
        circularity = 0;
    }

    public void clearEdges() {
        foreach (pixel p in unorderedEdge) {
            p.edge = false;
        }
        edge.Clear();
    }
}

public class lake : body {
    
}


public static class Global {
    public static double rootPi = Mathf.Sqrt(Mathf.PI);
    
}

public static class Terrains {
    public static terrainType[] mountainTypes = new terrainType[] {
        terrainType.deepMountain,
        terrainType.thinMountain
    };
    public static terrainType[] rockyTypes = new terrainType[] {
        terrainType.deepMountain,
        terrainType.thinMountain,
        terrainType.rocky
    };
}