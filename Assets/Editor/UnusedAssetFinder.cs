using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class UnusedAssetCleaner : EditorWindow
{
    // Stores info for each unused asset
    private class AssetInfo
    {
        public string path;
        public long size;
        public string extension => Path.GetExtension(path).ToLowerInvariant();
    }

    private enum SortType // Enumerator for the different types of sorting we can use
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
    private List<AssetInfo> filteredAssets = new List<AssetInfo>();
    private SortType currentSort = SortType.NameAsc;
    private HashSet<string> highlightedPaths = new HashSet<string>();// Tracks highlighted asset paths
    private const int rowHeight = 22; // Approximate row height for scrolling
    private string searchQuery = ""; // Current search query
    private bool allSelected => filteredAssets.Count > 0 && highlightedPaths.Count == filteredAssets.Count;

    [MenuItem("Tools/Unused Asset Cleaner")]
    public static void OpenWindow() // Opens a window for the tool
    {
        GetWindow<UnusedAssetCleaner>("Unused Asset Cleaner");
    }

    private void OnGUI() // Displays the results
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Scan for Unused Assets", GUILayout.Height(30), GUILayout.Width(200)))
        {
            ScanForUnusedAssets();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(allSelected ? "Deselect All" : "Select All", GUILayout.Height(30), GUILayout.Width(120)))
        {
            if (allSelected)
                highlightedPaths.Clear();
            else
                highlightedPaths = filteredAssets.Select(a => a.path).ToHashSet();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Search bar field
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Search:", GUILayout.Width(60));
        string newQuery = EditorGUILayout.TextField(searchQuery);
        if (newQuery != searchQuery)
        {
            searchQuery = newQuery;
            ApplySearchFilter();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (filteredAssets.Count > 0)
        {
            // Sorting and delete all buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Sort by:", GUILayout.Width(60));
            currentSort = (SortType)EditorGUILayout.EnumPopup(currentSort);
            if (GUILayout.Button("Sort", GUILayout.Width(80)))
            {
                SortAssets();
                ApplySearchFilter();
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Delete All Highlighted", GUILayout.Width(160)))
            {
                if (EditorUtility.DisplayDialog("Delete Highlighted", "Delete ALL highlighted assets?", "Yes", "No"))
                {
                    var toDelete = unusedAssets.Where(a => highlightedPaths.Contains(a.path)).ToList();
                    foreach (var asset in toDelete)
                    {
                        AssetDatabase.DeleteAsset(asset.path);
                        unusedAssets.Remove(asset);
                    }
                    AssetDatabase.Refresh();
                    highlightedPaths.Clear();
                    ApplySearchFilter();
                    return;
                }
            }

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
                    highlightedPaths.Clear();
                    ApplySearchFilter();
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            int totalCount = filteredAssets.Count;
            int viewHeight = Mathf.FloorToInt(position.height) - 160;
            int visibleCount = Mathf.FloorToInt(viewHeight / rowHeight);
            int scrollOffset = Mathf.FloorToInt(scrollPos.y / rowHeight);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(viewHeight));
            GUILayout.Space(totalCount * rowHeight); // Reserve scroll space

            for (int i = scrollOffset; i < Mathf.Min(totalCount, scrollOffset + visibleCount + 5); i++)
            {
                Rect rowRect = new Rect(0, i * rowHeight, position.width - 20, rowHeight);
                DrawRow(filteredAssets[i], rowRect);
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            GUILayout.Label("No matching unused assets found.");
        }
    }

    private void DrawRow(AssetInfo asset, Rect rect)// Draws a single row inside the scroll view for one asset
    {
        Color originalColor = GUI.backgroundColor;
        if (highlightedPaths.Contains(asset.path))
        {
            GUI.backgroundColor = new Color(1f, 0.0f, 0.0f); // Red highlight
        }

        GUILayout.BeginArea(rect, GUI.skin.box);
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(asset.extension, GUILayout.Width(50));
        GUILayout.Label(Path.GetFileName(asset.path), GUILayout.Width(200));
        GUILayout.Label(asset.path, GUILayout.Width(300));
        GUILayout.Label(FormatSize(asset.size), GUILayout.Width(80));

        if (GUILayout.Button("Find in Editor", GUILayout.Width(100))) // Only pings in editor
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(asset.path);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        if (GUILayout.Button(highlightedPaths.Contains(asset.path) ? "Deselect" : "Select", GUILayout.Width(70))) // Purely for highlight & selection
        {
            if (highlightedPaths.Contains(asset.path))
                highlightedPaths.Remove(asset.path);
            else
                highlightedPaths.Add(asset.path);
        }

        if (GUILayout.Button("Delete", GUILayout.Width(60)))
        {
            if (EditorUtility.DisplayDialog("Delete Asset", $"Are you sure you want to delete {asset.path}?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(asset.path);
                AssetDatabase.Refresh();
                unusedAssets.Remove(asset);
                highlightedPaths.Remove(asset.path);
                ApplySearchFilter();
            }
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();
        GUI.backgroundColor = originalColor;
    }

    private void ScanForUnusedAssets()// Scan method that searches unused assets based on the scenes selected in EditorBuildSettings
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets") && !AssetDatabase.IsValidFolder(p))
            .ToArray();

        string[] usedAssets = AssetDatabase.GetDependencies(
            EditorBuildSettings.scenes.Select(s => s.path).ToArray(), true);

        HashSet<string> usedSet = new HashSet<string>(usedAssets);

        unusedAssets = allAssetPaths
            .Where(p => !usedSet.Contains(p) && !p.EndsWith(".cs") && !p.EndsWith(".meta"))
            .Select(p => new AssetInfo
            {
                path = p,
                size = new FileInfo(p).Length
            }).ToList();

        SortAssets();
        ApplySearchFilter();
        highlightedPaths.Clear(); // Reset selection
    }

    private void ApplySearchFilter()// Updates filtered list based on search query
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            filteredAssets = unusedAssets;
        }
        else
        {
            string lower = searchQuery.ToLowerInvariant();
            filteredAssets = unusedAssets
                .Where(a => Path.GetFileName(a.path).ToLowerInvariant().Contains(lower)
                         || a.path.ToLowerInvariant().Contains(lower))
                .ToList();
        }
    }

    private void SortAssets()// Sorts the list of unused assets based on selected sort option
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
    private string FormatSize(long sizeInBytes)// Format and display file size in readable units
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