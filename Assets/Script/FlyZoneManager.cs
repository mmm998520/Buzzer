using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyZoneManager : MonoBehaviour
{
    public List<Transform> flyZoneGroups;
    public List<List<Transform>> flyZoneTransforms = new List<List<Transform>>();
    public List<List<Vector3>> flyZoneVectors = new List<List<Vector3>>();

    void Start()
    {
        setting();
    }

    void Update()
    {
        checkTransformChange();
    }

    public void setFlyZoneVectors(Transform transform, Vector3 vector,int flyZoneVectorsPosi, int flyZoneVectorsPosj)
    {
        flyZoneVectors[flyZoneVectorsPosi][flyZoneVectorsPosj] = vector;
    }

    public void setting()
    {
        List<Transform> fzTransforms = new List<Transform>();
        List<Vector3> fzVectors = new List<Vector3>();
        for (int i = 0; i < flyZoneGroups.Count; i++)
        {
            fzTransforms.Clear();
            fzVectors.Clear();
            for (int j = 0; j < flyZoneGroups[i].childCount; j++)
            {
                Transform child = flyZoneGroups[i].GetChild(j);
                fzTransforms.Add(child);
                fzVectors.Add(child.position);
            }
            flyZoneTransforms.Add(fzTransforms);
            flyZoneVectors.Add(fzVectors);
        }
    }

    public void checkTransformChange()
    {
        for (int i = 0; i < flyZoneTransforms.Count; i++)
        {
            for (int j = 0; j < flyZoneTransforms[i].Count; j++)
            {
                if (flyZoneTransforms[i][j].hasChanged)
                {
                    setFlyZoneVectors(flyZoneTransforms[i][j], flyZoneTransforms[i][j].position, i,j);
                }
            }
        }
    }

    public void addPoint(Transform newPoint, Transform group, int order)
    {
        int parantIndex = group.GetSiblingIndex();
        newPoint.SetParent(group);
        newPoint.SetSiblingIndex(order);
        flyZoneTransforms[parantIndex].Insert(2, newPoint);
        flyZoneVectors[parantIndex].Insert(2, newPoint.position);
        group.GetComponent<FlyZoneSpriteController>().addPoint(newPoint, order);
    }
}
