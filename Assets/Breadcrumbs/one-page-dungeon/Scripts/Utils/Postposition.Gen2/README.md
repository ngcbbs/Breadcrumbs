# PostpositionProcessor 모듈

한국어 조사 처리를 위한 유니티 모듈입니다. 한국어, 영어, 일본어 등 다양한 언어의 단어에 한국어 조사를 자연스럽게 적용할 수 있습니다.

## 주요 기능

- 한국어, 영어, 일본어, 숫자에 대한 조사 처리 지원
- 영어 발음 기반의 조사 처리 (단순 문자가 아닌 실제 발음 고려)
- 조사 타입 표준화와 열거형을 통한 직관적인 사용
- 정규식을 활용한 메시지 패턴 처리
- 전략 패턴을 통한 확장성 있는 설계
- 유니티 환경 및 비유니티 환경 모두 지원

## 모듈 구조

1. **PostpositionProcessor.cs**
   - 조사 처리의 핵심 로직과 기본 인터페이스 제공
   - 전략 패턴을 통한 언어별 처리 위임
   - 메시지 패턴 처리와 조사 적용 기능

2. **LanguageDetector.cs**
   - 언어 감지 인터페이스와 구현체
   - 유니티 의존성 분리
   - 시스템 문화권 기반 언어 감지 기능

3. **CharacterClassifier.cs**
   - 문자 유형 분류 및 특성 판별 유틸리티
   - 한글, 영어, 일본어, 숫자 판별
   - 받침 여부 및 자음/모음 판별 기능

4. **PostpositionStrategies.cs**
   - 언어별 조사 처리 전략 인터페이스와 구현체
   - 한국어, 영어, 일본어, 숫자별 조사 처리 로직

5. **Models.cs**
   - 공통 모델 및 열거형 정의
   - 언어 타입, 조사 타입 등 모델화

6. **EnglishToKoreanConverter.cs**
   - 영어 단어의 한글 변환 및 발음 분석
   - 받침 여부 판단 알고리즘
   - 영어 발음 규칙 기반 조사 처리

7. **Example.cs**
   - 모듈 사용 예제 코드

## 사용 방법

### 기본 조사 처리

```csharp
// 문자열 조사 처리
string result1 = PostpositionProcessor.ApplyPostposition("사과", "은/는");  // "사과는"
string result2 = PostpositionProcessor.ApplyPostposition("책", "은/는");    // "책은"
string result3 = PostpositionProcessor.ApplyPostposition("apple", "은/는"); // "apple은"

// 조사 타입 사용
string result4 = PostpositionProcessor.ApplyPostposition("사과", PostpositionType.Subject);  // "사과는"
string result5 = PostpositionProcessor.ApplyPostposition("책", PostpositionType.Object);    // "책을"
```

### 메시지 패턴 처리

```csharp
// 메시지와 키워드 매핑 사용
string message = "안녕하세요, {이름:은/는} 학생입니다.";
Dictionary<string, string> keywordMap = new Dictionary<string, string> { { "이름", "홍길동" } };
string processedMessage = PostpositionProcessor.ProcessMessage(message, keywordMap);
// "안녕하세요, 홍길동은 학생입니다."

// 객체 속성 매핑 사용
var person = new Person { Name = "김철수", Job = "디자이너" };
string message2 = "{Name:이/가} {Job:을/를} 합니다.";
string processedMessage2 = PostpositionProcessor.ProcessMessage(message2, person);
// "김철수가 디자이너를 합니다."
```

### 유니티 환경에서 언어 감지기 설정

```csharp
// 유니티 언어 감지기 설정
LanguageDetectorFactory.SetDetector(new UnityLanguageDetector(() => Application.systemLanguage.ToString()));
```

## 확장하기

새로운 언어나 예외 케이스를 지원하려면:

1. `IPostpositionStrategy` 인터페이스를 구현하는 새로운 전략 클래스 추가
2. `PostpositionStrategyFactory`에 새 전략 등록
3. 필요한 경우 `CharacterClassifier` 또는 해당 언어에 특화된 변환기 추가

## 주의사항

- 유니티 환경에서 사용 시 `UNITY_2017_1_OR_NEWER` 전처리기 지시문을 통해 유니티 종속성 처리
- 대용량 텍스트 처리 시 정규식 패턴 매칭으로 인한 성능 이슈 고려
