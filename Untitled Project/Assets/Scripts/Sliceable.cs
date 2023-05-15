using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    List<Vector3> newVertices = new List<Vector3>();
    List<int>[] newTriangles = new List<int>[2];
    List<Vector2> newUVs = new List<Vector2>();
    List<Vector3> newPositiveVertices = new List<Vector3>();
    List<int>[] newPositiveTriangles = new List<int>[2];
    List<Vector2> newPositiveUVs = new List<Vector2>();
    List<Vector3> newNegativeVertices = new List<Vector3>();
    List<int>[] newNegativeTriangles = new List<int>[2];
    List<Vector2> newNegativeUVs = new List<Vector2>();
    int newPositiveVerticesCount;
    int newNegativeVerticesCount;

    void Start()
    {
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex] = new List<int>();
            newPositiveTriangles[subMeshIndex] = new List<int>();
            newNegativeTriangles[subMeshIndex] = new List<int>();
        }
    }
    public (List<Vector3>, List<int>[], List<Vector2>, List<Vector3>, List<int>[], List<Vector2>) SliceMesh(List<Vector3> oldVertices, List<int>[] oldTriangles, List<Vector2> oldUVs, Vector3 planeNormal, Vector3 planePosition, float planeBoundsMinimum, float planeBoundsMaximum)
    {
        // slice plane is the plane that the mesh will be sliced by.
        Plane slicePlane = new Plane(planeNormal, planePosition);
        Vector3 planeDir = Vector3.Normalize(Vector3.Cross(planeNormal, new Vector3(0.0f, 0.0f, 1.0f)));

        newVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex].Clear();
        }
        newUVs.Clear();
        newPositiveVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newPositiveTriangles[subMeshIndex].Clear();
        }
        newPositiveUVs.Clear();
        newNegativeVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newNegativeTriangles[subMeshIndex].Clear();
        }
        newNegativeUVs.Clear();

        newPositiveVerticesCount = 0;
        newNegativeVerticesCount = 0;

        List<Vector3> vertices = oldVertices;
        List<int>[] triangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            triangles[subMeshIndex] = oldTriangles[subMeshIndex];
        }
        List<Vector2> UVs = oldUVs;

        // run algorithm for every triangle in the mesh
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = 0; i < triangles[subMeshIndex].Count; i += 3)
            {
                int tri0 = triangles[subMeshIndex][i + 0];
                int tri1 = triangles[subMeshIndex][i + 1];
                int tri2 = triangles[subMeshIndex][i + 2];
                Vector3 vert0 = vertices[tri0];
                Vector3 vert1 = vertices[tri1];
                Vector3 vert2 = vertices[tri2];

                bool Vert0Sign = slicePlane.GetSide(vert0);
                bool Vert1Sign = slicePlane.GetSide(vert1);
                bool Vert2Sign = slicePlane.GetSide(vert2);

                Vector3 pointA = vert0;
                Vector3 sideAB = vert1 - vert0;
                Vector3 pointB = vert1;
                Vector3 sideBC = vert2 - vert1;
                Vector3 pointC = vert2;
                Vector3 sideCA = vert0 - vert2;

                // t is the parameter for the plane
                // u is the parameter for the line segment
                float uAB = (((planeDir.x) * (pointA.y - planePosition.y)) + ((planeDir.y) * (planePosition.x - pointA.x))) / ((sideAB.x * planeDir.y) - (sideAB.y * planeDir.x));
                float tAB = (((sideAB.x) * (planePosition.y - pointA.y)) + ((sideAB.y) * (pointA.x - planePosition.x))) / ((planeDir.x * sideAB.y) - (planeDir.y * sideAB.x));
                float uBC = (((planeDir.x) * (pointB.y - planePosition.y)) + ((planeDir.y) * (planePosition.x - pointB.x))) / ((sideBC.x * planeDir.y) - (sideBC.y * planeDir.x));
                float tBC = (((sideBC.x) * (planePosition.y - pointB.y)) + ((sideBC.y) * (pointB.x - planePosition.x))) / ((planeDir.x * sideBC.y) - (planeDir.y * sideBC.x));
                float uCA = (((planeDir.x) * (pointC.y - planePosition.y)) + ((planeDir.y) * (planePosition.x - pointC.x))) / ((sideCA.x * planeDir.y) - (sideCA.y * planeDir.x));
                float tCA = (((sideCA.x) * (planePosition.y - pointC.y)) + ((sideCA.y) * (pointC.x - planePosition.x))) / ((planeDir.x * sideCA.y) - (planeDir.y * sideCA.x));

                // edge case: variable to keep track off if the plane sits entirely inside the triangle.
                bool sliceInside = false;
                // check if plane position is between 2 intersection points. If so, the plane is within the triangle.
                if ((uAB >= 0.0f && uAB <= 1.0f) && (uBC >= 0.0f && uBC <= 1.0f))
                {
                    Vector3 intersectionAB = planePosition + (tAB * planeDir);
                    Vector3 intersectionBC = planePosition + (tBC * planeDir);

                    float lerpX = (planePosition.x - intersectionAB.x) / (intersectionBC.x - intersectionAB.x);
                    float lerpY = (planePosition.y - intersectionAB.y) / (intersectionBC.y - intersectionAB.y);
                    if (lerpX >= 0.0f && lerpX <= 1.0f)
                        sliceInside = true;
                    if (lerpY >= 0.0f && lerpY <= 1.0f)
                        sliceInside = true;
                }
                if ((uBC >= 0.0f && uBC <= 1.0f) && (uCA >= 0.0f && uCA <= 1.0f))
                {
                    Vector3 intersectionBC = planePosition + (tBC * planeDir);
                    Vector3 intersectionCA = planePosition + (tCA * planeDir);

                    float lerpX = (planePosition.x - intersectionBC.x) / (intersectionCA.x - intersectionBC.x);
                    float lerpY = (planePosition.y - intersectionBC.y) / (intersectionCA.y - intersectionBC.y);
                    if (lerpX >= 0.0f && lerpX <= 1.0f)
                        sliceInside = true;
                    if (lerpY >= 0.0f && lerpY <= 1.0f)
                        sliceInside = true;
                }
                if ((uCA >= 0.0f && uCA <= 1.0f) && (uAB >= 0.0f && uAB <= 1.0f))
                {
                    Vector3 intersectionCA = planePosition + (tCA * planeDir);
                    Vector3 intersectionAB = planePosition + (tAB * planeDir);

                    float lerpX = (planePosition.x - intersectionCA.x) / (intersectionAB.x - intersectionCA.x);
                    float lerpY = (planePosition.y - intersectionCA.y) / (intersectionAB.y - intersectionCA.y);
                    if (lerpX >= 0.0f && lerpX <= 1.0f)
                        sliceInside = true;
                    if (lerpY >= 0.0f && lerpY <= 1.0f)
                        sliceInside = true;
                }

                // if there is an intersection between the plane (-0.5 to 0.5) and the line segment (0.0 to 1.0), then slice along the plane by seperating the triangle into 3 new triangles.
                // also find the intersection if the entire plane sits inside the triangle, and therefore does not intersect it.
                if ((uAB >= 0.0f && uAB <= 1.0f && tAB >= planeBoundsMinimum && tAB <= planeBoundsMaximum) ||
                    (uBC >= 0.0f && uBC <= 1.0f && tBC >= planeBoundsMinimum && tBC <= planeBoundsMaximum) ||
                    (uCA >= 0.0f && uCA <= 1.0f && tCA >= planeBoundsMinimum && tCA <= planeBoundsMaximum) ||
                    sliceInside)
                    {
                    Vector3 point0 = new Vector3();
                    Vector3 point1 = new Vector3();
                    Vector3 point2 = new Vector3();

                    int pointIndex0 = tri0;
                    int pointIndex1 = tri1;
                    int pointIndex2 = tri2;

                    if (Vert0Sign == Vert1Sign)
                    {
                        point0 = vert0;
                        point1 = vert1;
                        point2 = vert2;

                        // keep track of what triangles correspond to what vertices.
                        pointIndex0 = tri0;
                        pointIndex1 = tri1;
                        pointIndex2 = tri2;
                    }
                    else if (Vert1Sign == Vert2Sign)
                    {
                        point0 = vert1;
                        point1 = vert2;
                        point2 = vert0;

                        // keep track of what triangles correspond to what vertices.
                        pointIndex0 = tri1;
                        pointIndex1 = tri2;
                        pointIndex2 = tri0;
                    }
                    else if (Vert2Sign == Vert0Sign)
                    {
                        point0 = vert2;
                        point1 = vert0;
                        point2 = vert1;

                        // keep track of what triangles correspond to what vertices.
                        pointIndex0 = tri2;
                        pointIndex1 = tri0;
                        pointIndex2 = tri1;
                    }

                    List<Vector3> slicedTriangleIntersections = FindIntersections(planeNormal, planePosition, point0, point1, point2);

                    bool Vert0SignSliced = slicePlane.GetSide(point0);
                    bool Vert1SignSliced = slicePlane.GetSide(point1);
                    bool Vert2SignSliced = slicePlane.GetSide(point2);

                    // linearly interpolate to find UVs
                    List<Vector2> slicedUVIntersections = new List<Vector2>();

                    float UV0_LerpX = Mathf.InverseLerp(point0.x, point2.x, slicedTriangleIntersections[0].x);
                    float UV0_X = Mathf.Lerp(UVs[pointIndex0].x, UVs[pointIndex2].x, UV0_LerpX);
                    float UV0_LerpY = Mathf.InverseLerp(point0.y, point2.y, slicedTriangleIntersections[0].y);
                    float UV0_Y = Mathf.Lerp(UVs[pointIndex0].y, UVs[pointIndex2].y, UV0_LerpY);
                    slicedUVIntersections.Add(new Vector2(UV0_X, UV0_Y));

                    float UV1_LerpX = Mathf.InverseLerp(point1.x, point2.x, slicedTriangleIntersections[1].x);
                    float UV1_X = Mathf.Lerp(UVs[pointIndex1].x, UVs[pointIndex2].x, UV1_LerpX);
                    float UV1_LerpY = Mathf.InverseLerp(point1.y, point2.y, slicedTriangleIntersections[1].y);
                    float UV1_Y = Mathf.Lerp(UVs[pointIndex1].y, UVs[pointIndex2].y, UV1_LerpY);
                    slicedUVIntersections.Add(new Vector2(UV1_X, UV1_Y));


                    if (Vert1SignSliced)
                    {
                        AddTriangleMeshData(slicedTriangleIntersections[0], point1, slicedTriangleIntersections[1], slicedUVIntersections[0], UVs[pointIndex1], slicedUVIntersections[1], subMeshIndex, true);
                    }
                    else
                    {
                        AddTriangleMeshData(slicedTriangleIntersections[0], point1, slicedTriangleIntersections[1], slicedUVIntersections[0], UVs[pointIndex1], slicedUVIntersections[1], subMeshIndex, false);
                    }
                    if (Vert2SignSliced)
                    {
                        AddTriangleMeshData(point2, slicedTriangleIntersections[0], slicedTriangleIntersections[1], UVs[pointIndex2], slicedUVIntersections[0], slicedUVIntersections[1], subMeshIndex, true);
                    }
                    else
                    {
                        AddTriangleMeshData(point2, slicedTriangleIntersections[0], slicedTriangleIntersections[1], UVs[pointIndex2], slicedUVIntersections[0], slicedUVIntersections[1], subMeshIndex, false);
                    }
                    if (Vert0SignSliced)
                    {
                        AddTriangleMeshData(point0, point1, slicedTriangleIntersections[0], UVs[pointIndex0], UVs[pointIndex1], slicedUVIntersections[0], subMeshIndex, true);
                    }
                    else
                    {
                        AddTriangleMeshData(point0, point1, slicedTriangleIntersections[0], UVs[pointIndex0], UVs[pointIndex1], slicedUVIntersections[0], subMeshIndex, false);
                    }
                }
                // if there is no intersection, keep the original triangle.
                else
                {
                    if (!Vert0Sign || !Vert1Sign || !Vert2Sign)
                    {
                        AddTriangleMeshData(vert0, vert1, vert2, UVs[tri0], UVs[tri1], UVs[tri2], subMeshIndex, false);
                    }
                    else
                    {
                        AddTriangleMeshData(vert0, vert1, vert2, UVs[tri0], UVs[tri1], UVs[tri2], subMeshIndex, true);
                    }
                }
            }
        }
        return (newPositiveVertices, newPositiveTriangles, newPositiveUVs, newNegativeVertices, newNegativeTriangles, newNegativeUVs);
    }

    public List<Vector3> FindIntersections(Vector3 planeNormal, Vector3 planePosition, Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        // find 2 vectors to define plane orientation.
        // note that U & V are not related to the mesh UVs.
        Vector3 planeV = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 planeU = Vector3.Cross(planeV, planeNormal);

        // a line can be described as (Pa + Pab*t) in parameterized form.
        // a plane can be decribed as  (P0 + P1*u + P2*v) in parameterized form.
        // set them equal to each other (Pa + Pab*t) = (P0 + P1*u + P2*v).
        // create a system of linear equations to find intersection dependen on t, u, v. We care about t.
        // (Pa + Pab*t) where t is now know, gives the point where the line and plane intersect.

        Vector3 sideA = pointA - pointC;
        Vector3 sideB = pointB - pointC;

        float tA = Vector3.Dot(Vector3.Cross(planeU, planeV), (pointA - planePosition)) / Vector3.Dot((-sideA), Vector3.Cross(planeU, planeV));
        Vector3 iA = pointA + (sideA * tA);
        float tB = Vector3.Dot(Vector3.Cross(planeU, planeV), (pointB - planePosition)) / Vector3.Dot((-sideB), Vector3.Cross(planeU, planeV));
        Vector3 iB = pointB + (sideB * tB);
        // return the original verticies of the triangle plus it's intercepts on the plane
        return (new List<Vector3> { iA, iB });
    }

    public void AddTriangleMeshData(Vector3 v0, Vector3 v1, Vector3 v2, Vector2 UV0, Vector2 UV1, Vector2 UV2, int subMeshIndex, bool sign)
    {

        if (sign)
        {
            newPositiveVertices.Add(v0);
            newPositiveVertices.Add(v1);
            newPositiveVertices.Add(v2);
            newPositiveTriangles[subMeshIndex].Add(newPositiveVerticesCount + 0);
            newPositiveTriangles[subMeshIndex].Add(newPositiveVerticesCount + 1);
            newPositiveTriangles[subMeshIndex].Add(newPositiveVerticesCount + 2);
            newPositiveUVs.Add(UV0);
            newPositiveUVs.Add(UV1);
            newPositiveUVs.Add(UV2);
            newPositiveVerticesCount += 3;
        }
        else if (!sign)
        {
            newNegativeVertices.Add(v0);
            newNegativeVertices.Add(v1);
            newNegativeVertices.Add(v2);
            newNegativeTriangles[subMeshIndex].Add(newNegativeVerticesCount + 0);
            newNegativeTriangles[subMeshIndex].Add(newNegativeVerticesCount + 1);
            newNegativeTriangles[subMeshIndex].Add(newNegativeVerticesCount + 2);
            newNegativeUVs.Add(UV0);
            newNegativeUVs.Add(UV1);
            newNegativeUVs.Add(UV2);
            newNegativeVerticesCount += 3;
        }
    }

    public (List<Vector3>, List<int>[], List<Vector2>) DeleteMesh(List<Vector3> oldVertices, List<int>[] oldTriangles, List<Vector2> oldUVs, List<Vector3> planeNormals, List<Vector3> planePositions)
    {
        Plane slicePlane0 = new Plane(planeNormals[0], planePositions[0]);
        Plane slicePlane1 = new Plane(planeNormals[1], planePositions[1]);
        Plane slicePlane2 = new Plane(planeNormals[2], planePositions[2]);
        Plane slicePlane3 = new Plane(planeNormals[3], planePositions[3]);

        List<Vector3> newVertices = new List<Vector3>();
        List<int>[] newTriangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex] = new List<int>();
        }
        List<Vector2> newUVs = new List<Vector2>();

        int newVerticesCount = 0;

        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = 0; i < oldTriangles[subMeshIndex].Count; i += 3)
            {
                int tri0 = oldTriangles[subMeshIndex][i + 0];
                int tri1 = oldTriangles[subMeshIndex][i + 1];
                int tri2 = oldTriangles[subMeshIndex][i + 2];

                // the average position of two midpoints of the line segments of a triangle will always be inside the triangle. That is why it is a good reference for finding whether the triangle sits within the slice planes,
                // assuming that the triangle's edges sit within or on the slice planes.
                Vector3 midpoint01 = (oldVertices[tri0] + oldVertices[tri1]) / 2.0f;
                Vector3 midpoint02 = (oldVertices[tri0] + oldVertices[tri2]) / 2.0f;
                Vector3 point_inside = (midpoint01 + midpoint02) / 2.0f;

                bool VertSign0 = slicePlane0.GetSide(point_inside);
                bool VertSign1 = slicePlane1.GetSide(point_inside);
                bool VertSign2 = slicePlane2.GetSide(point_inside);
                bool VertSign3 = slicePlane3.GetSide(point_inside);

                if (!VertSign0 || !VertSign1 || !VertSign2 || !VertSign3)
                {
                    newVertices.Add(oldVertices[tri0]);
                    newVertices.Add(oldVertices[tri1]);
                    newVertices.Add(oldVertices[tri2]);
                    newTriangles[subMeshIndex].Add(newVerticesCount + 0);
                    newTriangles[subMeshIndex].Add(newVerticesCount + 1);
                    newTriangles[subMeshIndex].Add(newVerticesCount + 2);
                    newUVs.Add(oldUVs[tri0]);
                    newUVs.Add(oldUVs[tri1]);
                    newUVs.Add(oldUVs[tri2]);

                    newVerticesCount += 3;
                }
            }
        }

        return (newVertices, newTriangles, newUVs);
    }
    public (List<Vector3>, List<int>[], List<Vector2>) WeldMesh(List<Vector3> oldVertices, List<int>[] oldTriangles, List<Vector2> oldUVs, float maximumDifference)
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int>[] newTriangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex] = new List<int>();
        }
        List<Vector2> newUVs = new List<Vector2>();

        // this is to account for the amount of triangles added so far from all the previous sub-meshes
        int subMeshOffset = 0;

        // loop through every old vertex in the mesh
        // NOTE: because triangles and vertices are bijective before welding, the triangle count can be used as a substitute for the vertex count.
        // This is useful because it allows the weld to work by sub-mesh rather than the whole mesh.
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = subMeshOffset; i < oldTriangles[subMeshIndex].Count + subMeshOffset; i++)
            {
                // get the vertex information, and assume that it is not a duplicate to begin with
                Vector3 oldVertex = oldVertices[i];
                Vector2 oldUV = oldUVs[i];
                bool areDuplicates = false;
                // loop through all the vertices pushed to the new mesh so far. If this old vertex happens to be a duplicate of the new mesh's vertex, don't add it to the new mesh.
                for (int j = 0; j < newVertices.Count; j++)
                {
                    Vector3 newVertex = newVertices[j];
                    Vector2 newUV = newUVs[j];
                    // if the vertex is a duplicate, don't add it and add the triangle index for the corresponding vertex.
                    if (Vector3.Magnitude(newVertex - oldVertex) <= maximumDifference && Vector3.Magnitude(newUV - oldUV) <= maximumDifference)
                    {
                        newTriangles[subMeshIndex].Add(j);
                        areDuplicates = true;
                    }
                }
                // if there are no duplicates, add new mesh data for the vertex and its corresponding triangle index.
                if (!areDuplicates)
                {
                    newTriangles[subMeshIndex].Add(newVertices.Count);
                    newVertices.Add(oldVertex);
                    newUVs.Add(oldUV);
                }
            }
            // increment the sub-mesh offset by the number of new triangles for this current sub-mesh
            subMeshOffset += oldTriangles[subMeshIndex].Count;
        }
        return (newVertices, newTriangles, newUVs);
    }
}