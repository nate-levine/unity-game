using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Sliceable : MonoBehaviour
{
    List<Vector3> newVertices = new List<Vector3>();
    List<int>[] newTriangles = new List<int>[2];
    List<Vector3> newNormals = new List<Vector3>();
    List<Vector2> newUVs = new List<Vector2>();
    List<Vector2> newUV2s = new List<Vector2>();
    List<Color> newColors = new List<Color>();
    List<Vector3> newPositiveVertices = new List<Vector3>();
    List<int>[] newPositiveTriangles = new List<int>[2];
    List<Vector3> newPositiveNormals = new List<Vector3>();
    List<Vector2> newPositiveUVs = new List<Vector2>();
    List<Vector2> newPositiveUV2s = new List<Vector2>();
    List<Color> newPositiveColors = new List<Color>();
    List<Vector3> newNegativeVertices = new List<Vector3>();
    List<int>[] newNegativeTriangles = new List<int>[2];
    List<Vector3> newNegativeNormals = new List<Vector3>();
    List<Vector2> newNegativeUVs = new List<Vector2>();
    List<Vector2> newNegativeUV2s = new List<Vector2>();
    List<Color> newNegativeColors = new List<Color>();

    public enum LINE
    {
        NONE = -1,
        AB = 0,
        BC = 1,
        CA = 2
    }

    void Start()
    {
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex] = new List<int>();
            newPositiveTriangles[subMeshIndex] = new List<int>();
            newNegativeTriangles[subMeshIndex] = new List<int>();
        }
    }

    public (List<Vector3>, List<int>[], List<Vector3>, List<Vector2>, List<Vector2>, List<Color>, List<Vector3>, List<int>[], List<Vector3>, List<Vector2>, List<Vector2>, List<Color>) 
    LineSliceMesh(List<Vector3> oldVertices, List<int>[] oldTriangles, List<Vector3> oldNormals, List<Vector2> oldUVs, List<Vector2> oldUV2s, List<Color> oldColors, Vector3 planeNormal, Vector3 planePosition, float planeBoundsMinimum, float planeBoundsMaximum)
    {
        // slice plane is the plane that the mesh will be sliced by.
        Plane slicePlane = new Plane(planeNormal, planePosition);
        Vector3 planeDir = Vector3.Normalize(Vector3.Cross(planeNormal, new Vector3(0.0f, 0.0f, 1.0f)));

        newVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex].Clear();
        }
        newNormals.Clear();
        newUVs.Clear();
        newUV2s.Clear();
        newColors.Clear();
        newPositiveVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newPositiveTriangles[subMeshIndex].Clear();
        }
        newPositiveNormals.Clear();
        newPositiveUVs.Clear();
        newPositiveUV2s.Clear();
        newPositiveColors.Clear();
        newNegativeVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newNegativeTriangles[subMeshIndex].Clear();
        }
        newNegativeNormals.Clear();
        newNegativeUVs.Clear();
        newNegativeUV2s.Clear();
        newNegativeColors.Clear();

        List<Vector3> vertices = oldVertices;
        List<int>[] triangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            triangles[subMeshIndex] = oldTriangles[subMeshIndex];
        }
        List<Vector3> normals = oldNormals;
        List<Vector2> UVs = oldUVs;
        List<Vector2> UV2s = oldUV2s;
        List<Color> colors = oldColors;

        // sort vertices into positive and negative sides and create a map for the indices
        List<int> map = new List<int>();
        for (int i = 0; i < oldVertices.Count; i++)
        {
            Vector3 vert = oldVertices[i];
            Vector3 normal = oldNormals[i];
            Vector2 UV = oldUVs[i];
            Vector2 UV2 = oldUV2s[i];
            Color color = oldColors[i];
            bool vertSign = slicePlane.GetSide(vert);

            // positive side
            if (vertSign)
            {
                // map
                map.Add(newPositiveVertices.Count);
                // add to vertices
                newPositiveVertices.Add(vert);
                newPositiveNormals.Add(normal);
                newPositiveUVs.Add(UV);
                newPositiveUV2s.Add(UV2);
                newPositiveColors.Add(color);
            }
            // negative side
            else if (!vertSign)
            {
                // map
                map.Add(newNegativeVertices.Count);
                // add to vertices
                newNegativeVertices.Add(vert);
                newNegativeNormals.Add(normal);
                newNegativeUVs.Add(UV);
                newNegativeUV2s.Add(UV2);
                newNegativeColors.Add(color);
            }
        }
        // to set the index offset for welding later
        List<Vector3> oldPositiveVertices = new List<Vector3>(newPositiveVertices);
        List<Vector3> oldPositiveNormals = new List<Vector3>(newPositiveNormals);
        List<Vector2> oldPositiveUVs = new List<Vector2>(newPositiveUVs);
        List<Vector2> oldPositiveUV2s = new List<Vector2>(newPositiveUV2s);
        List<Color> oldPositiveColors = new List<Color>(newPositiveColors);
        List<Vector3> oldNegativeVertices = new List<Vector3>(newNegativeVertices);
        List<Vector3> oldNegativeNormals = new List<Vector3>(newNegativeNormals);
        List<Vector2> oldNegativeUVs = new List<Vector2>(newNegativeUVs);
        List<Vector2> oldNegativeUV2s = new List<Vector2>(newNegativeUV2s);
        List<Color> oldNegativeColors = new List<Color>(newNegativeColors);

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

                    // If an intersection point is on a line sigment with two edge points, make that intersection an edge point.
                    List<Vector2> slicedUV2Intersections = new List<Vector2>();

                    if ((UV2s[pointIndex0].x == 1.0f) && (UV2s[pointIndex2].x == 1.0f))
                    {
                        slicedUV2Intersections.Add(new Vector2(1.0f, 0.0f));
                    }
                    else
                    {
                        slicedUV2Intersections.Add(new Vector2(0.0f, 0.0f));
                    }
                    if ((UV2s[pointIndex1].x == 1.0f) && (UV2s[pointIndex2].x == 1.0f))
                    {
                        slicedUV2Intersections.Add(new Vector2(1.0f, 0.0f));
                    }
                    else
                    {
                        slicedUV2Intersections.Add(new Vector2(0.0f, 0.0f));
                    }


                    if (Vert1SignSliced && Vert0SignSliced)
                    {
                        // tri 1
                        newPositiveTriangles[subMeshIndex].Add(newPositiveVertices.Count + 0);
                        newPositiveTriangles[subMeshIndex].Add(map[pointIndex1]);
                        newPositiveTriangles[subMeshIndex].Add(newPositiveVertices.Count + 1);
                        // tri 0
                        newPositiveTriangles[subMeshIndex].Add(newPositiveVertices.Count + 0);
                        newPositiveTriangles[subMeshIndex].Add(map[pointIndex0]);
                        newPositiveTriangles[subMeshIndex].Add(map[pointIndex1]);

                        newPositiveVertices.Add(slicedTriangleIntersections[0]);
                        newPositiveVertices.Add(slicedTriangleIntersections[1]);
                        newPositiveNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newPositiveNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newPositiveUVs.Add(slicedUVIntersections[0]);
                        newPositiveUVs.Add(slicedUVIntersections[1]);
                        newPositiveUV2s.Add(slicedUV2Intersections[0]);
                        newPositiveUV2s.Add(slicedUV2Intersections[1]);
                        newPositiveColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                        newPositiveColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                    }
                    else
                    {
                        // tri 1
                        newNegativeTriangles[subMeshIndex].Add(newNegativeVertices.Count + 0);
                        newNegativeTriangles[subMeshIndex].Add(map[pointIndex1]);
                        newNegativeTriangles[subMeshIndex].Add(newNegativeVertices.Count + 1);
                        // tri 0
                        newNegativeTriangles[subMeshIndex].Add(newNegativeVertices.Count + 0);
                        newNegativeTriangles[subMeshIndex].Add(map[pointIndex0]);
                        newNegativeTriangles[subMeshIndex].Add(map[pointIndex1]);

                        newNegativeVertices.Add(slicedTriangleIntersections[0]);
                        newNegativeVertices.Add(slicedTriangleIntersections[1]);
                        newNegativeNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newNegativeNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newNegativeUVs.Add(slicedUVIntersections[0]);
                        newNegativeUVs.Add(slicedUVIntersections[1]);
                        newNegativeUV2s.Add(slicedUV2Intersections[0]);
                        newNegativeUV2s.Add(slicedUV2Intersections[1]);
                        newNegativeColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                        newNegativeColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                    }
                    if (Vert2SignSliced)
                    {
                        newPositiveTriangles[subMeshIndex].Add(map[pointIndex2]);
                        newPositiveTriangles[subMeshIndex].Add(newPositiveVertices.Count + 0);
                        newPositiveTriangles[subMeshIndex].Add(newPositiveVertices.Count + 1);

                        newPositiveVertices.Add(slicedTriangleIntersections[0]);
                        newPositiveVertices.Add(slicedTriangleIntersections[1]);
                        newPositiveNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newPositiveNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newPositiveUVs.Add(slicedUVIntersections[0]);
                        newPositiveUVs.Add(slicedUVIntersections[1]);
                        newPositiveUV2s.Add(slicedUV2Intersections[0]);
                        newPositiveUV2s.Add(slicedUV2Intersections[1]);
                        newPositiveColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                        newPositiveColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                    }
                    else
                    {
                        newNegativeTriangles[subMeshIndex].Add(map[pointIndex2]);
                        newNegativeTriangles[subMeshIndex].Add(newNegativeVertices.Count + 0);
                        newNegativeTriangles[subMeshIndex].Add(newNegativeVertices.Count + 1);

                        newNegativeVertices.Add(slicedTriangleIntersections[0]);
                        newNegativeVertices.Add(slicedTriangleIntersections[1]);
                        newNegativeNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newNegativeNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                        newNegativeUVs.Add(slicedUVIntersections[0]);
                        newNegativeUVs.Add(slicedUVIntersections[1]);
                        newNegativeUV2s.Add(slicedUV2Intersections[0]);
                        newNegativeUV2s.Add(slicedUV2Intersections[1]);
                        newNegativeColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                        newNegativeColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                    }
                }
                // if there is no intersection, keep the original triangle.
                // NOTE: Because of the way this algorithm works, these triangles that fall on the line but NOT the line segment are composed of triangles mapped to both the
                // positive and negative vertices. Therefore, they are not created properly. This needs to be fixed if the line segment is not arbitrarily large in magnitude.
                else
                {
                    if (!Vert0Sign || !Vert1Sign || !Vert2Sign)
                    {
                        newNegativeTriangles[subMeshIndex].Add(map[tri0]);
                        newNegativeTriangles[subMeshIndex].Add(map[tri1]);
                        newNegativeTriangles[subMeshIndex].Add(map[tri2]);
                    }
                    else
                    {
                        newPositiveTriangles[subMeshIndex].Add(map[tri0]);
                        newPositiveTriangles[subMeshIndex].Add(map[tri1]);
                        newPositiveTriangles[subMeshIndex].Add(map[tri2]);
                    }
                }
            }
        }
        // weld vertices along slice line
        List<Vector3> positiveVerticesToWeld = new List<Vector3>();
        List<Vector3> positiveNormalsToWeld = new List<Vector3>();
        List<Vector2> positiveUVsToWeld = new List<Vector2>();
        List<Vector2> positiveUV2sToWeld = new List<Vector2>();
        List<Color> positiveColorsToWeld = new List<Color>();
        for (int i = oldPositiveVertices.Count; i < newPositiveVertices.Count; i++)
        {
            positiveVerticesToWeld.Add(newPositiveVertices[i]);
            positiveNormalsToWeld.Add(newPositiveNormals[i]);
            positiveUVsToWeld.Add(newPositiveUVs[i]);
            positiveUV2sToWeld.Add(newPositiveUV2s[i]);
            positiveColorsToWeld.Add(newPositiveColors[i]);
        }
        var (positiveWeldedVertices, positiveWeldedNormals, positiveWeldedUVs, positiveWeldedUV2s, positiveWeldedColors, positiveWeldMap) = WeldVertices(positiveVerticesToWeld, positiveNormalsToWeld, positiveUVsToWeld, positiveUV2sToWeld, positiveColorsToWeld, 0.0f);


        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = 0; i < newPositiveTriangles[subMeshIndex].Count; i++)
            {
                // map
                if (newPositiveTriangles[subMeshIndex][i] >= oldPositiveVertices.Count)
                    newPositiveTriangles[subMeshIndex][i] = positiveWeldMap[newPositiveTriangles[subMeshIndex][i] - oldPositiveVertices.Count] + oldPositiveVertices.Count;
            }
        }
        newPositiveVertices = oldPositiveVertices;
        newPositiveVertices.AddRange(positiveWeldedVertices);
        newPositiveNormals = oldPositiveNormals;
        newPositiveNormals.AddRange(positiveWeldedNormals);
        newPositiveUVs = oldPositiveUVs;
        newPositiveUV2s = oldPositiveUV2s;
        newPositiveUVs.AddRange(positiveWeldedUVs);
        newPositiveUV2s.AddRange(positiveWeldedUV2s);
        newPositiveColors = oldPositiveColors;
        newPositiveColors.AddRange(positiveWeldedColors);
        List<Vector3> negativeVerticesToWeld = new List<Vector3>();
        List<Vector3> negativeNormalsToWeld = new List<Vector3>();
        List<Vector2> negativeUVsToWeld = new List<Vector2>();
        List<Vector2> negativeUV2sToWeld = new List<Vector2>();
        List<Color> negativeColorsToWeld = new List<Color>();
        for (int i = oldNegativeVertices.Count; i < newNegativeVertices.Count; i++)
        {
            negativeVerticesToWeld.Add(newNegativeVertices[i]);
            negativeNormalsToWeld.Add(newNegativeNormals[i]);
            negativeUVsToWeld.Add(newNegativeUVs[i]);
            negativeUV2sToWeld.Add(newNegativeUV2s[i]);
            negativeColorsToWeld.Add(newNegativeColors[i]);
        }
        var (negativeWeldedVertices, negativeWeldedNormals, negativeWeldedUVs, negativeWeldedUV2s, negativeWeldedColors, negativeWeldMap) = WeldVertices(negativeVerticesToWeld, negativeNormalsToWeld, negativeUVsToWeld, negativeUV2sToWeld, negativeColorsToWeld, 0.0f);


        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = 0; i < newNegativeTriangles[subMeshIndex].Count; i++)
            {
                // map
                if (newNegativeTriangles[subMeshIndex][i] >= oldNegativeVertices.Count)
                    newNegativeTriangles[subMeshIndex][i] = negativeWeldMap[newNegativeTriangles[subMeshIndex][i] - oldNegativeVertices.Count] + oldNegativeVertices.Count;
            }
        }
        newNegativeVertices = oldNegativeVertices;
        newNegativeVertices.AddRange(negativeWeldedVertices);
        newNegativeNormals = oldNegativeNormals;
        newNegativeNormals.AddRange(negativeWeldedNormals);
        newNegativeUVs = oldNegativeUVs;
        newNegativeUV2s = oldNegativeUV2s;
        newNegativeUVs.AddRange(negativeWeldedUVs);
        newNegativeUV2s.AddRange(negativeWeldedUV2s);
        newNegativeColors = oldNegativeColors;
        newNegativeColors.AddRange(negativeWeldedColors);

        // return
        return (newPositiveVertices, newPositiveTriangles, newPositiveNormals, newPositiveUVs, newPositiveUV2s, newPositiveColors, newNegativeVertices, newNegativeTriangles, newNegativeNormals, newNegativeUVs, newNegativeUV2s, newNegativeColors);
    }

    public (List<Vector3>, List<int>[], List<Vector3>, List<Vector2>, List<Vector2>, List<Color>) LineSegmentSliceMesh(List<Vector3> oldVertices, List<int>[] oldTriangles, List<Vector3> oldNormals, List<Vector2> oldUVs, List<Vector2> oldUV2s, List<Color> oldColors, Vector3 planeNormal, Vector3 planePosition, float planeBoundsMinimum, float planeBoundsMaximum)
    {
        // slice plane is the plane that the mesh will be sliced by.
        Plane slicePlane = new Plane(planeNormal, planePosition);
        Vector3 planeDir = Vector3.Normalize(Vector3.Cross(planeNormal, new Vector3(0.0f, 0.0f, 1.0f)));

        newVertices.Clear();
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex].Clear();
        }
        newNormals.Clear();
        newUVs.Clear();
        newUV2s.Clear();
        newColors.Clear();

        List<Vector3> vertices = oldVertices;
        List<int>[] triangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            triangles[subMeshIndex] = oldTriangles[subMeshIndex];
        }
        List<Vector3> normals = oldNormals;
        List<Vector2> UVs = oldUVs;
        List<Vector2> UV2s = oldUVs;
        List<Color> colors = oldColors;

        for (int i = 0; i < oldVertices.Count; i++)
        {
            Vector3 vert = oldVertices[i];
            Vector3 normal = oldNormals[i];
            Vector2 UV = oldUVs[i];
            Vector2 UV2 = oldUV2s[i];
            Color color = oldColors[i];

            newVertices.Add(vert);
            newNormals.Add(normal);
            newUVs.Add(UV);
            newUV2s.Add(UV2);
            newColors.Add(color);
        }

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

                // Degenerate triangle(s) calculations
                bool intersectsAB = false;
                bool intersectsBC = false;
                bool intersectsCA = false;
                if (uAB >= 0.0f && uAB <= 1.0f && tAB >= planeBoundsMinimum && tAB <= planeBoundsMaximum)
                    intersectsAB = true;
                if (uBC >= 0.0f && uBC <= 1.0f && tBC >= planeBoundsMinimum && tBC <= planeBoundsMaximum)
                    intersectsBC = true;
                if (uCA >= 0.0f && uCA <= 1.0f && tCA >= planeBoundsMinimum && tCA <= planeBoundsMaximum)
                    intersectsCA = true;

                LINE singleLineIntersection = LINE.NONE;
                int degenerateTriangleIntersectionPointIndex = 0;

                // if there is an intersection between the plane (-0.5 to 0.5) and the line segment (0.0 to 1.0), then slice along the plane by seperating the triangle into 3 new triangles.
                // also find the intersection if the entire plane sits inside the triangle, and therefore does not intersect it.
                if (intersectsAB || intersectsBC || intersectsCA || sliceInside)
                {
                    if (intersectsAB && !(intersectsBC || intersectsCA))
                    {
                        singleLineIntersection = LINE.AB;
                    }
                    else if (intersectsBC && !(intersectsCA || intersectsAB))
                    {
                        singleLineIntersection = LINE.BC;
                    }
                    else if (intersectsCA && !(intersectsAB || intersectsBC))
                    {
                        singleLineIntersection = LINE.CA;
                    }

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

                        if (singleLineIntersection == LINE.BC)
                        {
                            degenerateTriangleIntersectionPointIndex = 0;
                        }
                        else if (singleLineIntersection == LINE.CA)
                        {
                            degenerateTriangleIntersectionPointIndex = 1;
                        }
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

                        if (singleLineIntersection == LINE.AB)
                        {
                            degenerateTriangleIntersectionPointIndex = 1;
                        }
                        else if (singleLineIntersection == LINE.CA)
                        {
                            degenerateTriangleIntersectionPointIndex = 0;
                        }
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

                        if (singleLineIntersection == LINE.AB)
                        {
                            degenerateTriangleIntersectionPointIndex = 0;
                        }
                        else if (singleLineIntersection == LINE.BC)
                        {
                            degenerateTriangleIntersectionPointIndex = 1;
                        }
                    }

                    List<Vector3> slicedTriangleIntersections = FindIntersections(planeNormal, planePosition, point0, point1, point2);

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
                    
                    // If an intersection point is on a line sigment with two edge points, make that intersection an edge point.
                    List<Vector2> slicedUV2Intersections = new List<Vector2>();

                    if ((UV2s[pointIndex0].x == 1.0f) && (UV2s[pointIndex2].x == 1.0f))
                    {
                        slicedUV2Intersections.Add(new Vector2(1.0f, 0.0f));
                    }
                    else
                    {
                        slicedUV2Intersections.Add(new Vector2(0.0f, 0.0f));
                    }
                    if ((UV2s[pointIndex1].x == 1.0f) && (UV2s[pointIndex2].x == 1.0f))
                    {
                        slicedUV2Intersections.Add(new Vector2(1.0f, 0.0f));
                    }
                    else
                    {
                        slicedUV2Intersections.Add(new Vector2(0.0f, 0.0f));
                    }

                    // tri 1
                    newTriangles[subMeshIndex].Add(newVertices.Count + 0);
                    newTriangles[subMeshIndex].Add(pointIndex1);
                    newTriangles[subMeshIndex].Add(newVertices.Count + 1);
                    // tri 0
                    newTriangles[subMeshIndex].Add(newVertices.Count + 0);
                    newTriangles[subMeshIndex].Add(pointIndex0);
                    newTriangles[subMeshIndex].Add(pointIndex1);
                    // tri 2
                    newTriangles[subMeshIndex].Add(pointIndex2);
                    newTriangles[subMeshIndex].Add(newVertices.Count + 0);
                    newTriangles[subMeshIndex].Add(newVertices.Count + 1);

                    // Create "degenerate" triangles. These are triangles that have no area, but close the open seams leftover from the mesh slicing,
                    // specifically where a slice ends and there is another, unsliced triangle's, unconnected line segment.
                    if (singleLineIntersection != LINE.NONE)
                    {
                        if (degenerateTriangleIntersectionPointIndex == 0)
                        {
                            newTriangles[subMeshIndex].Add(newVertices.Count + 0);
                            newTriangles[subMeshIndex].Add(pointIndex0);
                            newTriangles[subMeshIndex].Add(pointIndex2);
                        }
                        else if (degenerateTriangleIntersectionPointIndex == 1)
                        {
                            newTriangles[subMeshIndex].Add(newVertices.Count + 1);
                            newTriangles[subMeshIndex].Add(pointIndex2);
                            newTriangles[subMeshIndex].Add(pointIndex1);
                        }
                    }
                    // If the slice is confined to within the triangle, create degenerate triangles on both sides where the slice ends.
                    if (sliceInside)
                    {
                        // left side
                        newTriangles[subMeshIndex].Add(newVertices.Count + 0);
                        newTriangles[subMeshIndex].Add(pointIndex0);
                        newTriangles[subMeshIndex].Add(pointIndex2);
                        // right side
                        newTriangles[subMeshIndex].Add(newVertices.Count + 1);
                        newTriangles[subMeshIndex].Add(pointIndex2);
                        newTriangles[subMeshIndex].Add(pointIndex1);
                    }

                    // create vertices
                    newVertices.Add(slicedTriangleIntersections[0]);
                    newVertices.Add(slicedTriangleIntersections[1]);
                    newNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                    newNormals.Add(new Vector3(0.0f, 0.0f, -1.0f));
                    newUVs.Add(slicedUVIntersections[0]);
                    newUVs.Add(slicedUVIntersections[1]);
                    newUV2s.Add(slicedUV2Intersections[0]);
                    newUV2s.Add(slicedUV2Intersections[1]);
                    newColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));
                    newColors.Add(new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f));

                }
                // if there is no intersection, keep the original triangle.
                // NOTE: Because of the way this algorithm works, these triangles that fall on the line but NOT the line segment are composed of triangles mapped to both the
                // positive and negative vertices. Therefore, they are not created properly. This needs to be fixed if the line segment is not arbitrarily large in magnitude.
                else
                {
                    newTriangles[subMeshIndex].Add(tri0);
                    newTriangles[subMeshIndex].Add(tri1);
                    newTriangles[subMeshIndex].Add(tri2);
                }
            }
        }

        // weld vertices along slice line
        List<Vector3> verticesToWeld = new List<Vector3>();
        List<Vector3> normalsToWeld = new List<Vector3>();
        List<Vector2> UVsToWeld = new List<Vector2>();
        List<Vector2> UV2sToWeld = new List<Vector2>();
        List<Color> colorsToWeld = new List<Color>();
        for (int i = oldVertices.Count; i < newVertices.Count; i++)
        {
            verticesToWeld.Add(newVertices[i]);
            normalsToWeld.Add(newNormals[i]);
            UVsToWeld.Add(newUVs[i]);
            UV2sToWeld.Add(newUV2s[i]);
            colorsToWeld.Add(newColors[i]);
        }
        var (weldedVertices, weldedNormals, weldedUVs, weldedUV2s, weldedColors, weldMap) = WeldVertices(verticesToWeld, normalsToWeld, UVsToWeld, UV2sToWeld, colorsToWeld, 0.0f);


        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = 0; i < newTriangles[subMeshIndex].Count; i++)
            {
                // map
                if (newTriangles[subMeshIndex][i] >= oldVertices.Count)
                    newTriangles[subMeshIndex][i] = weldMap[newTriangles[subMeshIndex][i] - oldVertices.Count] + oldVertices.Count;
            }
        }
        newVertices = oldVertices;
        newVertices.AddRange(weldedVertices);
        newNormals = oldNormals;
        newNormals.AddRange(weldedNormals);
        newUVs = oldUVs;
        newUV2s = oldUV2s;
        newUVs.AddRange(weldedUVs);
        newUV2s.AddRange(weldedUV2s);
        newColors = oldColors;
        newColors.AddRange(weldedColors);

        // return
        return (newVertices, newTriangles, newNormals, newUVs, newUV2s, newColors);
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

    /**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**//**/

    public (List<Vector3>, List<int>[], List<Vector3>, List<Vector2>, List<Vector2>, List<Color>) DeleteMesh(List<Vector3> oldVertices, List<int>[] oldTriangles, List<Vector3> oldNormals, List<Vector2> oldUVs, List<Vector2> oldUV2s, List<Color> oldColors, List<Vector3> planeNormals, List<Vector3> planePositions)
    {
        List<Vector3> vertices = oldVertices;
        List<int>[] triangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            triangles[subMeshIndex] = oldTriangles[subMeshIndex];
        }
        List<Vector3> normals = oldNormals;
        List<Vector2> UVs = oldUVs;
        List<Vector2> UV2s = oldUVs;
        List<Color> colors = oldColors;
        List<Vector3> newVertices = new List<Vector3>();
        List<int>[] newTriangles = new List<int>[2];
        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            newTriangles[subMeshIndex] = new List<int>();
        }
        List<Vector3> newNormals = new List<Vector3>();
        List<Vector2> newUVs = new List<Vector2>();
        List<Vector2> newUV2s = new List<Vector2>();
        List<Color> newColors = new List<Color>();

        // sort vertices into ones to keep and ones to delete and creates a map for the indices
        List<int> map = new List<int>();
        for (int i = 0; i < oldVertices.Count; i++)
        {
            Vector3 vert = oldVertices[i];
            Vector3 normal = oldNormals[i];
            Vector2 UV = oldUVs[i];
            Vector2 UV2 = oldUV2s[i];
            Color color = oldColors[i];

            float vert0Dot = Vector3.Dot(planeNormals[0], (vert - planePositions[0]));
            float vert1Dot = Vector3.Dot(planeNormals[1], (vert - planePositions[1]));
            float vert2Dot = Vector3.Dot(planeNormals[2], (vert - planePositions[2]));
            float vert3Dot = Vector3.Dot(planeNormals[3], (vert - planePositions[3]));

            // A "botched" together solution. It seems like perpendicular vectors sometimes give a dot product of slightly larger that 0.0f.
            float TINY_VALUE = 0.0001f;
            if ((vert0Dot <= TINY_VALUE) || (vert1Dot <= TINY_VALUE) || (vert2Dot <= TINY_VALUE) || (vert3Dot <= TINY_VALUE))
            {
                // map
                map.Add(newVertices.Count);
                // mesh data
                newVertices.Add(vert);
                newNormals.Add(normal);
                newUVs.Add(UV);
                // if within the bounds of the cut, assign edge
                if ((vert0Dot >= -TINY_VALUE) &&
                    (vert1Dot >= -TINY_VALUE) &&
                    (vert2Dot >= -TINY_VALUE) &&
                    (vert3Dot >= -TINY_VALUE) ||
                    (UV2 == new Vector2(1.0f, 0.0f)))
                {
                    newUV2s.Add(new Vector2(1.0f, 0.0f));
                }
                else
                {
                    newUV2s.Add(new Vector2(0.0f, 0.0f));
                }
                newColors.Add(color);
            }
            else
            {
                map.Add(-1);
            }
        }

        for (int subMeshIndex = 0; subMeshIndex < 2; subMeshIndex++)
        {
            for (int i = 0; i < triangles[subMeshIndex].Count; i += 3)
            {
                int tri0 = triangles[subMeshIndex][i + 0];
                int tri1 = triangles[subMeshIndex][i + 1];
                int tri2 = triangles[subMeshIndex][i + 2];

                if ((map[tri0] != -1) && (map[tri1] != -1) && (map[tri2] != -1))
                {
                    // the average position of two midpoints of the line segments of a triangle will always be inside the triangle. That is why it is a good reference for finding whether the triangle sits within the slice planes,
                    // assuming that the triangle's edges sit within or on the slice planes.
                    Vector3 midpoint01 = (newVertices[map[tri0]] + newVertices[map[tri1]]) / 2.0f;
                    Vector3 midpoint02 = (newVertices[map[tri0]] + newVertices[map[tri2]]) / 2.0f;
                    Vector3 pointInside = (midpoint01 + midpoint02) / 2.0f;

                    float pointInsideDot0 = Vector3.Dot(planeNormals[0], (pointInside - planePositions[0]));
                    float pointInsideDot1 = Vector3.Dot(planeNormals[1], (pointInside - planePositions[1]));
                    float pointInsideDot2 = Vector3.Dot(planeNormals[2], (pointInside - planePositions[2]));
                    float pointInsideDot3 = Vector3.Dot(planeNormals[3], (pointInside - planePositions[3]));

                    if ((pointInsideDot0 <= 0.0f) || (pointInsideDot1 <= 0.0f) || (pointInsideDot2 <= 0.0f) || (pointInsideDot3 <= 0.0f))
                    {
                        newTriangles[subMeshIndex].Add(map[tri0]);
                        newTriangles[subMeshIndex].Add(map[tri1]);
                        newTriangles[subMeshIndex].Add(map[tri2]);
                    }
                }
            }
        }

        return (newVertices, newTriangles, newNormals, newUVs, newUV2s, newColors);
    }
    public (List<Vector3>, List<Vector3>, List<Vector2>, List<Vector2>, List<Color>, List<int>) WeldVertices(List<Vector3> verticesToWeld, List<Vector3> normalsToWeld, List<Vector2> UVsToWeld, List<Vector2> UV2sToWeld, List<Color> colorsToWeld, float maximumDifference)
    {
        List<Vector3> weldedVertices = new List<Vector3>();
        List<Vector3> weldedNormals = new List<Vector3>();
        List<Vector2> weldedUVs = new List<Vector2>();
        List<Vector2> weldedUV2s = new List<Vector2>();
        List<Color> weldedColors = new List<Color>();
        List<int> weldMap = new List<int>();

        // loop through every old vertex in the mesh
        for (int i = 0; i < verticesToWeld.Count; i++)
        {
            // get the vertex information, and assume that it is not a duplicate to begin with
            Vector3 vertexToWeld = verticesToWeld[i];
            Vector3 normalToWeld = normalsToWeld[i];
            Vector2 UVToWeld = UVsToWeld[i];
            Vector2 UV2ToWeld = UV2sToWeld[i];
            Color colorToWeld = colorsToWeld[i];
            bool areDuplicates = false;
            // loop through all the vertices pushed to the new mesh so far. If this old vertex happens to be a duplicate of the new mesh's vertex, don't add it to the new mesh.
            for (int j = 0; j < weldedVertices.Count; j++)
            {
                Vector3 weldedVertex = weldedVertices[j];
                Vector3 weldedNormal = weldedNormals[j];
                Vector2 weldedUV = weldedUVs[j];
                Vector2 weldedUV2 = weldedUV2s[j];
                Color weldedColor = weldedColors[j];
                // if the vertex is a duplicate, don't add it and add the triangle index for the corresponding vertex.
                if (!areDuplicates && Vector3.Magnitude(weldedVertex - vertexToWeld) <= maximumDifference && Vector3.Magnitude(weldedUV - UVToWeld) <= maximumDifference)
                {
                    weldMap.Add(j);
                    areDuplicates = true;
                }
            }
            // if there are no duplicates, add new mesh data for the vertex and its corresponding triangle index.
            if (!areDuplicates)
            {
                // map
                weldMap.Add(weldedVertices.Count);
                // mesh data
                weldedVertices.Add(vertexToWeld);
                weldedNormals.Add(normalToWeld);
                weldedUVs.Add(UVToWeld);
                weldedUV2s.Add(UV2ToWeld);
                weldedColors.Add(colorToWeld);
            }
        }
        
        return (weldedVertices, weldedNormals, weldedUVs, weldedUV2s, weldedColors, weldMap);
    }
}