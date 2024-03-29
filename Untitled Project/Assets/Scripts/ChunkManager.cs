using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    // variables
    public int chunkSize;
    public Dictionary<Vector3, Chunk> chunkDictionary = new Dictionary<Vector3, Chunk>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        chunkSize = 8;


        for (int i = -64; i <= 64; i++)
        {
            for (int j = -64; j <= 64; j++)
            {
                Vector3 chunkCoordinates = new Vector3(i, j, 0);
                Chunk chunk = new Chunk(chunkCoordinates * chunkSize);
                chunkDictionary.Add(chunkCoordinates, chunk);
            }
        }
    }
}
