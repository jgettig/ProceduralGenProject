using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class NoiseGen : MonoBehaviour
{
    public RawImage debugImage;
    public Canvas parent;
    public TextMeshProUGUI hoverText;

    //get cutoff points from Unity UI
    [Header("Generation Attributes")]
    [Range(0,1)]
    public float deepMountain = 0.365f;
    private float thinOffset = 0.025f;

    [Range(0,1)]
    public float grassy = 0.7f;

    [Range(0, 10000)]
    public int minLakeSize = 300;

    [Range(0, 10000)]
    public int minMtnSize = 300;

    public bool doSearch;
    [Range(1,10)]
    public float searchDist = 1;

    [Range(0.005f,.5f)]
    public float rockyScale = 0.01f; 

    private int terrainTypes = 5;
    private cutoffs co;

    //Store dimensions of map
    private int width = 100;
    private int height = 100;
    private float scale = 0.03f;

    //will regenerate on each reset
    private Vector2 offset;

    //previous instances of each attribute. Used to minimize update calls.
    private float prevDeepMountain;
    private float prevThinMountain;
    private float prevGrassy;
    private int prevMinLakeSize;
    private int prevMinMtnSize;
    private float prevSearchDist;
    private bool prevDoSearch;
    private float prevRockyScale;

    private FlatMap map;

    // On start (or manual update) randomize map and save parameters
    void Start() {
        debugImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, debugImage.rectTransform.rect.height);

        map = new FlatMap(width, height, terrainTypes);

        reGenerate();
    }

    void saveParameters() {
        prevDeepMountain = deepMountain;
        prevGrassy = grassy;
        prevMinLakeSize = minLakeSize;
        prevMinMtnSize = minMtnSize;
        prevSearchDist = searchDist;
        prevDoSearch = doSearch;
        prevRockyScale = rockyScale;
    }

    void updateParameters() {
        co.deepMountain = deepMountain;
        co.thinMountain = deepMountain + thinOffset;
        co.grassy = grassy;
        co.lakeSize = minLakeSize;
        co.mtnSize = minMtnSize;
        co.searchDist = searchDist;
        co.doSearch = doSearch;
        co.rockyScale = rockyScale;
    }

    bool paramsChanged() {
        return prevGrassy != grassy || prevDeepMountain != deepMountain ||  
                prevMinLakeSize != minLakeSize || prevMinMtnSize != minMtnSize || prevSearchDist != searchDist ||
                prevDoSearch != doSearch || prevRockyScale != rockyScale;
    }

    // Every tick check for parameter changes and update map as necessary without regenerating.
    // On map update save all parameters so as to prevent full updates every tick
    void Update() {
        Vector2 mousePos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        parent.transform as RectTransform,
        Input.mousePosition, parent.worldCamera,
        out mousePos);

        Vector2 trueMousePos = mousePos;

        Vector2 imgDims = new Vector2(debugImage.rectTransform.rect.width, debugImage.rectTransform.rect.height);
        Vector2 scale = new Vector2(width/imgDims.x, height/imgDims.y);
        mousePos += imgDims/2;
        mousePos = Vector2.Scale(mousePos, scale);

        bool overBody = false;
        if(mousePos.x >=0 && mousePos.y >=0 && mousePos.x < width && mousePos.y < height) {
            pixel curPxl = map.GetPixelAt((int)MathF.Floor(mousePos.x), (int)MathF.Floor(mousePos.y));

            /*if(curPxl.grouped) {
                hoverText.rectTransform.position = (trueMousePos + (Vector2)parent.transform.position + new Vector2(0, -50));
                hoverText.text = curPxl.terrain + " size " + curPxl.group.size;
                if(curPxl.group is mountain) {
                    mountain g = (mountain)curPxl.group;
                    hoverText.text += "\ncircularity " + g.circularity;
                }
                overBody = true;
            } */

            //hoverText.rectTransform.position = (trueMousePos + (Vector2)parent.transform.position + new Vector2(100, 0));
            hoverText.text = curPxl.terrain + "\nlocation: " + curPxl.loc.x + ", " + curPxl.loc.y;
            if(curPxl.edge) {
                mountain m = (mountain)curPxl.group;
                hoverText.text += "\nPart of mountain edge\nedge index: " + m.edge.IndexOf(curPxl);
            }
            overBody = true;
        }

        if (!overBody) {
            hoverText.text = "";
        }

        if (grassy < deepMountain + thinOffset) grassy = deepMountain;

        if (paramsChanged()) {
            updateParameters();
            saveParameters();
            GenerateMap();
        }
    }

    // Regenerate map completely
    public void reGenerate() {
        offset.x = UnityEngine.Random.Range(-1000f, 1000f);
        offset.y = UnityEngine.Random.Range(-1000f, 1000f);

        updateParameters();
        saveParameters();
        GenerateMap();
    }

    void GenerateMap() {
        map.reset();

        map.Generate(offset);
        Color[] colorMap = map.updateColors();

        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(colorMap);
        tex.filterMode = FilterMode.Point;
        tex.Apply();

        debugImage.texture = tex;
    }
}