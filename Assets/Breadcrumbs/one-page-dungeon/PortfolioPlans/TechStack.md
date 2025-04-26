# 기술 스택 및 프로젝트 구조

## 개발 환경

### 엔진 및 주요 도구
- **게임 엔진**: Unity 2022.3 LTS
- **IDE**: Visual Studio 2022 / JetBrains Rider
- **버전 관리**: Git (GitHub/GitLab)
- **문서화**: Markdown / Mermaid 다이어그램

### 프로그래밍 언어 및 프레임워크
- **주요 언어**: C# (.NET 6.0)
- **스크립팅**: C# 스크립팅
- **구성 형식**: JSON, ScriptableObjects

## 핵심 기술 요소

### MagicOnion 네트워크 프레임워크
- **버전**: 최신 안정 버전의 MagicOnion 프레임워크
- **구성 요소**:
  - MagicOnion.Server: 서버 측 구현
  - MagicOnion.Client: 클라이언트 측 구현
  - MagicOnion.Unity: Unity 클라이언트 통합
- **통신 패턴**:
  - Service 패턴: 단방향 요청-응답 RPC
  - Hub 패턴: 양방향 실시간 통신
- **직렬화**: MessagePack

### 프로시저럴 생성 시스템
- **알고리즘**: BSP(Binary Space Partitioning) 및 셀룰러 오토마타
- **데이터 구조**: 타일 기반 그리드 시스템
- **룸 생성**: 기본 파라미터화된 룸 템플릿
- **던전 연결**: 경로 생성 및 노드 연결 알고리즘

### Unity 핵심 기능
- **렌더링**: URP(Universal Render Pipeline)
- **물리 엔진**: Unity 기본 물리 시스템
- **애니메이션**: Unity Mecanim
- **UI 시스템**: Unity UI (UGUI)
- **오디오**: Unity 기본 오디오 시스템

## 프로젝트 구조

### 폴더 구조
```
Assets/
├── Art/
│   ├── Materials/
│   ├── Models/
│   ├── Textures/
│   ├── Animations/
│   └── UI/
├── Audio/
│   ├── Music/
│   └── SFX/
├── Code/
│   ├── Scripts/
│   │   ├── Core/
│   │   ├── Gameplay/
│   │   ├── DungeonGeneration/
│   │   ├── AI/
│   │   ├── UI/
│   │   ├── Network/
│   │   └── Utils/
│   └── Plugins/
│       └── MagicOnion/
├── Data/
│   ├── ScriptableObjects/
│   ├── Prefabs/
│   └── Configs/
├── Resources/
├── Scenes/
└── StreamingAssets/
```

### 네임스페이스 구조
```csharp
namespace DungeonCrawler
{
    namespace Core { /* 코어 게임 시스템 */ }
    namespace Gameplay { /* 게임플레이 관련 시스템 */ }
    namespace DungeonGeneration { /* 던전 생성 알고리즘 */ }
    namespace AI { /* 인공지능 및 NPC 행동 */ }
    namespace UI { /* 사용자 인터페이스 요소 */ }
    namespace Network { /* MagicOnion 관련 네트워크 코드 */ }
    namespace Utils { /* 유틸리티 및 헬퍼 클래스 */ }
}
```

## 아키텍처 패턴

### 데이터 관리
- **ScriptableObject 기반 데이터**: 게임 데이터 및 구성 저장
- **JSON 구성**: 런타임 외부 구성 및 세팅
- **로컬 저장소**: PlayerPrefs 및 JSON 파일 기반 로컬 스토리지

### 디자인 패턴
- **서비스 로케이터**: 시스템 및 서비스 접근
- **커맨드 패턴**: 입력 처리 및 게임 명령 실행
- **상태 패턴**: 캐릭터 및 적 행동 관리
- **팩토리 패턴**: 오브젝트 및 시스템 동적 생성
- **옵저버 패턴**: 이벤트 기반 시스템 통신

### 네트워크 아키텍처
- **클라이언트-서버 모델**: MagicOnion 기반 통신
- **상태 동기화**: 주요 게임 오브젝트 및 이벤트 동기화
- **권한 모델**: 서버 권한 및 클라이언트 예측
- **영역 관리**: 관심 영역(AOI) 기반 네트워크 최적화

## 중요 구현 고려사항

### 성능 최적화
- **오브젝트 풀링**: 자주 생성/파괴되는 요소 재사용
- **LOD(Level of Detail)**: 거리 기반 세부 수준 조정
- **배치 처리**: 유사 렌더링 요소 배치 처리
- **비동기 로딩**: 백그라운드 리소스 로딩

### 확장성
- **모듈식 설계**: 독립적으로 작동 가능한 시스템 모듈
- **인터페이스 기반 프로그래밍**: 구현 세부사항 추상화
- **이벤트 기반 통신**: 시스템 간 직접 의존성 최소화
- **구성 중심 설계**: 하드코딩보다 구성 기반 동작

### 테스트 계획
- **유닛 테스트**: 핵심 알고리즘 및 비즈니스 로직 테스트
- **통합 테스트**: 시스템 간 상호작용 테스트
- **성능 프로파일링**: 병목 현상 및 최적화 기회 식별
- **네트워크 시뮬레이션**: 다양한 네트워크 조건 테스트

## 포트폴리오 구현 범위

1개월의 제한된 시간 내에서 다음과 같은 범위로 기술 스택을 구현할 예정입니다:

1. **MagicOnion 기본 프레임워크**: 핵심 통신 패턴 구현
2. **간소화된 프로시저럴 생성**: 기본 BSP 알고리즘 구현
3. **최소 게임플레이 시스템**: 이동, 전투, 아이템 기본 기능
4. **필수 UI 요소**: HUD, 인벤토리, 메뉴 핵심 기능
5. **성능 최적화**: 기본적인 오브젝트 풀링 및 최적화 기법

[메인 계획으로 돌아가기](./MasterPlan.md)
