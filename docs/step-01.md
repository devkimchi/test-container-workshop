# STEP 01: Dockerfile 및 Docker Compose 파일 생성

이 단계에서는 기본 프로젝트에 `Dockerfile` 및 `Docker Compose` 파일을 생성합니다.

## 사전 준비 사항

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) 설치
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 설치
- [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/) 설치
- [Visual Studio Code](https://code.visualstudio.com/) 설치

각 사전 준비사항의 설치 여부 확인은 [STEP 00: 개발 환경 설정](./step-00.md) 문서를 참고해주세요.

## 기본 프로젝트 복사

이 워크샵을 위해 필요한 기본 프로젝트를 준비해 뒀습니다. 이 프로젝트가 제대로 작동하는지 확인합니다. 기본 프로젝트의 프로젝트 구조는 아래와 같습니다.

```text
eShopLite
└── src
    ├── eShopLite.WebApp
    ├── eShopLite.WeatherApi
    ├── eShopLite.ProductApi
    ├── eShopLite.ProductData
    └── eShopLite.DataEntities
```

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 기본 프로젝트를 복사합니다.

    ```bash
    # Bash/Zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    cd $REPOSITORY_ROOT

    mkdir -p workshop && cp -a save-points/step-00/. workshop/
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    cd $REPOSITORY_ROOT

    New-Item -Type Directory -Path workshop -Force && Copy-Item -Path ./save-points/stepp-00/* -Destination ./workshop -Recurse -Force
    ```

## 기본 프로젝트 빌드 및 실행

1. 아래 명령어를 통해 전체 프로젝트를 빌드합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

1. 터미널 창을 세 개 열고 각각 아래 명령어를 실행시켜 애플리케이션을 실행합니다.

    ```bash
    # Terminal 1
    dotnet run --project ./src/eShopLite.WeatherApi

    # Terminal 2
    dotnet run --project ./src/eShopLite.ProductApi

    # Terminal 3
    dotnet watch run --project ./src/eShopLite.WebApp
    ```

1. 자동으로 웹 브라우저가 열리면서 `https://localhost:7000` 또는 `http://localhost:5000` 주소로 접속합니다. 만약 자동으로 웹 브라우저가 열리지 않았다면 수동으로 주소를 입력합니다.
1. 웹브라우저에서 `/weather` 또는 `/products` 경로로 접속하셔 각각 페이지가 정상적으로 보이는지 확인합니다.
1. 각 터미널 창에서 `Ctrl`+`C`를 눌러 애플리케이션을 종료합니다.

## `Dockerfile`을 이용한 컨테이너 이미지 생성

기본 프로젝트는 각 애플리케이션마다 의존성을 갖고 있습니다. 이 의존성 구조는 아래와 같습니다.

```text
eShopLite
└── src
    ├── eShopLite.WebApp
    │   └── eShopLite.DataEntities
    ├── eShopLite.ProductApi
    │   └── eShopLite.ProductData
    │       └── eShopLite.DataEntities
    └── eShopLite.WeatherApi
        └── eShopLite.DataEntities
```

따라서, 개별 애플리케이션마다 독립적인 컨테이너 이미지를 생성할 때 이 의존성을 해결해야 합니다.

### `Dockerfile` 생성: `eShopLite.WebApp`

1. 워크샵 루트 디렉토리로 이동합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 아래 명령어를 통해 `Dockerfile.webapp`을 생성합니다.

    ```bash
    touch Dockerfile.webapp
    ```

1. `Dockerfile.webapp`을 열고 아래 내용을 입력합니다.

    ```dockerfile
    # syntax=docker/dockerfile:1
    
    FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

    COPY ./src/eShopLite.WebApp /source/eShopLite.WebApp
    COPY ./src/eShopLite.DataEntities /source/eShopLite.DataEntities
    
    WORKDIR /source/eShopLite.WebApp
    
    RUN dotnet publish -c Release -o /app
    
    FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
    
    WORKDIR /app
    
    COPY --from=build /app .
    
    USER $APP_UID
    
    ENTRYPOINT ["dotnet", "eShopLite.WebApp.dll"]
    ```

   > **참고**: `Dockerfile.webapp`은 `eShopLite.WebApp` 프로젝트를 컨테이너 이미지로 빌드하기 위한 `Dockerfile`입니다.
   > 
   > - `Dockefile.webapp`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.
   > - `eShopLite.WebApp` 프로젝트와 `eShopLite.DataEntities` 프로젝트를 모두 복사해서 의존성을 해결합니다.

1. 아래 명령어를 실행시켜 `eShopLite.WebApp` 프로젝트를 컨테이너 이미지로 빌드합니다.

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. 아래 명령어를 실행시켜 빌드한 컨테이너 이미지를 실행합니다.

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    ```

1. 웹 브라우저에서 `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` 경로로 접속하여 **각각 페이지에 에러가 발생하는지 확인합니다**.

   > 이 시점에서는 에러가 발생해야 합니다.

1. 아래 명령어를 실행시켜 컨테이너를 종료하고 컨테이너 및 컨테이너 이미지를 삭제합니다.

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    docker rmi eshoplite-webapp:latest --force
    ```

### `Dockerfile` 생성: `eShopLite.ProductApi`

1. 워크샵 루트 디렉토리로 이동합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. 아래 명령어를 통해 `Dockerfile.productapi`을 생성합니다.

    ```bash
    touch Dockerfile.productapi
    ```

1. `Dockerfile.productapi`을 열고 아래 내용을 입력합니다.

    ```dockerfile
    # syntax=docker/dockerfile:1
    
    FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
    
    COPY ./src/eShopLite.ProductApi /source/eShopLite.ProductApi
    COPY ./src/eShopLite.ProductData /source/eShopLite.ProductData
    COPY ./src/eShopLite.DataEntities /source/eShopLite.DataEntities
    
    WORKDIR /source/eShopLite.ProductApi
    
    RUN dotnet publish -c Release -o /app
    
    FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
    
    WORKDIR /app
    
    COPY --from=build /app .
    
    RUN chown $APP_UID /app
    
    USER $APP_UID
    
    RUN touch /app/Database.db
    
    ENTRYPOINT ["dotnet", "eShopLite.ProductApi.dll"]
    ```

   > **참고**: `Dockerfile.productapi`은 `eShopLite.ProductApi` 프로젝트를 컨테이너 이미지로 빌드하기 위한 `Dockerfile`입니다.
   > 
   > - `Dockefile.productapi`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.
   > - `eShopLite.ProductApi` 프로젝트와 `eShopLite.ProductData`, `eShopLite.DataEntities` 프로젝트를 모두 복사해서 의존성을 해결합니다.

1. 아래 명령어를 실행시켜 `eShopLite.ProductApi` 프로젝트를 컨테이너 이미지로 빌드합니다.

    ```bash
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    ```

1. 아래 명령어를 실행시켜 빌드한 컨테이너 이미지를 실행합니다.

    ```bash
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    ```

1. 웹 브라우저에서 `http://localhost:3030` 주소로 접속하여 **404 에러가 보이는지 확인합니다**.
1. `/api/products` 경로로 접속하여 데이터가 정상적으로 보이는지 확인합니다.
1. 아래 명령어를 실행시켜 컨테이너를 종료하고 컨테이너 및 컨테이너 이미지를 삭제합니다.

    ```bash
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    docker rmi eshoplite-productapi:latest --force
    ```

### `Dockerfile` 생성: `eShopLite.WeatherApi`

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `Dockerfile.webapp`과 `Dockerfile.productapi` 작성 방법을 참고하여 `eShopLite.WeatherApi` 프로젝트를 컨테이너 이미지로 빌드하는 `Dockerfile.weatherapi`를 작성해보세요.
>
> - `Dockerfile.weatherapi`는 `eShopLite.WeatherApi` 프로젝트를 컨테이너 이미지로 빌드하기 위한 `Dockerfile`입니다.
> - `Dockefile.weatherapi`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.

`Dockerfile.weatherapi`를 작성했다면 아래 순서대로 실행해보세요.

1. 아래 명령어를 실행시켜 `eShopLite.WeatherApi` 프로젝트를 컨테이너 이미지로 빌드합니다.

    ```bash
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. 아래 명령어를 실행시켜 빌드한 컨테이너 이미지를 실행합니다.

    ```bash
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. 웹 브라우저에서 `http://localhost:3031` 주소로 접속하여 **404 에러가 보이는지 확인합니다**.
1. `/api/weatherforecast` 경로로 접속하여 데이터가 정상적으로 보이는지 확인합니다.
1. 아래 명령어를 실행시켜 컨테이너를 종료하고 컨테이너 및 컨테이너 이미지를 삭제합니다.

    ```bash
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## 컨테이너 오케스트레이션: Docker Network

앞서 작성한 세 개의 `Dockerfile`을 이용해 컨테이너 오케스트레이션을 합니다.

1. 아래 명령어를 실행시켜 `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi` 컨테이너 이미지를 빌드합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. 아래 명령어를 실행시켜 빌드한 컨테이너 이미지를 실행시킵니다.

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. 웹 브라우저를 열고 `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` 경로로 접속하여 **각각 페이지에 에러가 발생하는지 확인합니다**.

   > 이 시점에서도 여전히 에러가 발생해야 합니다.

1. 아래 명령어를 실행시켜 모든 컨테이너를 종료하고 삭제합니다.

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force

    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force

    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    ```

1. `src/eShopLite.WebApp/Program.cs` 파일을 열고 아래와 같이 코드를 수정합니다.

    ```csharp
    // 변경 전
    builder.Services.AddHttpClient<ProductApiClient>(client =>
    {
        client.BaseAddress = new("http://localhost:5051");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("http://localhost:5050");
    });
    ```

    ```csharp
    // 변경 후
    builder.Services.AddHttpClient<ProductApiClient>(client =>
    {
        client.BaseAddress = new("http://productapi:8080");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("http://weatherapi:8080");
    });
    ```

1. `eShopLite.WebApp` 프로젝트의 컨테이너 이미지를 다시 빌드합니다.

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. 아래 명령어를 실행시켜 네트워크를 생성합니다.

    ```bash
    docker network create eshoplite
    ```

1. 아래 명령어를 실행시켜 `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi` 컨테이너 이미지를 실행합니다.

    ```bash
    docker run -d -p 3000:8080 --network eshoplite --network-alias webapp --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --network eshoplite --network-alias productapi --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --network eshoplite --network-alias weatherapi --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. 웹 브라우저를 열고 `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` 경로로 접속하여 **각 페이지가 정상적으로 보이는지 확인합니다**.
1. 아래 명령어를 실행시켜 컨테이너와 네트워크를 삭제합니다.

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    
    docker network rm eshoplite --force
    ```

1. 아래 명령어를 실행시켜 컨테이너 이미지를 삭제합니다.

    ```bash
    docker rmi eshoplite-webapp:latest --force
    docker rmi eshoplite-productapi:latest --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## 컨테이너 오케스트레이션: Docker Compose

이번에는 Docker Compose를 이용해 컨테이너 오케스트레이션을 합니다.

1. 아래 명령어를 실행시켜 `compose.yaml` 파일을 생성합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop
    touch compose.yaml
    ```

   > - `compose.yaml`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.

1. `compose.yaml` 파일을 열고 아래 내용을 입력합니다.

    ```yaml
    name: eShopLite
    
    services:
      productapi:
        container_name: productapi
        build:
          context: .
          dockerfile: ./Dockerfile.productapi
          target: final
        ports:
          - 3030:8080
      weatherapi:
        container_name: weatherapi
        build:
          context: .
          dockerfile: ./Dockerfile.weatherapi
          target: final
        ports:
          - 3031:8080
      webapp:
        container_name: webapp
        build:
          context: .
          dockerfile: ./Dockerfile.webapp
          target: final
        ports:
          - 3000:8080
        depends_on:
          - productapi
          - weatherapi
    ```

1. 아래 명령어를 실행시켜 컨테이너를 실행합니다.

    ```bash
    docker compose up --build
    ```

1. 웹 브라우저를 열고 `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` 경로로 접속하여 **각 페이지가 정상적으로 보이는지 확인합니다**.
1. 터미널 창에서 `Ctrl`+`C`를 눌러 애플리케이션을 종료합니다.
1. 아래 명령어를 실행시켜 컨테이너와 네트워크, 컨테이너 이미지를 한 번에 삭제합니다.

    ```yml
    docker compose down --local
    ```

---

축하합니다! 개발 환경 설정이 끝났습니다. 이제 [STEP 02: TestContaienr로 API 테스트하기](./step-02.md) 단계로 넘어가세요.
