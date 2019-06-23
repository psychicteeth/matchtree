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
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 80;
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
                    
                    EditorGUILayout.IntSlider(numColors, 2, 5);
                    EditorGUILayout.IntSlider(numLeaves, 4, 9);
                }
                EditorGUILayout.EndHorizontal();

                SerializedProperty width = level.FindPropertyRelative("width");
                SerializedProperty height = level.FindPropertyRelative("height");

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.IntSlider(width, 5, Board.maxBoardWidth);
                    EditorGUILayout.IntSlider(height, 5, Board.maxBoardHeight);
                }
                EditorGUILayout.EndHorizontal();

                SerializedProperty goals = level.FindPropertyRelative("goals");

                EditorGUILayout.BeginVertical();
                {
                    int removeGoal = -1;
                    for (int j = 0; j < goals.arraySize; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        SerializedProperty goal = goals.GetArrayElementAtIndex(j);
                        SerializedProperty type = goal.FindPropertyRelative("type");

                        EditorGUILayout.PropertyField(type);
                        switch (type.enumValueIndex)
                        {
                            case 0:
                                {
                                    // score only
                                    SerializedProperty score = goal.FindPropertyRelative("scoreLimit");
                                    EditorGUILayout.PropertyField(score);
                                }
                                break;

                            case 1:
                                {
                                    // score and time
                                    SerializedProperty score = goal.FindPropertyRelative("scoreLimit");
                                    EditorGUILayout.PropertyField(score);
                                    SerializedProperty time = goal.FindPropertyRelative("timeLimit");
                                    EditorGUILayout.PropertyField(time);

                                }
                                break;

                            default:
                                break;
                        }
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            removeGoal = i;
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    if (removeGoal != -1)
                    {
                        levelData.levels[i].goals.RemoveAt(removeGoal);
                    }
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("New goal"))
                {
                    levelData.levels[i].goals.Add(new Goal());
                }

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

        EditorGUIUtility.labelWidth = labelWidth;
    }
}
