# 빌드 자동화 스크립트 예제 - Part 5

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// 빌드 번호 가져오기
    /// </summary>
    private int GetBuildNumber()
    {
        // 빌드 번호는 일반적으로 CI/CD 시스템에서 제공하지만,
        // 데모 목적으로 증가 카운터를 사용
        const string buildNumberPath = "ProjectSettings/BuildNumber.txt";
        
        int buildNumber = 1;
        
        if (File.Exists(buildNumberPath))
        {
            string content = File.ReadAllText(buildNumberPath);
            if (int.TryParse(content, out int storedBuildNumber))
            {
                buildNumber = storedBuildNumber + 1;
            }
        }
        
        // 새 빌드 번호 저장
        File.WriteAllText(buildNumberPath, buildNumber.ToString());
        
        return buildNumber;
    }
    
    /// <summary>
    /// 미사용 리소스 정리
    /// </summary>
    private void CleanupUnusedResources()
    {
        // 에디터에서 제공하는 기능 활용
        EditorWindow.GetWindow<UnusedAssetsWindow>().FindUnused();
        
        Debug.Log("Unused resources cleanup completed");
    }
    
    /// <summary>
    /// 씬 최적화
    /// </summary>
    private void OptimizeScenes()
    {
        // 빌드에 포함된 씬들에 대해 최적화 실행
        foreach (var scene in _scenesToBuild)
        {
            // 씬 로드
            EditorSceneManager.OpenScene(scene.path);
            
            // 정적 배칭 그룹화
            StaticBatchingUtility.Combine(EditorSceneManager.GetActiveScene().GetRootGameObjects());
            
            // 변경사항 저장
            EditorSceneManager.SaveOpenScenes();
        }
        
        Debug.Log("Scene optimization completed");
    }
    
    /// <summary>
    /// 빌드 전 테스트 실행
    /// </summary>
    private void RunPreBuildTests()
    {
        // 에디터 테스트 러너 실행
        // 실제 구현에서는 Test Runner API를 활용해 테스트 수행
        
        Debug.Log("Pre-build tests completed");
    }
    
    /// <summary>
    /// 빌드 후 작업 실행
    /// </summary>
    private void RunPostBuildTasks(string buildPath)
    {
        Debug.Log("Running post-build tasks...");
        
        // 불필요한 파일 정리
        CleanupBuildDirectory(buildPath);
        
        // 빌드 로그 생성
        GenerateBuildLog(buildPath);
        
        // 빌드 후 테스트 (선택 사항)
        // RunPostBuildTests(buildPath);
    }
    
    /// <summary>
    /// 빌드 디렉토리 정리
    /// </summary>
    private void CleanupBuildDirectory(string buildPath)
    {
        // 불필요한 파일 목록
        string[] unnecessaryFiles = {
            "UnityPlayer.dll.config",
            "WinPixEventRuntime.dll"
        };
        
        foreach (string file in unnecessaryFiles)
        {
            string filePath = Path.Combine(buildPath, file);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        // UnityCrashHandler 숨김 처리
        string crashHandlerPath = Path.Combine(buildPath, "UnityCrashHandler64.exe");
        if (File.Exists(crashHandlerPath))
        {
            File.SetAttributes(crashHandlerPath, FileAttributes.Hidden);
        }
        
        Debug.Log("Build directory cleanup completed");
    }
```
