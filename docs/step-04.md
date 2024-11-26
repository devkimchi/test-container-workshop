# STEP 04: .NET Aspire로 컨테이너 오케스트레이션하기

이 단계에서는 [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)을 이용해 모든 앱을 오케스트레이션합니다.

## 사전 준비 사항

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) 설치
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 설치
- [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/) 설치
- [Visual Studio Code](https://code.visualstudio.com/) 설치

각 사전 준비사항의 설치 여부 확인은 [STEP 00: 개발 환경 설정하기](./step-00.md) 문서를 참고해주세요.

## 기본 프로젝트 복사

이전 단계에서 사용하던 앱을 그대로 사용해도 좋고, 아래 명령어를 통해 세이브포인트로부터 새롭게 복사해서 사용해도 좋습니다. 새롭게 복사하려면 아래 명령어를 사용하세요.

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

    New-Item -Type Directory -Path workshop -Force && Copy-Item -Path ./save-points/step-00/* -Destination ./workshop -Recurse -Force
    ```

1. 아래 명령어를 통해 전체 프로젝트를 빌드합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## .NET Aspire 오케스트레이션 프로젝트 추가

앞서 진행했던 실습과 달리, 이번 실습에서는 .NET Aspire를 이용해 컨테이너 오케스트레이션을 진행합니다.

### .NET Aspire 프로젝트 추가

1. 아래 명령어를 실행시켜 .NET Aspire 오케스트레이터 프로젝트를 추가합니다.

    ```bash
    dotnet new aspire-apphost -n eShopLite.AppHost -o src/eShopLite.AppHost
    dotnet sln eShopLite.sln add ./src/eShopLite.AppHost
    ```

1. 아래 명령어를 실행시켜 .NET Aspire 오케스트레이터 프로젝트에 모든 앱을 추가합니다.

    ```bash
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.WebApp
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.ProductApi
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.WeatherApi
    ```

1. 아래 명령어를 실행시켜 .NET Aspire 기본 서비스 프로젝트를 추가합니다.

    ```bash
    dotnet new aspire-servicedefaults -n eShopLite.ServiceDefaults -o src/eShopLite.ServiceDefaults
    dotnet sln eShopLite.sln add ./src/eShopLite.ServiceDefaults
    ```

1. 아래 명령어를 실행시켜 .NET Aspire 기본 서비스 프로젝트를 각 앱에 추가합니다.

    ```bash
    dotnet add ./src/eShopLite.WebApp reference ./src/eShopLite.ServiceDefaults
    dotnet add ./src/eShopLite.ProductApi reference ./src/eShopLite.ServiceDefaults
    dotnet add ./src/eShopLite.WeatherApi reference ./src/eShopLite.ServiceDefaults
    ```

### `eShopLite.WebApp` 프로젝트 수정

1. `src/eShopLite.WebApp/Program.cs` 파일을 열고 `var builder = WebApplication.CreateBuilder(args);` 줄 바로 아래에 다음 내용을 추가합니다.

    ```csharp
    builder.AddServiceDefaults();
    ```

   > 기본 서비스 프로젝트에서 제공하는 서비스를 사용하도록 설정합니다.

1. 아래 코드를 수정합니다.

    ```csharp
    // 변경전
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
    // 변경후
    builder.Services.AddHttpClient<ProductApiClient>(client =>
    {
        client.BaseAddress = new("https+http://productapi");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("https+http://weatherapi");
    });
    ```

   > 오케스트레이터에서 제공하는 서비스 디스커버리를 사용하도록 수정합니다.

1. `app.Run();` 줄 바로 위에 다음 내용을 추가합니다.

    ```csharp
    app.MapDefaultEndpoints();
    ```

   > 기본 서비스 프로젝트의 상태 체크 엔드포인트를 사용하도록 설정합니다.

### `eShopLite.ProductApi` 프로젝트 수정

1. `src/eShopLite.ProductApi/Program.cs` 파일을 열고 `var builder = WebApplication.CreateBuilder(args);` 줄 바로 아래에 다음 내용을 추가합니다.

    ```csharp
    builder.AddServiceDefaults();
    ```

1. `app.Run();` 줄 바로 위에 다음 내용을 추가합니다.

    ```csharp
    app.MapDefaultEndpoints();
    ```

### `eShopLite.WeatherApi` 프로젝트 수정

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `eShopLite.ProductApi` 프로젝트 수정과 마찬가지로 `eShopLite.WeatherApi` 프로젝트를 수정해 보세요.

### `eShopLite.AppHost` 프로젝트 수정

1. `src/eShopLite.AppHost/Program.cs` 파일을 열고 `var builder = DistributedApplication.CreateBuilder(args);` 줄 바로 아래에 다음 내용을 추가합니다.

    ```csharp
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi");
    var weatherapi = builder.AddProject<Projects.eShopLite_WeatherApi>("weatherapi");
    ```

   > 오케스트레이터 프로젝트인 `AppHost`에 `ProductApi`와 `WeatherApi` 프로젝트를 추가합니다.

1. 그 아래 줄에 다음 내용을 추가합니다.

    ```csharp
    builder.AddProject<Projects.eShopLite_WebApp>("webapp")
           .WithExternalHttpEndpoints()
           .WithReference(productapi)
           .WithReference(weatherapi)
           .WaitFor(productapi)
           .WaitFor(weatherapi);
    ```

   > 오케스트레이터 프로젝트인 `AppHost`에 `WebApp` 프로젝트를 추가합니다.
   > 
   > - `.WithExternalHttpEndpoints()`: 외부 HTTP 엔드포인트를 사용할 수 있도록 설정합니다.
   > - `.WithReference(productapi)`: `WebApp` 프로젝트가 `ProductApi` 프로젝트를 참조하도록 설정합니다.
   > - `.WithReference(weatherapi)`: `WebApp` 프로젝트가 `WeatherApi` 프로젝트를 참조하도록 설정합니다.
   > - `.WaitFor(productapi)`: `ProductApi` 프로젝트가 준비될 때까지 기다리도록 설정합니다.
   > - `.WaitFor(weatherapi)`: `WeatherApi` 프로젝트가 준비될 때까지 기다리도록 설정합니다.

### .NET Aspire 오케스트레이터 실행

1. 아래 명령어를 실행시켜 .NET Aspire 오케스트레이터를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet watch run --project ./src/eShopLite.AppHost
    ```

1. 자동으로 웹 브라우저가 열리면서 대시보드가 나타납니다. 대시보드에 아래와 같이 `productapi`, `weatherapi`, `webapp` 리소스가 나타나면 성공입니다.

    ![Aspire Dashboard](./images/aspire-dashboard-1.png)

   > 경우에 따라 아래와 같이 로그인 화면이 나타날 수 있습니다.
   > 
   > ![Aspire Dashboard Login](./images/aspire-dashboard-login.png)
   > 
   > 화살표가 가리키는 링크를 클릭해서 안내에 따라 로그인하면 대시보드를 볼 수 있습니다.

1. 대시보드에 나타난 `productapi`와 `weatherapi` 각각의 Endpoints 링크를 클릭하면 OpenAPI 문서를 볼 수 있습니다.
1. 대시보드에 나타난 `webapp`의 Endpoints 링크를 클릭하면 웹 앱을 볼 수 있습니다. `/products`와 `/weather` 페이지를 확인해 보세요.
1. 터미널에서 `Ctrl`+`C`를 눌러 .NET Aspire 오케스트레이터를 종료합니다.

## .NET Aspire 오케스트레이터에서 데이터베이스 교체

지금까지 사용하던 데이터베이스를 SQLite에서 PostgreSQL로 교체해 보겠습니다.

### `eShopLite.AppHost` 프로젝트 수정

1. 아래 명령어를 실행시켜 `eShopLite.AppHost` 프로젝트에 PostgreSQL 패키지를 추가합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet add ./src/eShopLite.AppHost package Aspire.Hosting.PostgreSQL
    ```

1. `src/eShopLite.AppHost/Program.cs` 파일을 열고 `var builder = DistributedApplication.CreateBuilder(args);` 줄 바로 아래에 다음 내용을 추가합니다.

    ```csharp
    var productsdb = builder.AddPostgres("pg")
                            .WithPgAdmin()
                            .AddDatabase("productsdb");
    ```

   > PostgreSQL 데이터베이스를 추가합니다.
   > 
   > - `.WithPgAdmin()`: pgAdmin 대시보드를 사용할 수 있도록 설정합니다.
   > - `.AddDatabase("productsdb")`: `productsdb`라는 이름의 데이터베이스를 추가합니다.

1. 아래 내용을 수정합니다.

    ```csharp
    // 변경전
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi");
    ```

    ```csharp
    // 변경후
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi")
                            .WithReference(productsdb);
    ```

   > `ProductApi` 프로젝트에 PostgreSQL 데이터베이스를 추가합니다.
   > 
   > - `.WithReference(productsdb)`: `ProductApi` 프로젝트가 PostgreSQL 데이터베이스를 참조하도록 설정합니다.

### `eShopLite.ProductApi` 프로젝트 수정

1. 아래 명령어를 실행시켜 `eShopLite.ProductApi` 프로젝트에 PostgreSQL 패키지를 추가합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet add ./src/eShopLite.ProductApi package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
    ```

1. `src/eShopLite.ProductApi/appsettings.json` 파일을 열고 `ConnectionStrings` 섹션을 완전히 지웁니다. 이후 `appsettings.json` 파일은 다음과 같습니다.

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
    
      "AllowedHosts": "*"
    }
    ```

   > SQLite 데이터베이스 연결 문자열을 지웁니다.

1. `src/eShopLite.ProductApi/Program.cs` 파일을 열고 아래와 같이 변경합니다.

    ```csharp
    // 변경전
    builder.Services.AddDbContext<ProductDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("ProductsContext") ?? throw new InvalidOperationException("Connection string 'ProductsContext' not found.");
        options.UseSqlite(connectionString);
    });
    ```

    ```csharp
    // 변경후
    builder.AddNpgsqlDbContext<ProductDbContext>("productsdb");
    ```

   > PostgreSQL 데이터베이스 연결 문자열을 사용하도록 변경합니다.
   > 
   > - `productsdb`는 `AppHost` 프로젝트에서 PostgreSQL 데이터베이스를 추가할 때 사용한 이름입니다.

### .NET Aspire 오케스트레이터 실행

1. 아래 명령어를 실행시켜 .NET Aspire 오케스트레이터를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet watch run --project ./src/eShopLite.AppHost
    ```

1. 자동으로 웹 브라우저가 열리면서 대시보드가 나타납니다. 대시보드에 아래와 같이 `pg`, `pg-pgadmin`, `productsdb`, `productapi`, `weatherapi`, `webapp` 리소스가 나타나면 성공입니다.

    ![Aspire Dashboard](./images/aspire-dashboard-2.png)

   > 경우에 따라 아래와 같이 로그인 화면이 나타날 수 있습니다.
   > 
   > ![Aspire Dashboard Login](./images/aspire-dashboard-login.png)
   > 
   > 화살표가 가리키는 링크를 클릭해서 안내에 따라 로그인하면 대시보드를 볼 수 있습니다.

1. 대시보드에 나타난 `pg-pgadmin`의 Endpoints 링크를 클릭하면 PostgreSQL 데이터베이스의 관리자 대시보드 화면을 볼 수 있습니다.
1. 대시보드에 나타난 `productapi`와 `weatherapi` 각각의 Endpoints 링크를 클릭하면 OpenAPI 문서를 볼 수 있습니다.
1. 대시보드에 나타난 `webapp`의 Endpoints 링크를 클릭하면 웹 앱을 볼 수 있습니다. `/products`와 `/weather` 페이지를 확인해 보세요.
1. 터미널에서 `Ctrl`+`C`를 눌러 .NET Aspire 오케스트레이터를 종료합니다.

---

축하합니다! **.NET Aspire로 컨테이너 오케스트레이션하기** 실습이 끝났습니다. 이제 [STEP 05: .NET Aspire로 통합 테스트하기](./step-05.md) 단계로 넘어가세요.
