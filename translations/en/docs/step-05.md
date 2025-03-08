# STEP 05: Integration Testing with .NET Aspire

In this step, we will use [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) to perform integration testing for the entire app.

## Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

To verify that all prerequisites are installed, refer to the [STEP 00: Setting Up the Development Environment](./step-00.md) document.

## Copying the Previous Project

You can continue using the app from the previous step or copy it anew from a save point using the commands below. To copy it afresh, use the following commands:

1. Open the terminal and execute the following commands sequentially to create a practice directory and copy the previous project.

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

1. Build the entire project using the command below.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## Creating a Test Project: AppHost

Once the `eShopLite.AppHost.Tests` project is set up, the overall solution structure will look like this:

```text
eShopLite
├── src
│   ├── eShopLite.AppHost
│   │   ├── eShopLite.WebApp
│   │   ├── eShopLite.ProductApi
│   │   └── eShopLite.WeatherApi
│   ├── eShopLite.ServiceDefaults
│   ├── eShopLite.WebApp
│   │   ├── eShopLite.DataEntities
│   │   └── eShopLite.ServiceDefaults
│   ├── eShopLite.WeatherApi
│   │   ├── eShopLite.DataEntities
│   │   └── eShopLite.ServiceDefaults
│   └── eShopLite.ProductApi
│       ├── eShopLite.ProductData
│       │   └── eShopLite.DataEntities
│       └── eShopLite.ServiceDefaults
└── test
    └── eShopLite.AppHost.Tests
        └── eShopLite.AppHost
```

1. Use the following command to create the test project and add it to the solution.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new aspire-nunit -n eShopLite.AppHost.Tests -o test/eShopLite.AppHost.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.AppHost.Tests
    ```

   > For this integration test, we will use the `.NET Aspire` template `dotnet new aspire-nunit`.

1. Install the required NuGet packages for testing into the test project using the following command.

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests package FluentAssertions
    dotnet add ./test/eShopLite.AppHost.Tests package Microsoft.Playwright.NUnit
    ```

1. Add a reference to the `AppHost` project using the command below.

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests reference ./src/eShopLite.AppHost
    ```

1. Open the `test/eShopLite.AppHost.Tests/eShopLite.AppHost.Tests.csproj` file and modify it as follows:

    ```xml
    <!-- 변경전 -->
      <ItemGroup>
        <Using Include="System.Net" />
        <Using Include="Microsoft.Extensions.DependencyInjection" />
        <Using Include="Aspire.Hosting.ApplicationModel" />
        <Using Include="Aspire.Hosting.Testing" />
        <Using Include="NUnit.Framework" />
      </ItemGroup>
    ```

    ```xml
    <!-- 변경후 -->
      <ItemGroup>
        <Using Include="System.Net" />
        <Using Include="System.Threading.Tasks" />
        <Using Include="Aspire.Hosting.ApplicationModel" />
        <Using Include="Aspire.Hosting.Testing" />
        <Using Include="Microsoft.Extensions.DependencyInjection" />
        <Using Include="Microsoft.Playwright.NUnit" />
        <Using Include="NUnit.Framework" />
      </ItemGroup>
    ```

1. Add the `</Project>` 바로 위에 다음 `<Target>...</Target>` node to install Playwright after the project build.

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## Writing Test Code: Home Page

1. Use the following command to create a test class for the `/` page in the test project.

    ```bash
    # Bash/Zsh
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs -Force
    ```

1. Open the `test/eShopLite.AppHost.Tests/HomePageTests.cs` file and input the following code:

    ```csharp
    using Aspire.Hosting;
    
    using FluentAssertions;
    
    using Microsoft.Playwright;
    
    namespace eShopLite.AppHost.Tests;
    
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class HomePageTests : PageTest
    {
        private DistributedApplication? _app;
        private ResourceNotificationService? _resource;
    
        public override BrowserNewContextOptions ContextOptions() => new()
        {
            IgnoreHTTPSErrors = true,
        };
    }
    ```

1. Add the following code directly below the `ContextOptions()` method.

    ```csharp
        [OneTimeSetUp]
        public async Task Setup()
        {
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.eShopLite_AppHost>().ConfigureAwait(false);
            appHost.Services.ConfigureHttpClientDefaults(builder => builder.AddStandardResilienceHandler());
    
            this._app = await appHost.BuildAsync().ConfigureAwait(false);
    
            this._resource = this._app.Services.GetRequiredService<ResourceNotificationService>();
            await this._app.StartAsync().ConfigureAwait(false);
    
            await this._resource!.WaitForResourceAsync("pg-pgadmin", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            await this._resource!.WaitForResourceAsync("productapi", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            await this._resource!.WaitForResourceAsync("weatherapi", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
            await this._resource!.WaitForResourceAsync("webapp", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        }
    
        [OneTimeTearDown]
        public async Task Teardown()
        {
            this._resource!.Dispose();
            await this._app!.DisposeAsync().ConfigureAwait(false);
        }
    ```

   > - Add the `Setup()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행하기 전에 딱 한 번 실행합니다.
   >   - `DistributedApplication` 인스턴스를 생성합니다.
   >   - `ResourceNotificationService` 인스턴스를 생성합니다.
   >   - `DistributedApplication` 인스턴스를 시작합니다.
   >   - `ResourceNotificationService` 인스턴스를 사용하여 각 컨테이너가 실행될 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `DistributedApplication` 인스턴스를 삭제합니다.

1. `SetUp()` method and input the following test code:

    ```csharp
        [Test]
        public async Task Given_PageUrl_When_Invoked_Then_It_Should_Return_Heading1()
        {
            // Arrange
            var uri = this._app!.GetEndpoint("webapp").ToString();
            await Page.GotoAsync(uri);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
            // Act
            var h1 = Page.Locator("h1");
            var content = await h1.TextContentAsync().ConfigureAwait(false);
    
            // Assert
            content.Should().Be("Hello, world!");
        }
    ```

   > - Verify that the `Given_PageUrl_When_Invoked_Then_It_Should_Return_Heading1()`: 홈페이지를 방문했을 때, `H1` element renders correctly.

1. Save the test class and execute the tests using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Ensure that all tests pass successfully.

## Writing Test Code: Product Page

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> Refer to the Home Page test above and [STEP 03: Integration Testing with Testcontainers](./step-03.md) to create a test class for the `/products` page in a similar way.

1. Save the test class and execute the tests using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Ensure that all tests pass successfully.

## Writing Test Code: Weather Page

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> Refer to the Home Page test, Product Page test, and [STEP 03: Integration Testing with Testcontainers](./step-03.md) to create a test class for the `/weather` page in a similar way.

1. Save the test class and execute the tests using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Ensure that all tests pass successfully.

---

Congratulations! You have completed the **Integration Testing with .NET Aspire** exercise.

---

All workshop exercises are now complete. If you have any questions or encounter issues during the exercises, feel free to refer back to the [Save Points](../../../save-points) and try again.

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While we strive for accuracy, please note that automated translations may contain errors or inaccuracies. The original document in its native language should be regarded as the authoritative source. For critical information, professional human translation is advised. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.