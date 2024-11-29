# STEP 03: Testcontainers로 통합 테스트하기

이 단계에서는 [Testcontainers](https://dotnet.testcontainers.org/)를 이용해 컨테이너 안에서 동작하는 모든 애플리케이션을 통합 테스트합니다.

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

    mkdir -p workshop && cp -a save-points/step-02/. workshop/
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    cd $REPOSITORY_ROOT

    New-Item -Type Directory -Path workshop -Force && Copy-Item -Path ./save-points/step-02/* -Destination ./workshop -Recurse -Force
    ```

1. 아래 명령어를 통해 전체 프로젝트를 빌드합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## 테스트 프로젝트 생성: Web App

`eShopLite.WebApp.Tests` 프로젝트를 설치하고 나면 전체 솔루션 구조는 아래와 같이 바뀝니다.

```text
eShopLite
├── src
│   ├── eShopLite.WebApp
│   │   └── eShopLite.DataEntities
│   ├── eShopLite.WeatherApi
│   │   └── eShopLite.DataEntities
│   └── eShopLite.ProductApi
│       └── eShopLite.ProductData
│           └── eShopLite.DataEntities
└── test
    └── eShopLite.ProductApi.Tests
        └── eShopLite.DataEntities
    └── eShopLite.WeatherApi.Tests
        └── eShopLite.DataEntities
    └── eShopLite.WebApp.Tests
```

1. 아래 명령어를 통해 테스트 프로젝트를 생성하고 솔루션에 포함시킵니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit-playwright -n eShopLite.WebApp.Tests -o test/eShopLite.WebApp.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.WebApp.Tests
    ```

   > 이번 통합 테스트에서는 Playwright 라이브러리를 사용해 UI 테스트까지 함께 수행합니다. 따라서 `dotnet new nunit-playwright` 템플릿을 사용합니다.

1. 아래 명령어를 통해 테스트에 필요한 NuGet 패키지를 테스트 프로젝트에 설치합니다.

    ```bash
    dotnet add ./test/eShopLite.WebApp.Tests package FluentAssertions
    dotnet add ./test/eShopLite.WebApp.Tests package TestContainers
    ```

1. `test/eShopLite.WebApp.Tests/eShopLite.WebApp.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `<Target>...</Target>` 노드를 입력해서 테스트 실행 전에 컨테이너 이미지를 빌드하도록 합니다.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.webapp -t eshoplite-webapp-test:latest"/>
      </Target>
    
    </Project>
    ```

   > 프론트엔드 앱은 모든 백엔드 API를 호출해야 하므로 모든 컨테이너 이미지를 빌드해야 합니다.

1. 마찬가지로 `</Project>` 바로 위에 다음 `<Target>...</Target>` 노드를 입력해서 프로젝트 빌드 후에 Playwright를 설치하도록 합니다.

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## 테스트 코드 작성: Web App

### 테스트 코드 작성: Product Page

1. 아래 명령어를 통해 테스트 프로젝트에 `/products` 페이지를 위한 테스트 클래스를 생성합니다.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop/test/eShopLite.WebApp.Tests/Components/Pages
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.WebApp.Tests/Components/Pages/ProductsPageTests.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop/test/eShopLite.WebApp.Tests/Components/Pages -Force
    New-Item -Type File -Path $REPOSITORY_ROOT/workshop/test/eShopLite.WebApp.Tests/Components/Pages/ProductsPageTests.cs -Force
    ```

1. `test/eShopLite.WebApp.Tests/Components/Pages/ProductsPageTests.cs` 파일을 열고 아래와 같이 입력합니다.

    ```csharp
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Containers;
    using DotNet.Testcontainers.Networks;
    
    using FluentAssertions;
    
    using Microsoft.Playwright;
    
    namespace eShopLite.WebApp.Tests.Components.Pages;
    
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class ProductsPageTests : PageTest
    {
        private INetwork? _network;
        private ContainerBuilder? _productApiContainerBuilder;
        private ContainerBuilder? _webAppContainerBuilder;
        private IContainer? _productApiContainer;
        private IContainer? _webAppContainer;
    
        public override BrowserNewContextOptions ContextOptions() => new()
        {
            IgnoreHTTPSErrors = true,
        };
    }
    ```

1. `ContextOptions()` 메서드 바로 아래에 다음 코드를 추가합니다.

    ```csharp
        [OneTimeSetUp]
        public void Setup()
        {
            this._network = new NetworkBuilder()
                                .WithName(Guid.NewGuid().ToString("D"))
                                .Build();
    
            this._productApiContainerBuilder = new ContainerBuilder()
                                                   .WithImage("eshoplite-productapi-test:latest")
                                                   .WithName("productapi")
                                                   .WithNetwork(this._network)
                                                   .WithNetworkAliases("productapi")
                                                   .WithPortBinding(8080, true)
                                                   .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080));
    
            this._webAppContainerBuilder = new ContainerBuilder()
                                               .WithImage("eshoplite-webapp-test:latest")
                                               .WithNetwork(this._network)
                                               .WithNetworkAliases("webapp")
                                               .WithPortBinding(8080, true)
                                               .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080));
        }
    
        [OneTimeTearDown]
        public async Task Teardown()
        {
            await this._network!.DisposeAsync().ConfigureAwait(false);
        }
    ```

   > - `Setup()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행하기 전에 딱 한 번 실행합니다.
   >   - `INetwork` 인스턴스를 생성합니다.
   >     - `.WithName(Guid.NewGuid().ToString("D"))`: 임의의 이름을 가진 네트워크를 생성합니다.
   >   - Product API 컨테이너를 위한 `ContainerBuilder` 인스턴스를 생성합니다.
   >     - `.WithImage("eshoplite-productapi-test:latest")`: 컨테이너를 생성할 때 사용할 이미지를 지정합니다.
   >     - `.WithName("productapi")`: 컨테이너의 이름을 지정합니다.
   >     - `.WithNetwork(this._network)`: 컨테이너를 생성할 때 앞서 생성한 네트워크를 지정합니다.
   >     - `.WithNetworkAliases("productapi")`: 컨테이너의 네트워크 별칭을 지정합니다.
   >     - `.WithPortBinding(8080, true)`: 컨테이너의 8080 포트를 호스트의 임의의 포트에 바인딩합니다.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: 컨테이너가 8080 포트를 사용할 수 있을 때까지 대기합니다.
   >   - Web App 컨테이너를 위한 `ContainerBuilder` 인스턴스를 생성합니다.
   >     - `.WithImage("eshoplite-webapp-test:latest")`: 컨테이너를 생성할 때 사용할 이미지를 지정합니다.
   >     - `.WithNetwork(this._network)`: 컨테이너를 생성할 때 앞서 생성한 네트워크를 지정합니다.
   >     - `.WithNetworkAliases("webapp")`: 컨테이너의 네트워크 별칭을 지정합니다.
   >     - `.WithPortBinding(8080, true)`: 컨테이너의 8080 포트를 호스트의 임의의 포트에 바인딩합니다.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: 컨테이너가 8080 포트를 사용할 수 있을 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `INetwork` 인스턴스를 삭제합니다.

1. `SetUp()` 메서드 바로 아래에 다음 코드를 입력합니다.

    ```csharp
        [SetUp]
        public async Task Init()
        {
            this._productApiContainer = this._productApiContainerBuilder!.Build();
            this._webAppContainer = this._webAppContainerBuilder!.Build();
    
            await this._network!.CreateAsync().ConfigureAwait(false);
    
            await this._productApiContainer!.StartAsync().ConfigureAwait(false);
            await this._webAppContainer!.StartAsync().ConfigureAwait(false);
        }
    
        [TearDown]
        public async Task Cleanup()
        {
            await this._webAppContainer!.StopAsync().ConfigureAwait(false);
            await this._productApiContainer!.StopAsync().ConfigureAwait(false);
            await this._network!.DeleteAsync().ConfigureAwait(false);
    
            await this._webAppContainer!.DisposeAsync().ConfigureAwait(false);
            await this._productApiContainer!.DisposeAsync().ConfigureAwait(false);
        }
    ```

   > - `Init()` 메서드는 테스트 클래스의 각 테스트 메서드를 실행하기 전에 실행합니다.
   >   - Product API를 위한 `IContainer` 인스턴스를 생성합니다.
   >   - Web App을 위한 `IContainer` 인스턴스를 생성합니다.
   >   - 네트워크를 생성합니다.
   >   - Product API 컨테이너를 시작합니다.
   >   - Web App 컨테이너를 시작합니다.
   > - `Cleanup()` 메서드는 테스트 클래스의 각 테스트 메서드를 실행한 후에 실행합니다.
   >   - Web App 컨테이너를 중지합니다.
   >   - Product API 컨테이너를 중지합니다.
   >   - 네트워크를 삭제합니다.
   >   - Web App 컨테이너를 삭제합니다.
   >   - Product API 컨테이너를 삭제합니다.

1. `Init()` 메서드 바로 아래 다음 테스트 코드를 입력합니다.

    ```csharp
        [Test]
        public async Task Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()
        {
            // Arrange
            var uri = new UriBuilder(Uri.UriSchemeHttp, this._webAppContainer!.Hostname, this._webAppContainer!.GetMappedPublicPort(8080), "/products").Uri.ToString();
            await Page.GotoAsync(uri);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
            // Act
            var table = Page.Locator("table.table");
            var content = await table.TextContentAsync().ConfigureAwait(false);
    
            // Assert
            table.Should().NotBeNull();
            content.Should().NotBeNullOrEmpty();
        }
    ```

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()`: `/products` 페이지를 방문했을 때, `Table` 엘리먼트를 제대로 렌더링하는지 확인합니다.

1. `Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()` 메서드 바로 아래에 다음 코드를 입력합니다.

    ```csharp
        [Test]
        public async Task Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()
        {
            // Arrange
            var uri = new UriBuilder(Uri.UriSchemeHttp, this._webAppContainer!.Hostname, this._webAppContainer!.GetMappedPublicPort(8080), "/products").Uri.ToString();
            await Page.GotoAsync(uri);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
            // Act
            var table = Page.Locator("table.table");
            var trs = table.Locator("tbody").Locator("tr");
            var count = await trs.CountAsync().ConfigureAwait(false);
    
            // Assert
            count.Should().Be(9);
        }
    ```

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()`: `/products` 페이지를 방문했을 때, `Table` 엘리먼트가 데이터베이스에서 호출한 레코드를 제대로 렌더링하는지 확인합니다.

1. `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()` 메서드 바로 아래에 다음 코드를 입력합니다.

    ```csharp
        [Test]
        public async Task Given_PageUrl_When_Invoked_Then_It_Should_Return_Date()
        {
            // Arrange
            var uri = new UriBuilder(Uri.UriSchemeHttp, this._webAppContainer!.Hostname, this._webAppContainer!.GetMappedPublicPort(8080), "/products").Uri.ToString();
            await Page.GotoAsync(uri);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    
            // Act
            var table = Page.Locator("table.table");
            var trs = table.Locator("tbody").Locator("tr");
            var first = await trs.First.Locator("td").First.TextContentAsync().ConfigureAwait(false);
            var last = await trs.Last.Locator("td").First.TextContentAsync().ConfigureAwait(false);
    
            // Assert
            first.Should().Be("1");
            last.Should().Be("9");
        }
    ```

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Date()`: `/products` 페이지를 방문했을 때, `Table`의 각 레코드별로 Product ID 값을 제대로 렌더링하는지 확인합니다.

1. 테스트 클래스를 저장하고 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 성공적으로 통과했는지 확인합니다.

### 테스트 코드 작성: Weather Page

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 같은 방식으로 `/weather` 페이지를 테스트하는 테스트 클래스를 작성해보세요.

1. 테스트 클래스를 저장하고 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 성공적으로 통과했는지 확인합니다.

---

축하합니다! **Testcontainers로 통합 테스트하기** 실습이 끝났습니다. 이제 [STEP 04: .NET Aspire로 컨테이너 오케스트레이션하기](./step-04.md) 단계로 넘어가세요.
