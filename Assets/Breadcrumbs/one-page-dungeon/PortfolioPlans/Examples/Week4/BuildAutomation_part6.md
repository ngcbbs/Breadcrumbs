# 빌드 자동화 스크립트 예제 - Part 6

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// 빌드 로그 생성
    /// </summary>
    private void GenerateBuildLog(string buildPath)
    {
        // 빌드 정보 수집
        Dictionary<string, string> buildInfo = new Dictionary<string, string>
        {
            { "ProductName", PlayerSettings.productName },
            { "Version", _version },
            { "BuildTarget", _buildTarget.ToString() },
            { "BuildDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "BuildNumber", GetBuildNumber().ToString() },
            { "ScriptingBackend", PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone).ToString() },
            { "UnityVersion", Application.unityVersion },
            { "Optimized", _enableOptimizations.ToString() }
        };
        
        // 로그 파일 생성
        string logPath = Path.Combine(buildPath, "build_info.txt");
        
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("========================");
            writer.WriteLine(" Dungeon Crawler Demo Build");
            writer.WriteLine("========================");
            writer.WriteLine();
            
            foreach (var kvp in buildInfo)
            {
                writer.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
            
            writer.WriteLine();
            writer.WriteLine("Included Scenes:");
            
            foreach (var scene in _scenesToBuild)
            {
                writer.WriteLine($"- {scene.path}");
            }
        }
        
        Debug.Log($"Build log generated at {logPath}");
    }
    
    /// <summary>
    /// 버전 정보 파일 생성
    /// </summary>
    private void GenerateVersionInfo(string buildPath)
    {
        string versionInfoPath = Path.Combine(buildPath, "version.json");
        
        // 버전 정보 객체 생성
        var versionInfo = new
        {
            version = _version,
            buildNumber = GetBuildNumber(),
            buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            unityVersion = Application.unityVersion
        };
        
        // JSON으로 변환
        string json = JsonUtility.ToJson(versionInfo, true);
        
        // 파일로 저장
        File.WriteAllText(versionInfoPath, json);
        
        Debug.Log($"Version info generated at {versionInfoPath}");
    }
    
    /// <summary>
    /// 인스톨러 생성
    /// </summary>
    private void CreateInstaller(string buildPath)
    {
        // 실제 인스톨러 생성은 외부 도구(Inno Setup, NSIS 등)를 사용해야 함
        // 여기서는 데모 목적으로 인스톨러 스크립트만 생성
        
        string innoSetupScript = Path.Combine(buildPath, "installer.iss");
        
        // Inno Setup 스크립트 생성
        using (StreamWriter writer = new StreamWriter(innoSetupScript))
        {
            writer.WriteLine("[Setup]");
            writer.WriteLine($"AppName=Dungeon Crawler Demo");
            writer.WriteLine($"AppVersion={_version}");
            writer.WriteLine("DefaultDirName={autopf}\\Dungeon Crawler Demo");
            writer.WriteLine("DefaultGroupName=Dungeon Crawler Demo");
            writer.WriteLine("UninstallDisplayIcon={app}\\DungeonCrawlerDemo.exe");
            writer.WriteLine("Compression=lzma2");
            writer.WriteLine("SolidCompression=yes");
            writer.WriteLine("OutputDir=.");
            writer.WriteLine($"OutputBaseFilename=DungeonCrawlerDemo_Setup_{_version}");
            writer.WriteLine();
            
            writer.WriteLine("[Files]");
            writer.WriteLine("Source: \"*\"; DestDir: \"{app}\"; Flags: ignoreversion recursesubdirs");
            writer.WriteLine();
            
            writer.WriteLine("[Icons]");
            writer.WriteLine("Name: \"{group}\\Dungeon Crawler Demo\"; Filename: \"{app}\\DungeonCrawlerDemo.exe\"");
            writer.WriteLine("Name: \"{group}\\Uninstall Dungeon Crawler Demo\"; Filename: \"{uninstallexe}\"");
        }
        
        Debug.Log($"Installer script generated at {innoSetupScript}");
        Debug.Log("Note: Actual installer creation requires Inno Setup to be installed");
    }
```
