# 빌드 자동화 스크립트 예제 - Part 8

## BuildManager 클래스 (계속)

```csharp
    /// <summary>
    /// 포트폴리오 패키지 빌드
    /// </summary>
    private void BuildPortfolioPackage()
    {
        Debug.Log("Building portfolio package...");
        
        try
        {
            // 빌드 실행
            PerformBuild(false);
            
            // 포트폴리오 문서 정리
            OrganizePortfolioDocumentation();
            
            // 포트폴리오 프레젠테이션 준비
            PreparePortfolioPresentation();
            
            Debug.Log("Portfolio package build completed successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Portfolio package build failed: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 포트폴리오 문서 정리
    /// </summary>
    private void OrganizePortfolioDocumentation()
    {
        // 포트폴리오 제출 폴더 확인
        if (!Directory.Exists(_portfolioFolderPath))
        {
            Directory.CreateDirectory(_portfolioFolderPath);
        }
        
        // 문서 폴더 생성
        string docsPath = Path.Combine(_portfolioFolderPath, "Documentation");
        Directory.CreateDirectory(docsPath);
        
        // 문서 파일 생성
        CreatePortfolioDocumentationFiles(docsPath);
        
        // 소스 코드 샘플 준비
        PrepareCodeSamples(docsPath);
        
        Debug.Log("Portfolio documentation organized");
    }
    
    /// <summary>
    /// 포트폴리오 문서 파일 생성
    /// </summary>
    private void CreatePortfolioDocumentationFiles(string docsPath)
    {
        // 주요 문서 파일들
        Dictionary<string, string[]> documentFiles = new Dictionary<string, string[]>
        {
            { "Overview.md", new[] {
                "# Dungeon Crawler Demo - Overview",
                "",
                "## Project Description",
                "This project is a procedural dungeon crawler game with multiplayer features. It demonstrates",
                "various game development skills including procedural generation, networking, and more.",
                "",
                "## Key Features",
                "- Procedural dungeon generation",
                "- MagicOnion networking",
                "- Real-time combat system",
                "- Item and inventory management",
                "- Complete game loop implementation"
            }},
            
            { "TechnicalDesign.md", new[] {
                "# Technical Design Document",
                "",
                "## Architecture",
                "This project follows a modular architecture with the following key components:",
                "",
                "- Core Systems",
                "- Dungeon Generation",
                "- Gameplay",
                "- Networking",
                "- UI/UX",
                "",
                "## Technology Stack",
                "- Unity 2022.3 LTS",
                "- C#/.NET",
                "- MagicOnion for networking",
                "- MessagePack for serialization",
                "- UniTask for async programming"
            }},
            
            { "Development.md", new[] {
                "# Development Process",
                "",
                "## Methodology",
                "This project was developed over a 4-week period using an iterative approach:",
                "",
                "- Week 1: Foundation systems",
                "- Week 2: Core gameplay",
                "- Week 3: Networking and UI",
                "- Week 4: Integration and polishing",
                "",
                "## Challenges and Solutions",
                "During development, several challenges were encountered and addressed:",
                "",
                "1. Procedural generation balance",
                "2. Network synchronization",
                "3. Performance optimization"
            }}
        };
        
        // 문서 파일 생성
        foreach (var doc in documentFiles)
        {
            string filePath = Path.Combine(docsPath, doc.Key);
            File.WriteAllLines(filePath, doc.Value);
        }
    }
    
    /// <summary>
    /// 코드 샘플 준비
    /// </summary>
    private void PrepareCodeSamples(string docsPath)
    {
        // 코드 샘플 폴더 생성
        string codePath = Path.Combine(docsPath, "CodeSamples");
        Directory.CreateDirectory(codePath);
        
        // 주요 시스템 코드 파일 복사
        string[] systemPaths = {
            "Assets/Scripts/Dungeon",
            "Assets/Scripts/Gameplay",
            "Assets/Scripts/Network",
            "Assets/Scripts/UI"
        };
        
        foreach (string systemPath in systemPaths)
        {
            if (Directory.Exists(systemPath))
            {
                string destPath = Path.Combine(codePath, Path.GetFileName(systemPath));
                Directory.CreateDirectory(destPath);
                
                // C# 파일만 복사 (예시)
                foreach (string filePath in Directory.GetFiles(systemPath, "*.cs"))
                {
                    string fileName = Path.GetFileName(filePath);
                    File.Copy(filePath, Path.Combine(destPath, fileName), true);
                }
            }
        }
    }
```
