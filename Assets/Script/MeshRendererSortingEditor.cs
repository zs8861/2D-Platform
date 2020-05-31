using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
/// This just exposes the Sorting Layer / Order in MeshRenderer since it's there
/// but not displayed in the inspector. Getting MeshRenderer to render in front
/// of a SpriteRenderer is pretty hard without this.
[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererSortingEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshRenderer renderer = target as MeshRenderer;


        var layers = SortingLayer.layers;

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        int newId = DrawSortingLayersPopup(renderer.sortingLayerID);
        if (EditorGUI.EndChangeCheck())
        {
            renderer.sortingLayerID = newId;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        int order = EditorGUILayout.IntField("Sorting Order", renderer.sortingOrder);
        if (EditorGUI.EndChangeCheck())
        {
            renderer.sortingOrder = order;
        }
        EditorGUILayout.EndHorizontal();

    }

    int DrawSortingLayersPopup(int layerID)
    {
        var layers = SortingLayer.layers;
        var names = layers.Select(l => l.name).ToArray();
        if (!SortingLayer.IsValid(layerID))
        {
            layerID = layers[0].id;
        }
        var layerValue = SortingLayer.GetLayerValueFromID(layerID);
        var newLayerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, names);
        return layers[newLayerValue].id;
    }

}

// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org/>

#endif