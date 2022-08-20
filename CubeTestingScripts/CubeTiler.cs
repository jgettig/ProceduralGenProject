using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTiler : MonoBehaviour
{
    public GameObject cubePrefab;
    public Material mountainMat;
    public Material rockyMat;
    public Material grassyMat;
    public Material waterMat;
    public GameObject[,,] cubeObjects;
    public Cube[,,] cubeScripts;

    public FlatMap map;

    private Vector2 offset;

    // Start is called before the first frame update
    void Start()
    {
        int length = 100;
        int width = 100;
        int height = 4;

        map = new FlatMap(width, length, 5);
        offset.x = UnityEngine.Random.Range(-1000f, 1000f);
        offset.y = UnityEngine.Random.Range(-1000f, 1000f);
        map.Generate(offset);

        cubeObjects = new GameObject[height, length, width];
        cubeScripts = new Cube[height, length, width];

        for (int y = 0; y <= height; y += height-1) {
            for (int z = 0; z < length; ++z) {
                for (int x = 0; x < length; ++x) {
                    cubeObjects[y,z,x] = Instantiate(cubePrefab, new Vector3(x, y, z), Quaternion.identity);
                    cubeObjects[y,z,x].transform.parent = transform;
                    cubeScripts[y,z,x] = cubeObjects[y,z,x].GetComponent<Cube>();
                    cubeScripts[y,z,x].Init(null, true, false);
                }
            }
        }

        for (int z = 0; z < length; ++z) {
            for (int x = 0; x < width; ++x) {
                cubeObjects[1,z,x] = Instantiate(cubePrefab, new Vector3(x, 1, z), Quaternion.identity);
                cubeObjects[1,z,x].transform.parent = transform;
                cubeScripts[1,z,x] = cubeObjects[1,z,x].GetComponent<Cube>();
                cubeObjects[2,z,x] = Instantiate(cubePrefab, new Vector3(x, 2, z), Quaternion.identity);
                cubeObjects[2,z,x].transform.parent = transform;
                cubeScripts[2,z,x] = cubeObjects[2,z,x].GetComponent<Cube>();
                switch (map.GetPixelAt(x, z).terrain) {
                    case terrainType.deepMountain:
                        cubeScripts[1,z,x].Init(mountainMat, false, false);
                        cubeScripts[2,z,x].Init(mountainMat, false, false);
                        break;
                    case terrainType.rocky:
                        cubeScripts[1,z,x].Init(rockyMat, false, false);
                        cubeScripts[2,z,x].Init(null, true, false);
                        break;
                    case terrainType.grassy:
                        cubeScripts[1,z,x].Init(grassyMat, false, false);
                        cubeScripts[2,z,x].Init(null, true, false);
                        break;
                    case terrainType.water:
                        cubeScripts[1,z,x].Init(waterMat, false, true);
                        cubeScripts[2,z,x].Init(null, true, false);
                        break;
                    default:
                        cubeScripts[1,z,x].Init(null, true, false);
                        cubeScripts[2,z,x].Init(null, true, false);
                        break;
                }
            }
        }

        for(int y = 0; y < height; ++y) {
            for(int z = 0; z < length; ++z) {
                for(int x = 0; x < width; ++x) {
                    //Order: 0-top, 1-front, 2-left, 3-back, 4-right, 5-bottom
                    Cube c = cubeScripts[y,z,x];
                    Cube[] adjacents = new Cube[6];

                    if (y < height-1) adjacents[0] = cubeScripts[y+1, z, x];
                    if (z > 0) adjacents[1] = cubeScripts[y, z-1, x];
                    if (x > 0) adjacents[2] = cubeScripts[y, z, x-1];
                    if (z < length-1) adjacents[3] = cubeScripts[y, z+1, x];
                    if (x < width-1) adjacents[4] = cubeScripts[y, z, x+1];
                    if (y > 0) adjacents[5] = cubeScripts[y-1, z, x];

                    c.Draw(adjacents);
                }
            }
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
