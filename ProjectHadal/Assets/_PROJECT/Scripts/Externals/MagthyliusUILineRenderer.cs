using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public Vector2 gridSize;
    public List<Vector2> points;

    float width;
    float height;
    float unitWidth;
    float unitHeight;

    public float thickness = 10f;
    public bool noBend = false;
    List<Vector2> bendPoints;

    float useless;
    float Useless
    {
        get { return useless; }
        set 
        {
            useless = value;
		    SetVerticesDirty();
	    }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        DrawEverything(vh);
    }

    public void DrawEverything(VertexHelper vh)
    {
        vh.Clear();

        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        unitWidth = width / gridSize.x;
        unitHeight = height / gridSize.y;

        if (points.Count < 2) return;

        float angle = 0f;
        if (noBend)
        {
            bendPoints = new List<Vector2>();
            bendPoints.Add(points[0]);
            if (points.Count > 2)
            {
                for (int i = 1; i < points.Count - 1; i++)
                {
                    //! add twice
                    bendPoints.Add(points[i]);
                    bendPoints.Add(points[i]);
                }
            }
            bendPoints.Add(points[points.Count - 1]);

            //! render bend
            for (int i = 0; i < bendPoints.Count; i += 2)
            {
                angle = 0f;
                Vector2 point = bendPoints[i];

                if (i < bendPoints.Count - 1)
                {
                    angle = GetAngle(bendPoints[i], bendPoints[i + 1]) + 90f;
                }

                DrawVerticesForPoint(bendPoints[i], vh, angle);
                DrawVerticesForPoint(bendPoints[i + 1], vh, angle);
                //print(i + " angle: " + angle);
            }

            for (int i = 0; i < bendPoints.Count - 1; i++)
            {
                int index = i * 2;
                vh.AddTriangle(index + 0, index + 1, index + 3);
                vh.AddTriangle(index + 3, index + 2, index + 0);
            }
        }
        else
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                if (i < points.Count - 1)
                {
                    angle = GetAngle(points[i], points[i + 1]) + 45f;
                }

                DrawVerticesForPoint(point, vh, angle);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                int index = i * 2;
                vh.AddTriangle(index + 0, index + 1, index + 3);
                vh.AddTriangle(index + 3, index + 2, index + 0);
            }
        }
    }

    void DrawVerticesForPoint(Vector2 point, VertexHelper vh, float angle)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(unitWidth * point.x, unitHeight * point.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(unitWidth * point.x, unitHeight * point.y);
        vh.AddVert(vertex);
    }

    float GetAngle(Vector2 self, Vector2 target)
    {
        return (float)(Mathf.Atan2(target.y - self.y, target.x - self.x) * (180 / Mathf.PI));
    }

    public void UpdatePoints(List<Vector2> newPoints)
    {
        points = newPoints;
        //Canvas.ForceUpdateCanvases();
        Useless++;
    }
}
