using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MC_Octree))]
public class ChunckEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MC_Octree chunk = (MC_Octree)target;
        if(GUILayout.Button("Set vertecie"))
        {
           // chunk.setVertexIsOnSurface();
        }
    }
}
