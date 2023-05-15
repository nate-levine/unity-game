using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    private LineRenderer m_lineRenderer;
    private List<Vector3> m_points;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        for (int i = 0; i < m_lineRenderer.positionCount; i++)
        {
            m_lineRenderer.SetPosition(i, m_points[i]);
        }
    }

    public void RenderLine(List<Vector3> points)
    {
        m_points = points;
        m_lineRenderer.positionCount = m_points.Count;
    }
}
