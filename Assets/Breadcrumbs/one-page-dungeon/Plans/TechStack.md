# 기술 스택

## 엔진 및 개발 환경

### Unity 엔진
- **버전**: Unity 2022.3 LTS 또는 최신 안정 버전
- **렌더링**: URP(Universal Render Pipeline) 또는 HDRP(High Definition Render Pipeline)
- **물리 엔진**: Unity Physics 또는 Havok Physics
- **스크립팅 API**: Unity C# API
- **애셋 관리**: 어드레서블 시스템 및 애셋 번들
- **원격 구성**: Unity Cloud 및 Game Services

### 개발 환경
- **IDE**: Visual Studio 2022 또는 JetBrains Rider
- **버전 관리**: Git (GitHub/GitLab)
- **이슈 추적**: Jira 또는 GitHub Issues
- **문서화**: Confluence 또는 GitBook
- **CI/CD**: Jenkins, GitHub Actions, 또는 GitLab CI
- **코드 품질**: SonarQube, NDepend 또는 ReSharper

## 프로그래밍 언어 및 프레임워크

### 프로그래밍 언어
- **메인 언어**: C# (.NET 6.0 이상)
- **스크립팅**: C# 스크립팅 및 필요 시 Lua 또는 Python 지원
- **쿼리 언어**: SQL (데이터베이스 쿼리)
- **마크업**: JSON, XML, YAML (구성 및 데이터)

### 프레임워크 및 라이브러리
- **게임 프레임워크**: Unity Engine 내장 프레임워크
- **UI 시스템**: Unity UI 또는 UI Toolkit
- **DI(의존성 주입)**: Zenject 또는 VContainer
- **데이터 관리**: ScriptableObjects 기반 아키텍처
- **상태 관리**: 자체 개발 상태 머신 또는 NodeCanvas
- **멀티스레딩**: Unity Job System 및 .NET Task Parallel Library

## 네트워크 및 백엔드

### MagicOnion
- **버전**: 최신 안정 버전의 MagicOnion 프레임워크
- **구성 요소**:
  - **MagicOnion.Server**: 서버 측 구현
  - **MagicOnion.Client**: 클라이언트 측 구현
  - **MagicOnion.Unity**: Unity 클라이언트 통합
  - **MagicOnion.Hosting**: 서버 호스팅 및 구성
  - **MagicOnion.MSBuild.Tasks**: 코드 생성 및 자동화
- **통신 패턴**:
  - **Service 패턴**: 단방향 요청-응답 RPC
  - **Hub 패턴**: 양방향 실시간 통신
  - **Stream 패턴**: 대용량 데이터 스트리밍

### 직렬화 및 네트워크 최적화
- **MessagePack**: 고성능 바이너리 직렬화
  - MessagePack for C#
  - MessagePack.UnityShims
  - 커스텀 포맷터 및 리졸버
- **압축 기술**: zlib 또는 LZ4 기반 압축
- **네트워크 최적화**: 델타 압축 및 관심 필터링

### 서버 기술
- **서버 플랫폼**: .NET 6.0+ / ASP.NET Core
- **운영 체제**: Linux (Ubuntu Server 또는 Alpine)
- **컨테이너화**: Docker 및 Docker Compose
- **오케스트레이션**: Kubernetes (AWS EKS 또는 Azure AKS)
- **로드 밸런싱**: 클라우드 로드 밸런서 또는 NGINX
- **서비스 메시**: Istio 또는 Linkerd (선택적)

### 데이터베이스 및 저장소
- **메인 데이터베이스**: PostgreSQL 또는 MongoDB
  - Entity Framework Core 또는 Dapper (SQL)
  - MongoDB C# Driver (NoSQL)
- **캐시 시스템**: Redis
  - StackExchange.Redis 클라이언트
  - 분산 캐싱 및 세션 관리
- **파일 스토리지**: 클라우드 스토리지 (AWS S3 또는 Azure Blob)
  - 애셋 및 사용자 생성 콘텐츠
  - CDN 통합

## 백엔드 서비스

### 인증 및 사용자 관리
- **인증 시스템**: 자체 개발 또는 Identity Server
- **소셜 로그인**: OAuth 2.0 / OpenID Connect 통합
- **세션 관리**: JWT 토큰 또는 세션 기반 인증
- **보안**: HTTPS, 데이터 암호화, 2FA

### 분석 및 모니터링
- **로깅**: Serilog 또는 NLog
  - ELK 스택 또는 Graylog 통합
  - 구조화된 로깅 및 쿼리
- **모니터링**: Prometheus + Grafana
  - 시스템 및 서비스 건강 상태
  - 커스텀 게임 메트릭
