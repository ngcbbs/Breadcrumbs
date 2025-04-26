# 빌드 자동화 스크립트 예제 - Part 2

## BuildManager 클래스 (계속)

```csharp
    private void DrawBuildTaskSettings()
    {
        GUILayout.Label("Build Tasks", EditorStyles.boldLabel);
        
        _runPreBuildTasks = EditorGUILayout.Toggle("Run Pre-Build Tasks", _runPreBuildTasks);
        _runPostBuildTasks = EditorGUILayout.Toggle("Run Post-Build Tasks", _runPostBuildTasks);
        _generateVersionInfo = EditorGUILayout.Toggle("Generate Version Info", _generateVersionInfo);
        _createInstaller = EditorGUILayout.Toggle("Create Installer", _createInstaller);
    }
    
    private void DrawDeploymentSettings()
    {
        GUILayout.Label("Deployment Settings", EditorStyles.boldLabel);
        
        _createZipArchive = EditorGUILayout.Toggle("Create ZIP Archive", _createZipArchive);
        _copyToPortfolioFolder = EditorGUILayout.Toggle("Copy to Portfolio Folder", _copyToPortfolioFolder);
        
        if (_copyToPortfolioFolder)
        {
            _portfolioFolderPath = EditorGUILayout.TextField("Portfolio Folder", _portfolioFolderPath);
        }
    }
    
    private void DrawBuildButtons()
    {
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Build Demo"))
        {
            PerformBuild(false);
        }
        
        if (GUILayout.Button("Build & Run"))
        {
            PerformBuild(true);
        }
        
        if (GUILayout.Button("Build Portfolio Package"))
        {
            BuildPortfolioPackage();
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void LoadDefaultScenes()
    {
        _scenesToBuild.Clear();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            _scenesToBuild.Add(scene);
        }
    }
    
    /// <summary>
    /// 빌드 수행
    /// </summary>
    /// <param name="autoRun">빌드 후 자동 실행 여부</param>
    private void PerformBuild(bool autoRun)
    {
        try
        {
            // 빌드 경로 생성
            string fullBuildPath = Path.Combine(_buildPath, _buildName);
            Directory.CreateDirectory(_buildPath);
            
            // 빌드 옵션 설정
            BuildOptions options = _buildOptions;
            if (autoRun)
            {
                options |= BuildOptions.AutoRunPlayer;
            }
            
            // 빌드 전 작업
            if (_runPreBuildTasks)
            {
                RunPreBuildTasks();
            }
            
            // 빌드 최적화 설정 적용
            if (_enableOptimizations)
            {
                ApplyBuildOptimizations();
            }
            
            // 빌드 실행
            string exeName = _buildName + GetExecutableExtension(_buildTarget);
            string fullExePath = Path.Combine(fullBuildPath, exeName);
            
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = _scenesToBuild.Select(s => s.path).ToArray(),
                locationPathName = fullExePath,
                target = _buildTarget,
                options = options
            };
            
            Debug.Log($"Starting build for {_buildName} v{_version}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build completed successfully in {summary.totalTime.TotalSeconds:F2} seconds");
                Debug.Log($"Build size: {summary.totalSize / 1048576:F2} MB");
                
                // 빌드 후 작업
                if (_runPostBuildTasks)
                {
                    RunPostBuildTasks(fullBuildPath);
                }
                
                // 버전 정보 생성
                if (_generateVersionInfo)
                {
                    GenerateVersionInfo(fullBuildPath);
                }
                
                // 인스톨러 생성
                if (_createInstaller)
                {
                    CreateInstaller(fullBuildPath);
                }
                
                // 배포 작업
                if (_createZipArchive)
                {
                    CreateZipArchive(fullBuildPath);
                }
                
                // 포트폴리오 폴더로 복사
                if (_copyToPortfolioFolder)
                {
                    CopyToPortfolioFolder(fullBuildPath);
                }
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"Build failed with {summary.totalErrors} errors");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Build failed with exception: {ex.Message}\n{ex.StackTrace}");
        }
    }
```
