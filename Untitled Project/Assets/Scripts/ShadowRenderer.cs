using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowRenderer : MonoBehaviour
{
    // References to the compute shaders.
    public ComputeShader shadowComputeShader;
    public ComputeShader triangleToVertexCountComputeShader;

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
    // Light.
    private Vector3 lightPosition;
    // Render texture to draw to
    private RenderTexture renderTexture;
    // Local to world space transform matrix.
    private Matrix4x4 localToWorldMatrix;
    //
    private Camera cam;

    private Vector3 prev;

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
        if (transform.GetChild(0).GetComponent<Camera>())
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.ARGB32);
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

    public void GenerateShadows(Vector3[] vertices, int[] indices, int numberOfIndices, Matrix4x4 shadowObjectLocalToWorldMatrix)
    {
        // If initialized, call disable to clean things up.
        if (initialized)
        {
            OnDisable();
        }
        initialized = true;

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
        lightPosition = gameObject.transform.position;
        shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
        localToWorldMatrix = shadowObjectLocalToWorldMatrix;
        shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", localToWorldMatrix);

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

        // Arbitrarily large bounds to indicate to unity to not cull the written mesh under any circumstances.
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000000.0f);
    }

    // LateUpdate() is called after update is called.
    public void DrawShadow()
    {
        if (initialized)
        {
            // Clear the compute buffer of the last frame's data.
            writeTriangleBuffer.SetCounterValue(0);

            // Update the compute shader with the frame specific data.
            lightPosition = gameObject.transform.position;
            shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
            shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", localToWorldMatrix);
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

            Debug.Log("ShadowRenderer: " + Time.time);
            // Queue a draw call for the generated mesh.
            Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 0, cam, null, ShadowCastingMode.Off, false, gameObject.layer);
            Graphics.Blit(cam.targetTexture, GetComponent<Light>().shadowMaskRenderTexture);

            prev = Camera.main.transform.position;
        }
    }
}
