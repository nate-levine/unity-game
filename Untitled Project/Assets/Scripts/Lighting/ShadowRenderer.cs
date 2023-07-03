using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ShadowRenderer : MonoBehaviour
{
    public bool isStatic;

    // A state variable to keep track of whether the shadow compute buffer is ready to dispatch yet.
    public bool initialized;
    // References to the compute shaders.
    public ComputeShader shadowComputeShader;
    public ComputeShader triangleToVertexCountComputeShader;
    // Material to  hold the shader.
    private Material shadowMaterial;
    // Compute buffers to read and write data to the compute shader.
    private ComputeBuffer readVertexBuffer;
    private ComputeBuffer readIndexBuffer;
    private ComputeBuffer writeTriangleBuffer;
    // A compute buffer to hold indirect draw arguments.
    private ComputeBuffer argsBuffer;
    // The id of the kernel in the shadow compute shader.
    private int idShadowKernel;
    // The id of the kernel in the triangle to vertex count compute shader.
    private int idTriangleToVertexCountKernel;
    // Size of the shadow compute shader dispatch.
    private int dispatchSize;
    // Bounds of the generated shadow mesh.
    private Bounds bounds;
    // Local to world space transform matrix of the mesh casting shadows.
    private Matrix4x4 localToWorldMatrix;
    // Position of the light
    private Vector3 lightPosition;
    // Camera to draw the shadow mesh too.
    private Camera cam;

    // The stride of one entry in each compute buffer.
    private const int READ_VERTEX_STRIDE = sizeof(float) * 3; // float3
    private const int READ_INDEX_STRIDE = sizeof(int); // int
    private const int WRITE_TRIANGLE_STRIDE = ((sizeof(float) * 3) * 4); // Vector3 + float3
    private const int ARGS_STRIDE = sizeof(int) * 4; // int[4]

    private void OnEnable()
    {
        initialized = false;
        // Initialize material with proper shader.
        shadowMaterial = new Material(Shader.Find("Custom/Shadows"));
    }

    private void Start()
    {
        /* Create an instance of both compute shaders for that specific light.
         * Unfortunately this is necessary, as using one compute shader for multiple draw calls overrides
         * each previous one.
         */
        shadowComputeShader = Instantiate(shadowComputeShader);
        triangleToVertexCountComputeShader = Instantiate(triangleToVertexCountComputeShader);
        // Set the camera and initialize its render texture.
        if (transform.GetChild(0).GetComponent<Camera>())
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            cam.targetTexture = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.ARGB32);
        }
    }

    private void OnDisable()
    {
        // Release the buffers when done, freeing up memory.
        if (initialized)
        {
            readVertexBuffer.Release();
            readIndexBuffer.Release();
            writeTriangleBuffer.Release();
            argsBuffer.Release();
        }
        initialized = false;
    }

    // Generate the shadow mesh.
    public void GenerateShadowMask(Vector3[] vertices, int[] indices, int numberOfIndices, Matrix4x4 shadowObjectLocalToWorldMatrix)
    {
        // If initialized, call disable to clear memory space.
        if (initialized)
        {
            OnDisable();
        }

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
        // Set compute buffers with the kernelID, the name of the compute shader variable, and the c# script variables.
        shadowComputeShader.SetBuffer(idShadowKernel, "_ReadVertices", readVertexBuffer);
        shadowComputeShader.SetBuffer(idShadowKernel, "_ReadIndices", readIndexBuffer);
        shadowComputeShader.SetBuffer(idShadowKernel, "_WriteTriangles", writeTriangleBuffer);
        shadowComputeShader.SetInt("_NumberOfReadIndices", numberOfIndices);
        // Current light position.
        lightPosition = gameObject.transform.position;
        shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
        // Local space to world space transform matrix of the shadow caster game object.
        localToWorldMatrix = shadowObjectLocalToWorldMatrix;
        shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", localToWorldMatrix);
        // Pass indirect arguments buffer to the triangle to vertex count compute shader.
        triangleToVertexCountComputeShader.SetBuffer(idTriangleToVertexCountKernel, "_IndirectArgumentsBuffer", argsBuffer);
        // Set the write data in the shader.
        shadowMaterial.SetBuffer("_WriteTriangles", writeTriangleBuffer);
        // Arbitrarily large bounds to indicate to unity to not cull the shadow mesh under any circumstances.
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000000.0f);

        /* Determine the number of dispatches necessary to process the edges, by retrieving the kernel group size,
         * and dividing the number of edges by the kernel group size. Round up to the nearest integer number of dispatches
         * so no triangles at the end of the mesh triangle array are missed.
         */
        // Get kernel thread group size.
        shadowComputeShader.GetKernelThreadGroupSizes(idShadowKernel, out uint threadGroupSize, out _, out _);
        // Caluclate dispatch size based on kernel thread group size.
        dispatchSize = Mathf.CeilToInt((float)numberOfIndices / threadGroupSize);
        // The compute shaders are now ready to dispatch.
        initialized = true;

        // Once ready, dispatch the compute shader.
        if (initialized)
        {
            Dispatch();
        }

        /* On the frame where the shadow mesh is generated, the DrawShadow function cannot be called. To compensate for this, call DrawShadow() at the end of GenerateShadows() to not skip a frame,
         * which would cause a temporary flicker out when the shadow mesh in generated.
         */
        DrawShadow();
    }

    // Draw the shadow mesh to the light's camera.
    public void DrawShadow()
    {
        // If the light is not static, the shadow mesh must be updated dynamically each frame.
        if (initialized && !isStatic)
        {
            Dispatch();
        }
        if (initialized)
        {
            // Queue a draw call to the light's camera for the generated mesh.
            Graphics.DrawProceduralIndirect(shadowMaterial, bounds, MeshTopology.Triangles, argsBuffer, 0, cam, null, ShadowCastingMode.Off, false, gameObject.layer);
            // Blit the camera view to the shadow mask render texture.
            Graphics.Blit(cam.targetTexture, GetComponent<CustomLight>().GetShadowMaskRenderTexture());
        }
    }

    // Dispatch, or run, the compute shaders.
    private void Dispatch()
    {
        // Clear the compute buffer of the last frame's data.
        writeTriangleBuffer.SetCounterValue(0);

        // Update the compute shader with the frame specific data.
        // Curent light position.
        lightPosition = gameObject.transform.position;
        shadowComputeShader.SetFloats("_LightPosition", new float[] { lightPosition.x, lightPosition.y, lightPosition.z });
        // Local space to world space transform matrix of the shadow caster game object.
        shadowComputeShader.SetMatrix("_LocalToWorldTransformMatrix", localToWorldMatrix);
        
        // Dispatch the shadow compute shader to run on the GPU.
        shadowComputeShader.Dispatch(idShadowKernel, dispatchSize, 1, 1);
        /* Get the count of the draw buffer into the argurment buffer. 
         * This sets the vertex count for the draw call in the indirect arguments buffer.
         */
        ComputeBuffer.CopyCount(writeTriangleBuffer, argsBuffer, 0);
        /* The shadow compute shader outputs triangles, but the graphics shader needs the number of vertices.
           To fix this, we will multiply the vertex count by 3. To avoid transfering data back to the CPU,
           This will be done on the GPU with a small compute shader called the indirect arguments buffer.
        */
        triangleToVertexCountComputeShader.Dispatch(idTriangleToVertexCountKernel, 1, 1, 1);
    }
}
