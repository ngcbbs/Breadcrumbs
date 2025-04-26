# 빌드 자동화 스크립트 예제 - Part 7

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// ZIP 아카이브 생성
    /// </summary>
    private void CreateZipArchive(string buildPath)
    {
        // Unity에서는 기본적으로 ZIP 압축을 지원하지 않아 외부 도구나 라이브러리가 필요
        // 여기서는 System.IO.Compression 네임스페이스 사용을 가정 (추가 참조 필요)
        
        string zipFileName = $"{_buildName}_{_version}.zip";
        string zipFilePath = Path.Combine(_buildPath, zipFileName);
        
        Debug.Log($"Creating ZIP archive at {zipFilePath}...");
        
        // 실제 구현에서는 다음과 같은 코드 사용
        // System.IO.Compression.ZipFile.CreateFromDirectory(buildPath, zipFilePath);
        
        // 데모 목적으로 실제 압축 대신 메시지만 표시
        Debug.Log($"ZIP archive creation would compress {buildPath} to {zipFilePath}");
    }
    
    /// <summary>
    /// 포트폴리오 폴더로 복사
    /// </summary>
    private void CopyToPortfolioFolder(string buildPath)
    {
        // 포트폴리오 폴더 생성
        Directory.CreateDirectory(_portfolioFolderPath);
        
        // 빌드 파일 복사할 대상 폴더
        string destBuildPath = Path.Combine(_portfolioFolderPath, "Build");
        
        // 기존 폴더 삭제 (있는 경우)
        if (Directory.Exists(destBuildPath))
        {
            Directory.Delete(destBuildPath, true);
        }
        
        // 새 폴더 생성
        Directory.CreateDirectory(destBuildPath);
        
        Debug.Log($"Copying build to portfolio folder {destBuildPath}...");
        
        // 빌드 파일 복사
        CopyDirectory(buildPath, destBuildPath);
        
        // 포트폴리오 추가 파일 복사
        CopyPortfolioDocumentation(_portfolioFolderPath);
        
        Debug.Log("Portfolio package preparation completed");
    }
    
    /// <summary>
    /// 디렉토리 복사 유틸리티
    /// </summary>
    private void CopyDirectory(string sourceDir, string destDir)
    {
        // 하위 디렉토리 생성
        foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourceDir, destDir));
        }
        
        // 파일 복사
        foreach (string filePath in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
        {
            string newFilePath = filePath.Replace(sourceDir, destDir);
            File.Copy(filePath, newFilePath, true);
        }
    }
    
    /// <summary>
    /// 포트폴리오 문서 복사
    /// </summary>
    private void CopyPortfolioDocumentation(string portfolioFolderPath)
    {
        // 문서 폴더 생성
        string docsPath = Path.Combine(portfolioFolderPath, "Documentation");
        Directory.CreateDirectory(docsPath);
        
        // README 파일 생성
        string readmePath = Path.Combine(portfolioFolderPath, "README.md");
        
        using (StreamWriter writer = new StreamWriter(readmePath))
        {
            writer.WriteLine("# Dungeon Crawler Demo - Portfolio Project");
            writer.WriteLine();
            writer.WriteLine($"Version: {_version}");
            writer.WriteLine($"Build Date: {DateTime.Now.ToString("yyyy-MM-dd")}");
            writer.WriteLine();
            writer.WriteLine("## Overview");
            writer.WriteLine("This is a portfolio demo project of a procedural dungeon crawler game with multiplayer features.");
            writer.WriteLine("The project demonstrates procedural generation, MagicOnion networking, and core gameplay systems.");
            writer.WriteLine();
            writer.WriteLine("## Installation");
            writer.WriteLine("1. Extract the ZIP archive or run the installer");
            writer.WriteLine("2. Launch DungeonCrawlerDemo.exe");
            writer.WriteLine();
            writer.WriteLine("## Controls");
            writer.WriteLine("- WASD: Movement");
            writer.WriteLine("- Mouse: Look");
            writer.WriteLine("- Left Mouse Button: Attack");
            writer.WriteLine("- Right Mouse Button: Block/Aim");
            writer.WriteLine("- Shift: Sprint");
            writer.WriteLine("- Space: Jump");
            writer.WriteLine("- E: Interact");
            writer.WriteLine("- I: Inventory");
            writer.WriteLine("- ESC: Menu");
            writer.WriteLine();
            writer.WriteLine("## Documentation");
            writer.WriteLine("Please see the Documentation folder for detailed information about the project.");
        }
        
        // 스크린샷 폴더 생성
        string screenshotsPath = Path.Combine(portfolioFolderPath, "Screenshots");
        Directory.CreateDirectory(screenshotsPath);
        
        // 기술 문서 폴더로 복사
        string techDocsPath = "Documentation";
        if (Directory.Exists(techDocsPath))
        {
            CopyDirectory(techDocsPath, docsPath);
        }
    }
```
