#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class RecompileOneScript : MonoBehaviour
{
    [MenuItem("Tools/Reimport Selected Script (Atmosphere Reload)")]
    static void ReimportSelected()
    {
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (path.EndsWith(".cs"))
            {
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log("Recompiled: " + path);
            }
        }
    }
}
#endif
