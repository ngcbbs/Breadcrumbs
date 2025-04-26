# 빌드 자동화 스크립트 예제 - Part 9

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// 포트폴리오 프레젠테이션 준비
    /// </summary>
    private void PreparePortfolioPresentation()
    {
        // 프레젠테이션 폴더 생성
        string presentationPath = Path.Combine(_portfolioFolderPath, "Presentation");
        Directory.CreateDirectory(presentationPath);
        
        // 스크린샷 폴더 생성
        string screenshotsPath = Path.Combine(presentationPath, "Screenshots");
        Directory.CreateDirectory(screenshotsPath);
        
        // 비디오 폴더 생성
        string videosPath = Path.Combine(presentationPath, "Videos");
        Directory.CreateDirectory(videosPath);
        
        // README 파일 생성
        string readmePath = Path.Combine(presentationPath, "README.md");
        
        using (StreamWriter writer = new StreamWriter(readmePath))
        {
            writer.WriteLine("# Dungeon Crawler Demo - Portfolio Presentation");
            writer.WriteLine();
            writer.WriteLine("## Presentation Guide");
            writer.WriteLine("This folder contains resources for presenting the Dungeon Crawler Demo portfolio:");
            writer.WriteLine();
            writer.WriteLine("- **Screenshots**: Key visuals from the game");
            writer.WriteLine("- **Videos**: Demo gameplay videos");
            writer.WriteLine("- **Slides.pptx**: Presentation slides");
            writer.WriteLine();
            writer.WriteLine("## Demo Script");
            writer.WriteLine("1. Start with an overview of the project");
            writer.WriteLine("2. Showcase the procedural dungeon generation");
            writer.WriteLine("3. Demonstrate the core gameplay systems");
            writer.WriteLine("4. Show multiplayer functionality");
            writer.WriteLine("5. Discuss technical challenges and solutions");
            writer.WriteLine();
            writer.WriteLine("## Key Technical Points");
            writer.WriteLine("- Procedural generation algorithm");
            writer.WriteLine("- MagicOnion networking architecture");
            writer.WriteLine("- State management for game flow");
            writer.WriteLine("- Performance optimization techniques");
        }
        
        Debug.Log("Portfolio presentation materials prepared");
    }
    
    /// <summary>
    /// 빌드 대상에 맞는 실행 파일 확장자 가져오기
    /// </summary>
    private string GetExecutableExtension(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return ".exe";
                
            case BuildTarget.StandaloneOSX:
                return ".app";
                
            case BuildTarget.StandaloneLinux64:
                return "";
                
            default:
                return "";
        }
    }
}

/// <summary>
/// 버전 정보 스크립터블 오브젝트
/// </summary>
public class VersionInfo : ScriptableObject
{
    public string Version;
    public string BuildDate;
    public int BuildNumber;
}

/// <summary>
/// 미사용 에셋 확인을 위한 에디터 창
/// </summary>
public class UnusedAssetsWindow : EditorWindow
{
    private List<string> _unusedAssets = new List<string>();
    private Vector2 _scrollPosition;
    
    [MenuItem("Portfolio/Find Unused Assets")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetsWindow>("Unused Assets");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Unused Assets", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Find Unused"))
        {
            FindUnused();
        }
        
        EditorGUILayout.Space();
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        foreach (string asset in _unusedAssets)
        {
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(asset);
            
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(asset);
            }
            
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                AssetDatabase.DeleteAsset(asset);
                FindUnused();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    /// <summary>
    /// 미사용 에셋 찾기
    /// </summary>
    public void FindUnused()
    {
        _unusedAssets.Clear();
        
        // 모든 에셋 경로 가져오기
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        
        // 빌드에 포함된 에셋 가져오기
        string[] usedAssets = AssetDatabase.GetDependencies(EditorBuildSettings.scenes.Select(s => s.path).ToArray());
        
        // 사용되지 않는 에셋 찾기
        foreach (string asset in allAssets)
        {
            // 에디터 전용 폴더는 건너뛰기
            if (asset.StartsWith("Assets/Editor") || asset.Contains("/Editor/"))
                continue;
                
            // 메타 파일 무시
            if (asset.EndsWith(".meta"))
                continue;
                
            // 씬 파일 무시
            if (asset.EndsWith(".unity"))
                continue;
                
            // 스크립트 파일 무시
            if (asset.EndsWith(".cs"))
                continue;
                
            // 다른 에셋에 의해 사용되지 않으면 미사용 에셋으로 간주
            if (!usedAssets.Contains(asset) && asset.StartsWith("Assets/"))
            {
                _unusedAssets.Add(asset);
            }
        }
        
        Debug.Log($"Found {_unusedAssets.Count} unused assets");
    }
}
```
