# STEP 04: Container Orchestration with .NET Aspire

In this step, you'll orchestrate all apps using [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview).

## Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

To verify the installation of each prerequisite, refer to the document [STEP 00: Setting Up the Development Environment](./step-00.md).

## Copy Base Project

You can continue using the app from the previous step or copy a fresh version from the save point using the commands below. To copy a fresh version, use the following commands:

1. Open a terminal and execute the following commands sequentially to create the practice directory and copy the base project:

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

1. Build the entire project using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## Adding a .NET Aspire Orchestration Project

Unlike the previous practice, this time you will use .NET Aspire for container orchestration.

Once you install the .NET Aspire orchestration project, the overall solution structure will change as shown below:

```text
eShopLite
└── src
    ├── eShopLite.AppHost
    │   ├── eShopLite.WebApp
    │   ├── eShopLite.ProductApi
    │   └── eShopLite.WeatherApi
    ├── eShopLite.ServiceDefaults
    ├── eShopLite.WebApp
    │   ├── eShopLite.DataEntities
    │   └── eShopLite.ServiceDefaults
    ├── eShopLite.WeatherApi
    │   ├── eShopLite.DataEntities
    │   └── eShopLite.ServiceDefaults
    └── eShopLite.ProductApi
        ├── eShopLite.ProductData
        │   └── eShopLite.DataEntities
        └── eShopLite.ServiceDefaults
```

### Adding the .NET Aspire Project

1. Run the following command to add the .NET Aspire orchestrator project:

    ```bash
    dotnet new aspire-apphost -n eShopLite.AppHost -o src/eShopLite.AppHost
    dotnet sln eShopLite.sln add ./src/eShopLite.AppHost
    ```

1. Execute the following command to add all apps to the .NET Aspire orchestrator project:

    ```bash
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.WebApp
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.ProductApi
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.WeatherApi
    ```

1. Use the command below to add the .NET Aspire base service project:

    ```bash
    dotnet new aspire-servicedefaults -n eShopLite.ServiceDefaults -o src/eShopLite.ServiceDefaults
    dotnet sln eShopLite.sln add ./src/eShopLite.ServiceDefaults
    ```

1. Run the following command to add the .NET Aspire base service project to each app:

    ```bash
    dotnet add ./src/eShopLite.WebApp reference ./src/eShopLite.ServiceDefaults
    dotnet add ./src/eShopLite.ProductApi reference ./src/eShopLite.ServiceDefaults
    dotnet add ./src/eShopLite.WeatherApi reference ./src/eShopLite.ServiceDefaults
    ```

### Modify `eShopLite.WebApp` 프로젝트 수정

1. `src/eShopLite.WebApp/Program.cs` 파일을 열고 `var builder = WebApplication.CreateBuilder(args);` by adding the following content immediately below:

    ```csharp
    builder.AddServiceDefaults();
    ```

   > This enables the use of services provided by the base service project.

1. Update the following code:

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

   > Modify to use service discovery provided by the orchestrator.

1. Add the following content right before `app.Run();`:

    ```csharp
    app.MapDefaultEndpoints();
    ```

   > Enables the use of the health check endpoint from the base service project.

### Modify `eShopLite.ProductApi` 프로젝트 수정

1. `src/eShopLite.ProductApi/Program.cs` 파일을 열고 `var builder = WebApplication.CreateBuilder(args);` by adding the following content immediately below:

    ```csharp
    builder.AddServiceDefaults();
    ```

1. Add the following content right before `app.Run();`:

    ```csharp
    app.MapDefaultEndpoints();
    ```

### Modify `eShopLite.WeatherApi` 프로젝트 수정

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `eShopLite.ProductApi` 프로젝트 수정과 마찬가지로 `eShopLite.WeatherApi` 프로젝트를 수정해 보세요.

### `eShopLite.AppHost` 프로젝트 수정

1. `src/eShopLite.AppHost/Program.cs` 파일을 열고 `var builder = DistributedApplication.CreateBuilder(args);` by adding the following content immediately below:

    ```csharp
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi");
    var weatherapi = builder.AddProject<Projects.eShopLite_WeatherApi>("weatherapi");
    ```

   > Add the orchestrator project: `AppHost`에 `ProductApi`와 `WeatherApi`.

1. Add the following content in the subsequent line:

    ```csharp
    builder.AddProject<Projects.eShopLite_WebApp>("webapp")
           .WithExternalHttpEndpoints()
           .WithReference(productapi)
           .WithReference(weatherapi)
           .WaitFor(productapi)
           .WaitFor(weatherapi);
    ```

   > Configure the orchestrator project `AppHost`에 `WebApp` 프로젝트를 추가합니다.
   > 
   > - `.WithExternalHttpEndpoints()`: 외부 HTTP 엔드포인트를 사용할 수 있도록 설정합니다.
   > - `.WithReference(productapi)`: `WebApp` 프로젝트가 `ProductApi` 프로젝트를 참조하도록 설정합니다.
   > - `.WithReference(weatherapi)`: `WebApp` 프로젝트가 `WeatherApi` 프로젝트를 참조하도록 설정합니다.
   > - `.WaitFor(productapi)`: `ProductApi` 프로젝트가 준비될 때까지 기다리도록 설정합니다.
   > - `.WaitFor(weatherapi)`: `WeatherApi` to wait until all projects are ready.

