using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowRenderer : MonoBehaviour
{
    public GameObject testObject;
    public Material material;
    public GameObject lightObject;
    // A reference to the compute shader.
    public ComputeShader shadowComputeShader;
    public ComputeShader triangleToVertexCountComputeShader;

    private struct ReadVertex
    {
        public Vector3 position;
    };

    // A state variable to keep track of whether the compute buffers have been set up.
    private bool initialized;
    // Compute buffers to read and write data to the compute shader.
    private ComputeBuffer readVertexBuffer;
    private ComputeBuffer readIndexBuffer;
    private ComputeBuffer writeTriangleBuffer;
    // A compute buffer to hold indirect draw arguments.
    private ComputeBuffer argsBuffer;
    // The id of the kernel in the shadow compute shader.
    private int idShadowKernel;
    // The id of the kernel in the triangle to vertex count shader.
    private int idTriangleToVertexCountKernel;
    // The id of the kernel in the index to vertex count compute shader.
    private int dispatchSize;
    // Bounds of the generated mesh.
    private Bounds bounds;
    // Chunk meshes.
    private Mesh mesh;
    // Light.
    private Vector3 lightPosition;

    // The stride of one entry in each compute buffer.
    private const int READ_VERTEX_STRIDE = sizeof(float) * 3;
    private const int READ_INDEX_STRIDE = sizeof(int);
    private const int WRITE_TRIANGLE_STRIDE = ((sizeof(float) * 3) * 3) + (sizeof(float) *  3); // sizeof(triangle vertices) + sizeof(normal) = (sizeof(WRITE_VERTEX_STRIDE) * 3) + (sizeof(float) * 3)
    private const int ARGS_STRIDE = sizeof(int) * 4;

    private void OnEnable()
    {
        // If initialized, call disable to clean things up.
        if(initialized)
        {
            OnDisable();
        }
        initialized = true;

        // Get mesh from ChunkMeshGenerator.
        mesh = testObject.GetComponent<MeshFilter>().mesh;
        // Retrieve and store vertices.
        Vector3[] positions = mesh.vertices;
        ReadVertex[] vertices = new ReadVertex[positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new ReadVertex()
            {
                position = positions[i]
            };
        }
        // Retrieve, calculate, and store indices.
        List<int> indexList = new List<int>();
        indexList.AddRange(mesh.GetTriangles(0));
        List<int> bufferIndexList = new List<int>();
        for (int i = 0; i < indexList.Count; i += 3)
        {
            // Line segment 0
            bufferIndexList.Add(indexList[i] + 0);
            bufferIndexList.Add(indexList[i] + 1);
            // Line segment 1
            bufferIndexList.Add(indexList[i] + 1);
            bufferIndexList.Add(indexList[i] + 2);
            // Line segment 2
            bufferIndexList.Add(indexList[i] + 2);
            bufferIndexList.Add(indexList[i] + 0);
        }
        int[] indices = bufferIndexList.ToArray();
        int numberOfEdges = indices.Length;

        // Create the compute buffers.
        readVertexBuffer = new ComputeBuffer(vertices.Length, READ_VERTEX_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable); // Initialize the buffer.
        readVertexBuffer.SetData(vertices); // Upload data to the GPU.
        readIndexBuffer = new ComputeBuffer(indices.Length, READ_INDEX_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        readIndexBuffer.SetData(indices);
        writeTriangleBuffer = new ComputeBuffer(numberOfEdges * 2, WRITE_TRIANGLE_STRIDE, ComputeBufferType.Append); // Each edge will generate 2 triangles.
        writeTriangleBuffer.SetCounterValue(0); // Set counter to 0 to be safe and make sure the buffer is cleared.
        argsBuffer = new ComputeBuffer(1, ARGS_STRIDE, ComputeBufferType.IndirectArguments);
        /* The data in arguments buffer corresponds to:
         * 0: The vertex count per draw instance.
         * 1: Instance count.
         * 2: Start vertex location if using a graphics buffer.
         * 3: Start instance location if using a graphics buffer.
         */ 
        argsBuffer.SetData(new int[] { 0, 1, 0, 0 });

        // Set variables in the computer shader.
        // Find the kernel of the compute shader.
        idShadowKernel = shadowComputeShader.FindKernel("Main");
        idTriangleToVertexCountKernel = triangleToVertexCountComputeShader.FindKernel("Main");
        // Set compute buffers with the kernelID, the name of the compute shader variable, and the c# script variable.
        shadowComputeShader.SetBuffer(idShadowKernel, "_ReadVertices", readVertexBuffer);
        shadowComputeShader.SetBuffer(idShadowKernel, "_ReadIndices", readIndexBuffer);
        shadowComputeShader.SetBuffer(idShadowKernel, "_WriteTriangles", writeTriangleBuffer);
        shadowComputeShader.SetInt("_NumberOfReadEdges", numberOfEdges);
        lightPosition = lightObject.transform.position;
        shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
        shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", testObject.transform.localToWorldMatrix);

        // Pass arguments buffer to the compute shader.
        triangleToVertexCountComputeShader.SetBuffer(idTriangleToVertexCountKernel, "_IndirectArgumentsBuffer", argsBuffer);

        // Set the write data to the material.
        material.SetBuffer("_WriteTriangles", writeTriangleBuffer);

        /* Determine the number of dispatches necessary to process the edges, by retrieving the kernel group size,
         * and dividing the number of edges by the kernel group size. Round up to the nearest integer number of dispatches
         * so no edges at the end of the array are missed.
         */
        shadowComputeShader.GetKernelThreadGroupSizes(idShadowKernel, out uint threadGroupSize, out _, out _);
        dispatchSize = Mathf.CeilToInt((float)numberOfEdges / threadGroupSize);

        // Arbitrarily large bounds to indicate to unity to not cull the written mesh under any circumstances.
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000000.0f);
    }

    private void OnDisable()
    {
        // Release the buffers, freeing up memory.
        if (initialized)
        {
            readVertexBuffer.Release();
            readIndexBuffer.Release();
            writeTriangleBuffer.Release();
            argsBuffer.Release();
        }
        initialized = false;
    }

    // LateUpdate() is called after update is called.
    private void LateUpdate()
    {
        // Clear the compute buffer of the last frame's data.
        writeTriangleBuffer.SetCounterValue(0);

        // Update the compute shader with the frame specific data.
        lightPosition = lightObject.transform.position;
        shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
        shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", testObject.transform.localToWorldMatrix);
        // Dispatch the compute shader to run on the GPU.
        shadowComputeShader.Dispatch(idShadowKernel, dispatchSize, 1, 1);

        /* Get the count of the draw buffer into the argurment buffer. 
         * This sets the vertex count for the draw call.
         */
        ComputeBuffer.CopyCount(writeTriangleBuffer, argsBuffer, 0);

        /* The shadow compute shader outputs triangles, but the graphics shader needs the number of vertices.
           To fix this, we will multiply the vertex count by 3. To avoid transfering data back to the CPU,
           This will be done on the GPU with a small compute shader.
        */
        triangleToVertexCountComputeShader.Dispatch(idTriangleToVertexCountKernel, 1, 1, 1);

        // Queue a draw call for the generated mesh.
        Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 0, null, null, ShadowCastingMode.On, true, gameObject.layer);
    }
}
