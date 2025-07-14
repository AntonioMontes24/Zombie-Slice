using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class UnusedAssetCleaner : EditorWindow
{
    private class AssetInfo
    {
        public string path;
        public long size;
        public string extension => Path.GetExtension(path).ToLowerInvariant();
    }

    private enum SortType//Enumerator for the different types of selection we can use
    {
        NameAsc,
        NameDesc,
        PathAsc,
        PathDesc,
        SizeAsc,
        SizeDesc,
        TypeAsc,
        TypeDesc
    }

    private Vector2 scrollPos;
    private List<AssetInfo> unusedAssets = new List<AssetInfo>();
    private SortType currentSort = SortType.NameAsc;

    [MenuItem("Tools/Unused Asset Cleaner")]
    public static void OpenWindow()//Opens a window for the tool 
    {
        GetWindow<UnusedAssetCleaner>("Unused Asset Cleaner");
    }

    private void OnGUI()//Displays the results 
    {
        GUILayout.Space(10);
        if (GUILayout.Button("Scan for Unused Assets", GUILayout.Height(30)))
        {
            ScanForUnusedAssets();
        }

        GUILayout.Space(10);

        if (unusedAssets.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Sort by:", GUILayout.Width(60));
            currentSort = (SortType)EditorGUILayout.EnumPopup(currentSort);
            if (GUILayout.Button("Sort", GUILayout.Width(80)))
            {
                SortAssets();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete All", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Delete All", "Are you sure you want to delete ALL unused assets?", "Yes", "No"))
                {
                    foreach (var asset in unusedAssets)
                    {
                        AssetDatabase.DeleteAsset(asset.path);
                    }
                    AssetDatabase.Refresh();
                    unusedAssets.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (var asset in unusedAssets)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(asset.extension, GUILayout.Width(50));
                GUILayout.Label(Path.GetFileName(asset.path), GUILayout.Width(200));
                GUILayout.Label(asset.path, GUILayout.Width(300));
                GUILayout.Label(FormatSize(asset.size), GUILayout.Width(80));

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(asset.path);
                    Selection.activeObject = obj;
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Asset", $"Are you sure you want to delete {asset.path}?", "Yes", "No"))
                    {
                        AssetDatabase.DeleteAsset(asset.path);
                        AssetDatabase.Refresh();
                        unusedAssets.Remove(asset);
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("No unused assets found.");
        }
    }

    private void ScanForUnusedAssets() // Scan method that searches unsued assets based on the scenes selected on the EdtitorBuildScene
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets") && !AssetDatabase.IsValidFolder(p))
            .ToArray();

        string[] usedAssets = AssetDatabase.GetDependencies(EditorBuildSettings.scenes.Select(s => s.path).ToArray(), true);

        HashSet<string> usedSet = new HashSet<string>(usedAssets);
        unusedAssets = allAssetPaths
            .Where(p => !usedSet.Contains(p) && !p.EndsWith(".cs"))
            .Select(p => new AssetInfo
            {
                path = p,
                size = new FileInfo(p).Length
            }).ToList();

        SortAssets();
    }

    private void SortAssets()// swicthc case depeneding on selection 
    {
        switch (currentSort)
        {
            case SortType.NameAsc:
                unusedAssets = unusedAssets.OrderBy(a => Path.GetFileName(a.path)).ToList();
                break;
            case SortType.NameDesc:
                unusedAssets = unusedAssets.OrderByDescending(a => Path.GetFileName(a.path)).ToList();
                break;
            case SortType.PathAsc:
                unusedAssets = unusedAssets.OrderBy(a => a.path).ToList();
                break;
            case SortType.PathDesc:
                unusedAssets = unusedAssets.OrderByDescending(a => a.path).ToList();
                break;
            case SortType.SizeAsc:
                unusedAssets = unusedAssets.OrderBy(a => a.size).ToList();
                break;
            case SortType.SizeDesc:
                unusedAssets = unusedAssets.OrderByDescending(a => a.size).ToList();
                break;
            case SortType.TypeAsc:
                unusedAssets = unusedAssets.OrderBy(a => a.extension).ThenBy(a => a.path).ToList();
                break;
            case SortType.TypeDesc:
                unusedAssets = unusedAssets.OrderByDescending(a => a.extension).ThenBy(a => a.path).ToList();
                break;
        }
    }

    private string FormatSize(long sizeInBytes)//Format and display correct size
    {
        float size = sizeInBytes;
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