### Running the .NET Aspire Orchestrator

1. Execute the following command to run the .NET Aspire orchestrator:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet watch run --project ./src/eShopLite.AppHost
    ```

1. A web browser will automatically open, displaying the dashboard. The dashboard will show `productapi`, `weatherapi`, `webapp` 리소스가 나타나면 성공입니다.

    ![Aspire Dashboard](../../../docs/images/aspire-dashboard-1.png)

   > 경우에 따라 아래와 같이 로그인 화면이 나타날 수 있습니다.
   > 
   > ![Aspire Dashboard Login](../../../docs/images/aspire-dashboard-login.png)
   > 
   > 화살표가 가리키는 링크를 클릭해서 안내에 따라 로그인하면 대시보드를 볼 수 있습니다.

1. 대시보드에 나타난 `productapi`와 `weatherapi` 각각의 Endpoints 링크를 클릭하면 OpenAPI 문서를 볼 수 있습니다.
1. 대시보드에 나타난 `webapp`의 Endpoints 링크를 클릭하면 웹 앱을 볼 수 있습니다. `/products`와 `/weather` 페이지를 확인해 보세요.
1. 터미널에서 `Ctrl`+`C`를 눌러 .NET Aspire 오케스트레이터를 종료합니다.

## .NET Aspire 오케스트레이터에서 데이터베이스 교체

지금까지 사용하던 데이터베이스를 SQLite에서 PostgreSQL로 교체해 보겠습니다.

### `eShopLite.AppHost` 프로젝트 수정

1. 아래 명령어를 실행시켜 `eShopLite.AppHost`. Add the PostgreSQL package to the `eShopLite.AppHost` project.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet add ./src/eShopLite.AppHost package Aspire.Hosting.PostgreSQL
    ```

1. Add the following content immediately below `src/eShopLite.AppHost/Program.cs` 파일을 열고 `var builder = DistributedApplication.CreateBuilder(args);`:

    ```csharp
    var productsdb = builder.AddPostgres("pg")
                            .WithPgAdmin()
                            .AddDatabase("productsdb");
    ```

   > Add a PostgreSQL database.
   > 
   > - Add a database named `productsdb`: pgAdmin 대시보드를 사용할 수 있도록 설정합니다.
   > - `productsdb`.

1. Modify the following content:

    ```csharp
    // 변경전
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi");
    ```

    ```csharp
    // 변경후
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi")
                            .WithReference(productsdb);
    ```

   > Add the PostgreSQL package to the `ProductApi` 프로젝트에 PostgreSQL 데이터베이스를 추가합니다.
   > 
   > - `.WithReference(productsdb)`: `ProductApi` 프로젝트가 PostgreSQL 데이터베이스를 참조하도록 설정합니다.

### `eShopLite.ProductApi` 프로젝트 수정

1. 아래 명령어를 실행시켜 `eShopLite.ProductApi` project.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet add ./src/eShopLite.ProductApi package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
    ```

1. The `src/eShopLite.ProductApi/appsettings.json` 파일을 열고 `ConnectionStrings` 섹션을 완전히 지웁니다. 이후 `appsettings.json` file should be updated as follows:

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

   > Remove the SQLite database connection string.

1. Open the `src/eShopLite.ProductApi/Program.cs` file and modify it as follows:

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

   > Update to use the PostgreSQL database connection string.
   > 
   > - The name `productsdb`는 `AppHost` corresponds to the database added in the `AppHost` project.

### Running the .NET Aspire Orchestrator

1. Execute the following command to run the .NET Aspire orchestrator:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet watch run --project ./src/eShopLite.AppHost
    ```

1. A web browser will automatically open, displaying the dashboard. The dashboard will show `pg`, `pg-pgadmin`, `productsdb`, `productapi`, `weatherapi`, `webapp` 리소스가 나타나면 성공입니다.

    ![Aspire Dashboard](../../../docs/images/aspire-dashboard-2.png)

   > 경우에 따라 아래와 같이 로그인 화면이 나타날 수 있습니다.
   > 
   > ![Aspire Dashboard Login](../../../docs/images/aspire-dashboard-login.png)
   > 
   > 화살표가 가리키는 링크를 클릭해서 안내에 따라 로그인하면 대시보드를 볼 수 있습니다.

1. 대시보드에 나타난 `pg-pgadmin`의 Endpoints 링크를 클릭하면 PostgreSQL 데이터베이스의 관리자 대시보드 화면을 볼 수 있습니다.
1. 대시보드에 나타난 `productapi`와 `weatherapi` 각각의 Endpoints 링크를 클릭하면 OpenAPI 문서를 볼 수 있습니다.
1. 대시보드에 나타난 `webapp`의 Endpoints 링크를 클릭하면 웹 앱을 볼 수 있습니다. `/products`와 `/weather` 페이지를 확인해 보세요.
1. 터미널에서 `Ctrl`+`C`. Press Ctrl+C to stop the .NET Aspire orchestrator.

---

Congratulations! You've completed the **Container Orchestration with .NET Aspire** practice. Proceed to the next step: [STEP 05: Integration Testing with .NET Aspire](./step-05.md).

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While efforts are made to ensure accuracy, please note that automated translations may contain errors or inaccuracies. The original document in its native language should be regarded as the authoritative source. For critical information, professional human translation is advised. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.