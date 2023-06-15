using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowMeshGenerator : MonoBehaviour
{
    public GameObject shadowObject;

    private struct ReadVertex
    {
        public Vector3 position;
    };

    // References to the compute shaders.
    [SerializeField] ComputeShader shadowComputeShader;
    [SerializeField] ComputeShader triangleToVertexCountComputeShader;

    // Material to run the shader.
    private Material material;
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
    // Camera to render the mesh too.
    private Camera cam;
    // Light.
    private Vector3 lightPosition;
    // Render texture to draw to
    private RenderTexture renderTexture;

    // The stride of one entry in each compute buffer.
    private const int READ_VERTEX_STRIDE = sizeof(float) * 3;
    private const int READ_INDEX_STRIDE = sizeof(int);
    private const int WRITE_TRIANGLE_STRIDE = ((sizeof(float) * 3) * 4); // sizeof(triangle vertices) = (sizeof(WRITE_VERTEX_STRIDE) * 3)
    private const int ARGS_STRIDE = sizeof(int) * 4;

    private void OnEnable()
    {
        initialized = false;
        // Initialize material with proper shader.
        material = new Material(Shader.Find("Custom/Shadows"));
    }

    private void Start()
    {
        // Create an instance of the compute shader for that specific light.
        shadowComputeShader = Instantiate(shadowComputeShader);
        triangleToVertexCountComputeShader = Instantiate(triangleToVertexCountComputeShader);

        foreach (Transform child in transform)
        {
            if (child.gameObject.GetComponent<Camera>())
            {
                cam = child.gameObject.GetComponent<Camera>();

                // Create a render texture for the mesh to output too.
                renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            }
        }
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

    public void GenerateShadows()
    {
        // If initialized, call disable to clean things up.
        if (initialized)
        {
            OnDisable();
        }
        initialized = true;

        Mesh mesh = new Mesh();
        if (shadowObject.GetComponent<ChunkMeshGenerator>())
        {
            mesh = shadowObject.GetComponent<ChunkMeshGenerator>().GetMesh();
        }

        // Get mesh from ChunkMeshGenerator.
        List<Vector3> meshVertices = new List<Vector3>();
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            meshVertices.Add(mesh.vertices[i]);
        }
        List<int> meshIndices = new List<int>();
        for (int i = 0; i < mesh.GetTriangles(0).Length; i++)
        {
            meshIndices.Add(mesh.GetTriangles(0)[i]);
        }
        for (int i = 0; i < mesh.GetTriangles(1).Length; i++)
        {
            meshIndices.Add(mesh.GetTriangles(1)[i]);
        }
        // Retrieve and store vertices.
        ReadVertex[] vertices = new ReadVertex[meshIndices.Count];
        int[] indices = new int[meshIndices.Count];
        // Retrieve, calculate, and store indices.
        for (int i = 0; i < meshIndices.Count; i++)
        {
            vertices[i] = new ReadVertex()
            {
                position = meshVertices[meshIndices[i]]
            };
            indices[i] = i;
        }
        int numberOfIndices = indices.Length;

        // Create the compute buffers.
        readVertexBuffer = new ComputeBuffer(vertices.Length, READ_VERTEX_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable); // Initialize the buffer.
        readVertexBuffer.SetData(vertices); // Upload data to the GPU.
        readIndexBuffer = new ComputeBuffer(indices.Length, READ_INDEX_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        readIndexBuffer.SetData(indices);
        writeTriangleBuffer = new ComputeBuffer(numberOfIndices * 6, WRITE_TRIANGLE_STRIDE, ComputeBufferType.Append); // Each edge will generate 2 triangles.
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
        shadowComputeShader.SetInt("_NumberOfReadIndices", numberOfIndices);
        //lightPosition = gameObject.transform.position;
        shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
        shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", shadowObject.transform.localToWorldMatrix);

        // Pass arguments buffer to the compute shader.
        triangleToVertexCountComputeShader.SetBuffer(idTriangleToVertexCountKernel, "_IndirectArgumentsBuffer", argsBuffer);

        // Set the write data to the material.
        material.SetBuffer("_WriteTriangles", writeTriangleBuffer);

        /* Determine the number of dispatches necessary to process the edges, by retrieving the kernel group size,
         * and dividing the number of edges by the kernel group size. Round up to the nearest integer number of dispatches
         * so no edges at the end of the array are missed.
         */
        shadowComputeShader.GetKernelThreadGroupSizes(idShadowKernel, out uint threadGroupSize, out _, out _);
        dispatchSize = Mathf.CeilToInt((float)numberOfIndices / threadGroupSize);

        // Arbitrarily large bounds to stop Unity from culling the written mesh under any circumstances.
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000000.0f);
    }

    // LateUpdate() is called after update is called.
    private void LateUpdate()
    {
        if (initialized)
        {
            // For each light in the scene, create a shadow mask and save it to a render texture.
            for (int i = 0; i < transform.childCount; i++)

                if (transform.GetChild(i).GetComponent<Light>())
                {
                    // Clear the compute buffer of the last frame's data.
                    writeTriangleBuffer.SetCounterValue(0);

                    // Update the compute shader with the specific data.
                    lightPosition = transform.GetChild(i).transform.position;
                    shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
                    shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", shadowObject.transform.localToWorldMatrix);
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

                    // Write draw to render texture. Pass that render texture to the light game object.
                    RenderTexture rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
                    Graphics.SetRenderTarget(cam.targetTexture);
                    Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 0, cam, null, ShadowCastingMode.Off, true, gameObject.layer);
                    Graphics.Blit(cam.targetTexture, rt);
                    transform.GetChild(i).GetComponent<Light>().shadowMask = rt;
                    RenderTexture.ReleaseTemporary(rt);
                }
        }
    }
}
