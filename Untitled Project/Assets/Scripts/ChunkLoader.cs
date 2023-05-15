using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public static ChunkLoader Instance { get; private set; }

    public List<Chunk> loadedChunks = new List<Chunk>();

    public List<Vector3> currentChunkKeys = new List<Vector3>();
    public List<Vector3> previousChunkKeys = new List<Vector3>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Update()
    {
        // Load chunks in render view
        currentChunkKeys.Clear();
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector3 chunkKey = new Vector3((int)(Camera.main.transform.position.x / ChunkManager.Instance.chunkSize) + i, (int)(Camera.main.transform.position.y / ChunkManager.Instance.chunkSize) + j, 0);
                if (ChunkManager.Instance.chunkDictionary.ContainsKey(chunkKey))
                    currentChunkKeys.Add(chunkKey);
            }
        }

        if (currentChunkKeys.Count != previousChunkKeys.Count)
        {
            LoadChunks();
        }
        else if (currentChunkKeys.Count == previousChunkKeys.Count)
        {
            for (int i = 0; i < currentChunkKeys.Count; i++)
            {
                if (currentChunkKeys[i] != previousChunkKeys[i])
                    LoadChunks();
            }
        }
    }

    public void LoadChunks()
    {
        loadedChunks.Clear();
        foreach (Vector3 chunkKey in currentChunkKeys)
        {
            if (ChunkManager.Instance.chunkDictionary.ContainsKey(chunkKey))
            {
                loadedChunks.Add(ChunkManager.Instance.chunkDictionary[chunkKey]);
            }
        }

        ChunkMeshGenerator.Instance.ClearMeshData();
        ChunkMeshGenerator.Instance.GenerateMesh();

        previousChunkKeys.Clear();
        foreach (Vector3 chunkKey in currentChunkKeys)
        {
            previousChunkKeys.Add(chunkKey);
        }
    }
}
