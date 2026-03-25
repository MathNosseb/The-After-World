using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetEditor : Editor {

    Planet planet;
    Editor shapeEditor;
    Editor colourEditor;

	public override void OnInspectorGUI()
	{
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
        }

        if (GUILayout.Button("Generate Planet"))
        {
            planet.InitNoise();
            planet.Generate();
        }

        DrawSettingsEditor(planet.shape, planet.Generate, ref planet.shapeFoldout, ref shapeEditor);
        DrawSettingsEditor(planet.shading, planet.Generate, ref planet.shadingFoldout, ref colourEditor);
	}

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

	private void OnEnable()
	{
        planet = (Planet)target;
	}
}
