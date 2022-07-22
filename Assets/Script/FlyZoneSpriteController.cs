using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class FlyZoneSpriteController : MonoBehaviour
{
    SpriteShapeController spriteShapeController;
    List<Transform> polygonPoints = new List<Transform>();

    void Start()
    {
        for(int i=0;i<transform.childCount;i++)
        {
            polygonPoints.Add(transform.GetChild(i));
        }
        spriteShapeController = GetComponent<SpriteShapeController>();
    }

    void Update()
    {
        for (int i = 0; i < polygonPoints.Count; i++)
        {
            spriteShapeController.spline.SetPosition(i, polygonPoints[i].position);
        }
    }

    public void addPoint(Transform target, int order)
    {
        spriteShapeController.spline.InsertPointAt(order, target.position);
        polygonPoints.Insert(order, target);
    }
}
