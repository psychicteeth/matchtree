using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{        
    public void Start()
    {
    }

    public override void OnInspectorGUI()
    {
        LevelData levelData = (LevelData)target;
        serializedObject.Update();

        SerializedProperty levels = serializedObject.FindProperty("levels");

        EditorGUI.BeginChangeCheck();

        int toDelete = -1;
        int toMoveUp = -1;

        for (int i = 0; i < levels.arraySize; i++)
        {
            SerializedProperty level = levels.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Level " + (i + 1) + ":");
                // delete this level
                if (GUILayout.Button("X"))
                {
                    toDelete = i;
                }
                // move this level up
                if (i > 0 && GUILayout.Button("Move up"))
                {
                    toMoveUp = i;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            {
                // do fields for colour, leaves and width/height
                EditorGUILayout.BeginHorizontal();
                {
                    SerializedProperty numColors = level.FindPropertyRelative("numColors");
                    SerializedProperty numLeaves = level.FindPropertyRelative("numLeaves");

                    // for some reason Unity isn't displaying these as sliders
                    EditorGUILayout.IntSlider(numColors, 2, 5);
                    EditorGUILayout.IntSlider(numLeaves, 4, 9);
                }
                EditorGUILayout.EndHorizontal();


                SerializedProperty width = level.FindPropertyRelative("width");
                SerializedProperty height = level.FindPropertyRelative("height");

                EditorGUILayout.BeginHorizontal();
                {

                    // for some reason Unity isn't displaying these as sliders
                    EditorGUILayout.IntSlider(width, 5, Board.maxBoardWidth);
                    EditorGUILayout.IntSlider(height, 5, Board.maxBoardHeight);
                }
                EditorGUILayout.EndHorizontal();

                int w = width.intValue;
                int h = height.intValue;

                SerializedProperty map = level.FindPropertyRelative("map");
                for (int y = h - 1; y >= 0; y--)
                {
                    Rect r = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
                    for (int x = 0; x < w; x++)
                    {
                        // access the property in the good old 1D array -> 2D map format
                        SerializedProperty mapSquare = map.GetArrayElementAtIndex(x + y * Board.maxBoardWidth);

                        // checkboxes let you turn on and off a tile's existence in the level
                        mapSquare.boolValue = EditorGUI.Toggle(r, mapSquare.boolValue);

                        r.x += r.width;
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
        }

        if (GUILayout.Button("New"))
        {
            levelData.levels.Add(new LevelDescriptor());
        }

        if (toDelete != -1)
        { 
            levels.DeleteArrayElementAtIndex(toDelete);
        }

        if (toMoveUp != -1)
        {
            levels.MoveArrayElement(toMoveUp, toMoveUp - 1);
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
