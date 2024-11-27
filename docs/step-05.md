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

## 테스트 프로젝트 생성: AppHost

`eShopLite.AppHost.Tests` 프로젝트를 설치하고 나면 전체 솔루션 구조는 아래와 같이 바뀝니다.

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

1. 아래 명령어를 통해 테스트 프로젝트를 생성하고 솔루션에 포함시킵니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new aspire-nunit -n eShopLite.AppHost.Tests -o test/eShopLite.AppHost.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.AppHost.Tests
    ```

   > 이번 통합 테스트에서는 .NET Aspire용 `dotnet new aspire-nunit` 템플릿을 사용합니다.

1. 아래 명령어를 통해 테스트에 필요한 NuGet 패키지를 테스트 프로젝트에 설치합니다.

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests package FluentAssertions
    dotnet add ./test/eShopLite.AppHost.Tests package Microsoft.Playwright.NUnit
    ```

1. 아래 명령어를 통해 `AppHost` 프로젝트를 참조합니다.

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests reference ./src/eShopLite.AppHost
    ```

1. `test/eShopLite.AppHost.Tests/eShopLite.AppHost.Tests.csproj` 파일을 열고 아래 내용을 수정합니다.

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

1. `</Project>` 바로 위에 다음 `<Target>...</Target>` 노드를 입력해서 프로젝트 빌드 후에 Playwright를 설치하도록 합니다.

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## 테스트 코드 작성: Home Page

1. 아래 명령어를 통해 테스트 프로젝트에 `/` 페이지를 위한 테스트 클래스를 생성합니다.

    ```bash
    # Bash/Zsh
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs -Force
    ```

1. `test/eShopLite.AppHost.Tests/HomePageTests.cs` 파일을 열고 아래와 같이 입력합니다.

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

1. `ContextOptions()` 메서드 바로 아래에 다음 코드를 추가합니다.

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
            await this._app!.DisposeAsync().ConfigureAwait(false);
        }
    ```

   > - `Setup()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행하기 전에 딱 한 번 실행합니다.
   >   - `DistributedApplication` 인스턴스를 생성합니다.
   >   - `ResourceNotificationService` 인스턴스를 생성합니다.
   >   - `DistributedApplication` 인스턴스를 시작합니다.
   >   - `ResourceNotificationService` 인스턴스를 사용하여 각 컨테이너가 실행될 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `DistributedApplication` 인스턴스를 삭제합니다.

1. `SetUp()` 메서드 바로 아래에 다음 테스트 코드를 입력합니다.

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

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Heading1()`: 홈페이지를 방문했을 때, `H1` 엘리먼트를 제대로 렌더링하는지 확인합니다.

1. 테스트 클래스를 저장하고 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 성공적으로 통과했는지 확인합니다.

## 테스트 코드 작성: Product Page

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 홈페이지 테스트 및 [STEP 03: Testcontainers로 통합 테스트하기](./step-03.md)를 참고해서 같은 방식으로 `/products` 페이지를 테스트하는 테스트 클래스를 작성해보세요.

1. 테스트 클래스를 저장하고 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 성공적으로 통과했는지 확인합니다.

## 테스트 코드 작성: Weather Page

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 홈페이지 테스트, Products 페이지 테스트 및 [STEP 03: Testcontainers로 통합 테스트하기](./step-03.md)를 참고해서 같은 방식으로 `/weather` 페이지를 테스트하는 테스트 클래스를 작성해보세요.

1. 테스트 클래스를 저장하고 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 성공적으로 통과했는지 확인합니다.

---

축하합니다! **.NET Aspire로 통합 테스트하기** 실습이 끝났습니다.

---

모든 워크샵 실습이 끝났습니다. 혹시나 실습 중 궁금한 점이나 문제가 발생했다면 다시 한번 [세이브 포인트](../save-points/)를 참고해서 따라해 보세요.
