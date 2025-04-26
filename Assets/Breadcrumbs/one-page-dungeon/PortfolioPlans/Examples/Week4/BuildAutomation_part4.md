# 빌드 자동화 스크립트 예제 - Part 4

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// 빌드 파이프라인 실행
    /// </summary>
    public void ExecuteBuildPipeline()
    {
        Debug.Log($"Starting build pipeline for {_buildTarget}...");
        
        try
        {
            // 빌드 전 준비 작업
            PrepareBuildEnvironment();
            
            // 빌드 설정 적용
            ApplyBuildSettings();
            
            // 빌드 최적화 적용
            ApplyBuildOptimizations();
            
            // 씬 최적화
            OptimizeScenes();
            
            // 빌드 전 테스트 실행
            RunPreBuildTests();
            
            // 빌드 번호 가져오기
            int buildNumber = GetBuildNumber();
            
            // 최종 빌드 폴더 경로 생성
            string buildFolder = $"{_buildOutputPath}/{_productName}_v{_version}.{buildNumber}_{_buildTarget}";
            string executableName = $"{_productName}";
            
            // 빌드 타겟에 따라 확장자 추가
            if (_buildTarget == BuildTarget.StandaloneWindows || 
                _buildTarget == BuildTarget.StandaloneWindows64)
            {
                executableName += ".exe";
            }
            
            // 빌드 경로 생성
            Directory.CreateDirectory(buildFolder);
            string buildPath = Path.Combine(buildFolder, executableName);
            
            // 빌드 옵션 설정
            BuildOptions options = BuildOptions.None;
            
            // 개발 빌드 옵션 적용
            if (_isDevelopmentBuild)
            {
                options |= BuildOptions.Development;
                options |= BuildOptions.AllowDebugging;
            }
            
            // 로그 파일 옵션 적용
            if (_enableDetailedLogs)
            {
                options |= BuildOptions.DetailedBuildReport;
            }
            
            // 빌드 파라미터 설정
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = _scenesToBuild.Select(s => s.path).ToArray(),
                locationPathName = buildPath,
                target = _buildTarget,
                options = options
            };
            
            // 빌드 실행
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // 빌드 결과 처리
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build completed successfully in {summary.totalTime.TotalMinutes:F2} minutes");
                Debug.Log($"Build size: {summary.totalSize / 1048576.0f:F2} MB");
                
                // 빌드 후 작업 실행
                RunPostBuildTasks(Path.GetDirectoryName(buildPath));
                
                // 빌드 결과 창 표시
                ShowBuildSummary(summary, buildPath);
            }
            else
            {
                Debug.LogError($"Build failed with result: {summary.result}");
                
                // 빌드 에러 로그 표시
                foreach (var step in report.steps)
                {
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                        {
                            Debug.LogError($"Build error: {message.content}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Build process failed with exception: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            // 빌드 프로세스 완료 후 정리 작업
            CleanupBuildProcess();
        }
    }
    
    /// <summary>
    /// 빌드 결과 요약 표시
    /// </summary>
    private void ShowBuildSummary(BuildSummary summary, string buildPath)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== BUILD SUMMARY =====");
        sb.AppendLine($"Product: {_productName} v{_version}");
        sb.AppendLine($"Target Platform: {_buildTarget}");
        sb.AppendLine($"Build Time: {summary.totalTime.TotalMinutes:F2} minutes");
        sb.AppendLine($"Build Size: {summary.totalSize / 1048576.0f:F2} MB");
        sb.AppendLine($"Build Path: {buildPath}");
        sb.AppendLine($"Development Build: {_isDevelopmentBuild}");
        sb.AppendLine("=========================");
        
        Debug.Log(sb.ToString());
        
        // 빌드 정보 파일 생성
        string buildInfoPath = Path.Combine(Path.GetDirectoryName(buildPath), "BuildInfo.txt");
        File.WriteAllText(buildInfoPath, sb.ToString());
    }
```
