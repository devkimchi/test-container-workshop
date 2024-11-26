# STEP 05: .NET Aspire로 통합 테스트하기

이 단계에서는 [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)을 이용해 모든 앱을 통합 테스트합니다.

## 사전 준비 사항

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) 설치
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 설치
- [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/) 설치
- [Visual Studio Code](https://code.visualstudio.com/) 설치

각 사전 준비사항의 설치 여부 확인은 [STEP 00: 개발 환경 설정하기](./step-00.md) 문서를 참고해주세요.

## 이전 프로젝트 복사

이전 단계에서 사용하던 앱을 그대로 사용해도 좋고, 아래 명령어를 통해 세이브포인트로부터 새롭게 복사해서 사용해도 좋습니다. 새롭게 복사하려면 아래 명령어를 사용하세요.

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 이전 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    cd $REPOSITORY_ROOT

    mkdir -p workshop && cp -a save-points/step-04/. workshop/
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    cd $REPOSITORY_ROOT

    New-Item -Type Directory -Path workshop -Force && Copy-Item -Path ./save-points/step-04/* -Destination ./workshop -Recurse -Force
    ```

1. 아래 명령어를 통해 전체 프로젝트를 빌드합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## 테스트 프로젝트 생성: xxxx

---

축하합니다! **.NET Aspire로 통합 테스트하기** 실습이 끝났습니다.
