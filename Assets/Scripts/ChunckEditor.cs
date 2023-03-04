using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MC_Chunk))]
public class ChunckEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MC_Chunk chunk = (MC_Chunk)target;
        if(GUILayout.Button("Set vertecie"))
        {
            chunk.setVertexIsOnSurface();
        }
    }
}
