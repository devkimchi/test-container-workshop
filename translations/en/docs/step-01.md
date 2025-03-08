# STEP 01: Creating Dockerfile and Docker Compose File

In this step, you will create the `Dockerfile` file to containerize each app provided in the base project and the `Docker Compose` file to orchestrate them.

## Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

Refer to the document [STEP 00: Setting Up the Development Environment](./step-00.md) to check the installation status of each prerequisite.

## Copying the Base Project

The base project required for this workshop has been prepared. Verify that the project works correctly. The structure of the base project is as follows:

```text
eShopLite
└── src
    ├── eShopLite.WebApp
    ├── eShopLite.WeatherApi
    ├── eShopLite.ProductApi
    ├── eShopLite.ProductData
    └── eShopLite.DataEntities
```

1. Open the terminal and run the following commands step by step to create the practice directory and copy the base project:

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

    New-Item -Type Directory -Path workshop -Force && Copy-Item -Path ./save-points/step-00/* -Destination ./workshop -Recurse -Force
    ```

## Building and Running the Base Project

1. Build the entire project using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

1. Open three terminal windows and run the following commands in each to launch the application:

    ```bash
    # Terminal 1
    cd $REPOSITORY_ROOT/workshop
    dotnet run --project ./src/eShopLite.WeatherApi
    ```

    ```bash
    # Terminal 2
    cd $REPOSITORY_ROOT/workshop
    dotnet run --project ./src/eShopLite.ProductApi
    ```

    ```bash
    # Terminal 3
    cd $REPOSITORY_ROOT/workshop
    dotnet watch run --project ./src/eShopLite.WebApp
    ```

   > Each terminal might not recognize the `$REPOSITORY_ROOT` value. In that case, run the following commands separately for each terminal:
   > 
   > ```bash
   > # Bash/Zsh
   > REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
   > ```
   > 
   > ```powershell
   > # PowerShell
   > $REPOSITORY_ROOT = git rev-parse --show-toplevel
   > ```

1. A web browser will automatically open, displaying `https://localhost:7000` 또는 `http://localhost:5000` 주소로 접속합니다. 만약 자동으로 웹 브라우저가 열리지 않았다면 수동으로 주소를 입력합니다.
1. 웹브라우저에서 `/weather` 또는 `/products` 경로로 접속하셔 각각 페이지가 정상적으로 보이는지 확인합니다.
1. 각 터미널 창에서 `Ctrl`+`C`. Press Ctrl+C to stop the application.
1. Close all terminals except for one.

## Creating Container Images Using `Dockerfile`

The base project has dependencies for each application. The dependency structure is as follows:

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

Therefore, when creating independent container images for each application, these dependencies must be resolved.

> At this point, Docker Desktop must be running. If the Docker icon is not visible in the taskbar, launch Docker Desktop.

### Creating `Dockerfile`: `eShopLite.WebApp`

1. Navigate to the workshop root directory:

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. Use the following commands to create `Dockerfile.webapp`:

    ```bash
    # Bash/Zsh
    touch Dockerfile.webapp
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path Dockerfile.webapp -Force
    ```

1. Open `Dockerfile.webapp` and enter the following content:

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

   > **Note**: `Dockerfile.webapp`은 `eShopLite.WebApp` is the `Dockerfile` used to build the container image for the project.
   > 
   > - `Dockefile.webapp`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.
   > - `eShopLite.WebApp` 프로젝트와 `eShopLite.DataEntities` 프로젝트를 모두 복사해서 의존성을 해결합니다.

1. 아래 명령어를 실행시켜 `eShopLite.WebApp` builds the container image for the project.

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. Run the following command to execute the built container image:

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    ```

1. Open a web browser and navigate to `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` to **check for errors on each page**.

   > Errors should occur at this point.

1. Run the following command to stop the container and delete the container and container image:

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    docker rmi eshoplite-webapp:latest --force
    ```

### Creating `Dockerfile`: `eShopLite.ProductApi`

1. Navigate to the workshop root directory:

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. Use the following commands to create `Dockerfile.productapi`:

    ```bash
    # Bash/Zsh
    touch Dockerfile.productapi
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path Dockerfile.productapi -Force
    ```

1. Open `Dockerfile.productapi` and enter the following content:

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

   > **Note**: `Dockerfile.productapi`은 `eShopLite.ProductApi` is the `Dockerfile` used to build the container image for the project.
   > 
   > - `Dockefile.productapi`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.
   > - `eShopLite.ProductApi` 프로젝트와 `eShopLite.ProductData`, `eShopLite.DataEntities` 프로젝트를 모두 복사해서 의존성을 해결합니다.

1. 아래 명령어를 실행시켜 `eShopLite.ProductApi` builds the container image for the project.

    ```bash
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    ```

1. Run the following command to execute the built container image:

    ```bash
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    ```

1. Open a web browser and navigate to `http://localhost:3030` 주소로 접속하여 **404 에러가 보이는지 확인합니다**.
1. `/api/products` to verify that the data is displayed correctly.
1. Run the following command to stop the container and delete the container and container image:

    ```bash
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    docker rmi eshoplite-productapi:latest --force
    ```

### Creating `Dockerfile`: `eShopLite.WeatherApi`

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `Dockerfile.webapp`과 `Dockerfile.productapi` 작성 방법을 참고하여 `eShopLite.WeatherApi` 프로젝트를 컨테이너 이미지로 빌드하는 `Dockerfile.weatherapi`를 작성해보세요.
>
> - `Dockerfile.weatherapi`는 `eShopLite.WeatherApi` is the `Dockerfile` used to build the container image for the project.
> - `Dockefile.weatherapi`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.

`Dockerfile.weatherapi`를 작성했다면 아래 순서대로 실행해보세요.

1. 아래 명령어를 실행시켜 `eShopLite.WeatherApi` builds the container image for the project.

    ```bash
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. Run the following command to execute the built container image:

    ```bash
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. Open a web browser and navigate to `http://localhost:3031` 주소로 접속하여 **404 에러가 보이는지 확인합니다**.
1. `/api/weatherforecast` to verify that the data is displayed correctly.
1. Run the following command to stop the container and delete the container and container image:

    ```bash
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## Container Orchestration: Docker Network

Use the three `Dockerfile` files created earlier to orchestrate the containers.

1. Run the following command to build the container images for `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi`:

    ```bash
    cd $REPOSITORY_ROOT/workshop
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. Run the following command to execute the built container images:

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. Open a web browser and navigate to `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` to **check for errors on each page**.

   > Errors should still occur at this point.

1. Run the following command to stop and delete all containers:

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force

    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force

    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    ```

1. Open the `src/eShopLite.WebApp/Program.cs` file and modify the code as follows:

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

1. Rebuild the container image for the `eShopLite.WebApp` project:

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. Run the following command to create a network:

    ```bash
    docker network create eshoplite
    ```

1. Run the following command to execute the container images for `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi`:

    ```bash
    docker run -d -p 3000:8080 --network eshoplite --network-alias webapp --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --network eshoplite --network-alias productapi --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --network eshoplite --network-alias weatherapi --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. Open a web browser and navigate to `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` to **verify that each page displays correctly**.
1. Run the following command to delete the containers and network:

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    
    docker network rm eshoplite --force
    ```

1. Run the following command to delete the container images:

    ```bash
    docker rmi eshoplite-webapp:latest --force
    docker rmi eshoplite-productapi:latest --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## Container Orchestration: Docker Compose

This time, orchestrate the containers using Docker Compose.

1. Run the following command to create the `compose.yaml` file:

    ```bash
    # Bash/Zsh
    cd $REPOSITORY_ROOT/workshop
    touch compose.yaml
    ```

    ```powershell
    # PowerShell
    cd $REPOSITORY_ROOT/workshop
    New-Item -Type File -Path compose.yaml -Force
    ```

   > - Open the `compose.yaml`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.

1. `compose.yaml` file and enter the following content:

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

1. Run the following command to execute the containers:

    ```bash
    docker compose up --build
    ```

1. Open a web browser and navigate to `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` 경로로 접속하여 **각 페이지가 정상적으로 보이는지 확인합니다**.
1. 터미널 창에서 `Ctrl`+`C` and press Ctrl+C to stop the application.
1. Run the following command to delete the containers, network, and container images at once:

    ```bash
    docker compose down --rmi local
    ```

---

Congratulations! The **Creating Dockerfile and Docker Compose File** workshop is complete. Proceed to the next step, [STEP 02: Testing APIs with Testcontainers](./step-02.md).

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While efforts are made to ensure accuracy, please note that automated translations may include errors or inaccuracies. The original document in its native language should be regarded as the definitive source. For crucial information, professional human translation is advised. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.