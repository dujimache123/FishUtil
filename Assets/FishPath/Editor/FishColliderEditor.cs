using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FishCollider))]
public class FishColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FishCollider collider = (FishCollider)target;
        string scaleStr = "";
        int childcnt = collider.transform.childCount;
        for (int i = 0; i < childcnt; i++)
        {
            Transform child = collider.transform.FindChild(i.ToString());
            scaleStr += child.localScale.x.ToString() + ",";
        }
        scaleStr = scaleStr.TrimEnd(',');
        EditorGUILayout.TextArea(scaleStr);

        GUILayout.Space(5);
        if (GUILayout.Button("添加碰撞圆"))
        {
            AddCollider();
        }
        if (childcnt > 0)
        {
            for (int i = 0; i < childcnt; i++)
            {
                Transform child = collider.transform.FindChild(i.ToString());
                EditorGUILayout.BeginHorizontal();
                float scale = EditorGUILayout.FloatField(i.ToString() + "    缩放",child.localScale.x);
                child.localScale = new Vector3(scale, scale, scale);
                UpdateColliderPosition();
                if (i == childcnt-1 && GUILayout.Button("删除"))
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public void AddCollider()
    {
        FishCollider collider = (FishCollider)target;
        int childcnt = collider.transform.childCount;
        float radius = 50;
        float disFront = 0, disBack = 0, lastFrontCircleScale = 0, lastBackCircleScale = 0;
        for (int i = 0; i < childcnt; i++)
        {
            Transform child = collider.transform.FindChild(i.ToString());
            float scale = child.localScale.x;
            if (i % 2 != 0)
            {
                disFront += radius * scale;
            }
            else if (i % 2 == 0 && i != 0)
            {
                disBack -= radius * scale;
            }
            else
            {
                lastBackCircleScale = scale;
                lastFrontCircleScale = scale;
            }
        }

        Object assetObj = Resources.Load("ColliderCircle");
        GameObject colliderCircle = GameObject.Instantiate(assetObj) as GameObject;
        colliderCircle.transform.parent = collider.transform;
        colliderCircle.transform.localScale = Vector3.one;
        colliderCircle.transform.localPosition = Vector3.zero;
        colliderCircle.name = (childcnt).ToString();
        UIWidget widget = colliderCircle.GetComponent<UIWidget>();
        if (childcnt == 0)
        {
            widget.pivot = UIWidget.Pivot.Center;
        }
        else
        {
            if (childcnt % 2 != 0)
            {
                widget.pivot = UIWidget.Pivot.Left;
                colliderCircle.transform.localPosition = new Vector3(disFront, 0, 0);
            }
            else
            {
                widget.pivot = UIWidget.Pivot.Right;
                colliderCircle.transform.localPosition = new Vector3(disBack, 0, 0);
            }
        }
    }

    public void UpdateColliderPosition()
    {
        FishCollider collider = (FishCollider)target;
        int childcnt = collider.transform.childCount;
        float radius = 30;
        float disFront = 0, disBack = 0, lastFrontCircleScale = 0, lastBackCircleScale = 0;
        for (int i = 0; i < childcnt-1; i++)
        {
            Transform child = collider.transform.FindChild(i.ToString());
            float scale = child.localScale.x;
            if (i % 2 != 0)
            {
                disFront += radius * scale;
            }
            else if (i % 2 == 0 && i != 0)
            {
                disBack -= radius * scale;
            }
            else
            {
                lastBackCircleScale = scale;
                lastFrontCircleScale = scale;
            }
            Transform nextChild = collider.transform.FindChild((i+1).ToString());

            if ((i+1) % 2 != 0)
            {
                nextChild.localPosition = new Vector3(disFront, 0, 0);
            }
            else
            {
                nextChild.localPosition = new Vector3(disBack, 0, 0);
            }
        }
    }
}