- **APM**: Application Insights 또는 New Relic
  - 트레이싱 및 성능 분석
  - 예외 및 오류 추적

### 빌드 및 배포
- **빌드 자동화**: Jenkins, GitHub Actions, 또는 Azure DevOps
  - Unity Cloud Build 통합
  - 멀티플랫폼 빌드 파이프라인
- **배포 전략**: Blue-Green 또는 Canary 배포
  - 롤백 메커니즘
  - 환경별 구성 관리
- **인프라 자동화**: Terraform 또는 AWS CloudFormation
  - 인프라 as 코드
  - 환경 일관성 유지

## 그래픽 및 시각적 효과

### 렌더링 기술
- **셰이더**: Unity Shader Graph 또는 커스텀 HLSL/GLSL 셰이더
  - PBR(Physically-Based Rendering)
  - 스타일라이즈드 렌더링 (필요 시)
- **라이팅**: 실시간 및 베이크된 라이팅
  - 글로벌 일루미네이션
  - HDR 및 블룸 효과
- **후처리**: 포스트 프로세싱 스택
  - 색상 보정, 앰비언트 오클루전, 피사계 심도

### 비주얼 에셋 제작
- **3D 모델링**: Blender, Maya, 또는 3ds Max
  - 캐릭터, 환경, 아이템 모델링
  - LOD(Level of Detail) 시스템
- **텍스처링**: Substance Painter/Designer
  - PBR 워크플로우
  - 텍스처 아틀라스 및 최적화
- **애니메이션**: Maya 또는 Blender
  - 리깅 및 스켈레탈 애니메이션
  - Unity Mecanim 또는 커스텀 애니메이션 시스템

### 특수 효과
- **파티클 시스템**: Unity VFX Graph 또는 Shuriken
  - 전투 이펙트, 환경 이펙트
  - GPU 가속 파티클
- **프로시저럴 생성**: 자체 개발 알고리즘
  - 던전 맵 생성기
  - 프로시저럴 텍스처 및 메시

## 사운드 및 오디오

### 오디오 시스템
- **오디오 엔진**: Unity Audio System 또는 FMOD
  - 공간 오디오 및 믹싱
  - 적응형 음악 시스템
- **사운드 디자인**: 오디오 에셋 제작 및 구현
  - 환경, 전투, UI 사운드 효과
  - 음성 및 대화

### 음악 및 앰비언스
- **음악 시스템**: 동적 사운드트랙
  - 상황별 음악 전환
  - 레이어드 음악 컴포지션
- **앰비언스**: 공간 오디오 디자인
  - 환경별 앰비언트 사운드
  - 동적 잔향 및 효과

## 추가 툴 및 미들웨어

### 개발 도구
- **프로파일링**: Unity Profiler, dotTrace
  - 성능 병목 식별 및 최적화
  - 메모리 및 CPU 사용량 분석
- **테스트 프레임워크**: NUnit, Unity Test Framework
  - 유닛 테스트 및 통합 테스트
  - 자동화된 테스트 실행

### 게임플레이 미들웨어
- **AI 시스템**: 자체 개발 또는 NodeCanvas / Behavior Designer
  - 행동 트리 및 유한 상태 기계
  - 경로 탐색 및 의사 결정
- **물리 및 충돌**: Unity Physics 또는 PhysX
  - 물리 기반 상호작용
  - 효율적인 충돌 감지

### 서드파티 서비스
- **클라우드 서비스**: AWS, Azure, 또는 GCP
  - 서버리스 기능(필요 시)
  - 관리형 데이터베이스 및 캐싱
- **지원 서비스**: Zendesk 또는 Discord
  - 고객 지원 및 티켓 시스템
  - 커뮤니티 관리

## 품질 보증 및 테스트

### 테스트 프레임워크
- **자동화 테스트**: NUnit 및 Unity Test Framework
  - 유닛 테스트, 통합 테스트
  - 회귀 테스트 자동화
- **성능 테스트**: Unity Profiler 및 커스텀 벤치마크
  - 프레임 레이트 및 메모리 사용량
  - 네트워크 지연 및 대역폭 테스트

### QA 프로세스
- **수동 테스트**: 체계적인 게임플레이 테스트
  - 기능 검증 및 사용자 경험 평가
  - 엣지 케이스 및 예외 상황 검사
- **자동화 QA**: CI/CD 파이프라인 통합 테스트
  - 빌드 검증 및 스모크 테스트
  - 자동화된 리포팅 시스템

[목차로 돌아가기](./MasterPlan.md)
