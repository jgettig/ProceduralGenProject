using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetNoise : MonoBehaviour
{
    
    // Generates a 2d array of perlin noise with specified width, height, scale, and offset
    public static float[,] Generate(int width, int height, float scale, Vector2 offset) {

        // create empty map
        float[,] noiseMap = new float[height, width];

        // loop through each elt of noise map
        
        for(int y = 0; y < height; y++) {
            for(int x = 0; x < width; ++x) {

                // calculate sample positions
                float samplePosX = (float)x * scale + offset.x;
                float samplePosY = (float)y * scale + offset.y;

                noiseMap[y,x] = Mathf.PerlinNoise(samplePosX, samplePosY);
            }
        }
        return noiseMap;
    }

    // Generates a 1d array of perlin noise with specified width, scale, and offset
    // Additionally, averages first and second to last value to override last in order to
    // provide an output that (vaguely) loops
    public static float[] Generate1D(int width, float scale, Vector2 offset) {

        // create empty map
        float[] noiseMap = new float[width];

        // loop through each elt of noise map
        for(int i = 0; i < width; ++i) {

            // calculate sample position
            float samplePosX = (float)i * scale + offset.x;
            float samplePosY = (float)offset.y;

            noiseMap[i] = Mathf.PerlinNoise(samplePosX, samplePosY);
        }

        noiseMap[width-1] = (noiseMap[0] + noiseMap[width-2])/2f;

        return noiseMap;
    }
}
