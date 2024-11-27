# Testcontainers 워크샵

<!--
이 README 파일에 아래 내용을 포함시켜주세요.

- 워크샵 소개
- 워크샵 목표
- 워크샵 사전 준비사항
- 워크샵 진행 방법
- 추가 참고 자료
-->

컨테이너화 하기 전의 애플리케이션을 로컬에서 테스트하기는 어렵지 않습니다. 하지만, 이미 컨테이너로 빌드한 앱이라면 어떨까요? 로컬에서 이를 테스트하기는 까다롭습니다. 하지만, 테스트컨테이너 기능을 활용하면 마치 단위테스트 코드를 짜듯 컨테이너로 빌드한 앱을 코드로 테스트할 수 있습니다. 더불어 .NET Aspire를 활용하면 컨테이너 오케스트레이션 상황에서도 손쉽게 테스트할 수 있습니다.

## 워크샵 목표

- 기존의 애플리케이션을 Dockerfile을 이용해 Containerisation을 할 수 있습니다.
- 기존의 애플리케이션을 Docker Compose를 이용해 Container Orchestration을 할 수 있습니다.
- Testcontainers를 이용해 컨테이너로 빌드한 애플리케이션을 테스트할 수 있습니다.
- .NET Aspire를 이용해 컨테이너 오케스트레이션을 손쉽게 할 수 있습니다.
- .NET Aspire를 이용해 컨테이너 오케스트레이션 상황에서도 손쉽게 테스트할 수 있습니다.
- Playwright를 이용해 UI 테스트를 할 수 있습니다.

## 워크샵 사전 준비사항

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) 설치
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 설치
- [git CLI](https://git-scm.com/downloads) 설치
- [GitHub CLI](https://cli.github.com/) 설치
- [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/) 설치
- [Visual Studio Code](https://code.visualstudio.com/) 설치

## 워크샵 진행 방법

- 아래 단계별로 워크샵을 진행합니다. 각 단계별로 자기주도형 학습을 하며, 단계별 시작 전 진행자가 간단한 안내 후 시작합니다.

  | 순서                         | 제목                                           |
  |------------------------------|------------------------------------------------|
  | [STEP 00](./docs/step-00.md) | 개발 환경 설정하기                             |
  | [STEP 01](./docs/step-01.md) | Dockerfile 및 Docker Compose 파일 생성하기     |
  | [STEP 02](./docs/step-02.md) | Testcontainers로 API 테스트하기                |
  | [STEP 03](./docs/step-03.md) | Testcontainers로 통합 테스트하기               |
  | [STEP 04](./docs/step-04.md) | .NET Aspire로 컨테이너 오케스트레이션하기      |
  | [STEP 05](./docs/step-05.md) | .NET Aspire로 통합 테스트하기                  |

- 각 단계별 중간 결과물은 [세이브 포인트](./save-points)에서 확인할 수 있습니다.

## 추가 참고 자료

- [Testcontainers](https://www.testcontainers.org/) &ndash; [.NET](https://dotnet.testcontainers.org/)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)
- [Playwright](https://playwright.dev/) &ndash; [.NET](https://playwright.dev/dotnet/)
