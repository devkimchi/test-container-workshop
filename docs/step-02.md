# STEP 02: Testcontaienrs로 API 테스트하기

이 단계에서는 API 앱을 [Testcontainers](https://dotnet.testcontainers.org/)를 이용해 컨테이너 안에서 동작하는 애플리케이션을 테스트합니다.

## 사전 준비 사항

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0) 설치
- [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) 설치
- [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/) 설치
- [Visual Studio Code](https://code.visualstudio.com/) 설치

각 사전 준비사항의 설치 여부 확인은 [STEP 00: 개발 환경 설정](./step-00.md) 문서를 참고해주세요.

## 이전 프로젝트 복사

이전 단계에서 사용하던 앱을 그대로 사용해도 좋고, 아래 명령어를 통해 세이브포인트로부터 새롭게 복사해서 사용해도 좋습니다. 새롭게 복사하려면 아래 명령어를 사용하세요.

1. 터미널을 열고 아래 명령어를 차례로 실행시켜 실습 디렉토리를 만들고 이전 프로젝트를 복사합니다.

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

## 테스트 프로젝트 생성: Product API

`eShopLite.ProductApi.Tests` 프로젝트를 설치하고 나면 전체 솔루션 구조는 아래와 같이 바뀝니다.

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
```

1. 아래 명령어를 통해 테스트 프로젝트를 생성하고 솔루션에 포함시킵니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit -n eShopLite.ProductApi.Tests -o test/eShopLite.ProductApi.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.ProductApi.Tests
    ```

1. 아래 명령어를 통해 테스트에 필요한 NuGet 패키지를 테스트 프로젝트에 설치합니다.

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests package FluentAssertions
    dotnet add ./test/eShopLite.ProductApi.Tests package TestContainers
    ```

1. 아래 명령어를 통해 테스트에 필요한 레퍼런스 프로젝트를 테스트 프로젝트에 설치합니다.

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests reference ./src/eShopLite.DataEntities
    ```

1. 아래 명령어를 통해 테스트 프로젝트에 테스트 클래스를 생성합니다.

    ```bash
    mkdir -p $REPOSITORY_ROOT/workshop/test/eShopLite.ProductApi.Tests/Endpoints
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs
    ```

1. `test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs` 파일을 열고 아래와 같이 입력합니다.

    ```csharp
    using System.Net;
    using System.Net.Http.Json;
    
    using DotNet.Testcontainers.Builders;
    using DotNet.Testcontainers.Containers;
    
    using eShopLite.DataEntities;
    
    using FluentAssertions;
    
    namespace eShopLite.ProductApi.Tests.Endpoints;
    
    [TestFixture]
    public class ProductEndpointsTests
    {
        private static HttpClient http = new();
    
        private IContainer? _container;
    }
    ```

1. `private IContainer? _container;`줄 바로 아래에 다음 코드를 입력합니다.

    ```csharp
        [OneTimeSetUp]
        public void Setup()
        {
            this._container = new ContainerBuilder()
                                  .WithImage("eshoplite-productapi-test:latest")
                                  .WithPortBinding(8080, true)
                                  .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))
                                  .Build();
        }
    
        [OneTimeTearDown]
        public async Task Teardown()
        {
            await this._container!.DisposeAsync().ConfigureAwait(false);
        }
    ```

   > - `Setup()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행하기 전에 딱 한 번 실행합니다.
   >   - `IContainer` 인스턴스를 생성합니다.
   >   - `.WithImage("eshoplite-productapi-test:latest")`: 컨테이너를 생성할 때 사용할 이미지를 지정합니다.
   >   - `.WithPortBinding(8080, true)`: 컨테이너의 8080 포트를 호스트의 임의의 포트에 바인딩합니다.
   >   - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: 컨테이너가 8080 포트를 사용할 수 있을 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `IContainer` 인스턴스를 삭제합니다.

1. `SetUp()` 메서드 바로 아래에 다음 코드를 입력합니다.

    ```csharp
        [SetUp]
        public async Task Init()
        {
            await this._container!.StartAsync().ConfigureAwait(false);
        }
    
        [TearDown]
        public async Task Cleanup()
        {
            await this._container!.StopAsync().ConfigureAwait(false);
        }
    ```

   > - `Init()` 메서드는 테스트 클래스의 각 테스트 메서드를 실행하기 전에 실행합니다.
   >   - 컨테이너를 시작합니다.
   > - `Cleanup()` 메서드는 테스트 클래스의 각 테스트 메서드를 실행한 후에 실행합니다.
   >   - 컨테이너를 중지합니다.

1. `Init()` 메서드 바로 아래 다음 테스트 코드를 입력합니다.

    ```csharp
        [Test]
        public async Task Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()
        {
            // Arrange
            var uri = new UriBuilder(Uri.UriSchemeHttp, this._container!.Hostname, this._container!.GetMappedPublicPort(8080), "/api/products").Uri;
    
            // Act
            var response = await http.GetAsync(uri).ConfigureAwait(false);
    
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    ```

   > - `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()`: `GET /api/products` 엔드포인트를 호출했을 때 200 OK 응답을 받는지 확인합니다.

1. `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()` 메서드 바로 아래에 다음 코드를 입력합니다.

    ```csharp
        [Test]
        public async Task Given_Endpoint_When_Invoked_Then_It_Should_Return_Collection()
        {
            // Arrange
            var uri = new UriBuilder(Uri.UriSchemeHttp, this._container!.Hostname, this._container!.GetMappedPublicPort(8080), "/api/products").Uri;
    
            // Act
            var result = await http.GetFromJsonAsync<List<Product>>(uri).ConfigureAwait(false);
    
            // Assert
            result.Should().NotBeNullOrEmpty();
            result.Should().BeOfType<List<Product>>();
        }
    ```

   > - `Given_Endpoint_When_Invoked_Then_It_Should_Return_Collection()`: `GET /api/products` 엔드포인트를 호출했을 때 `Product` 컬렉션을 받는지 확인합니다.

1. 테스트 클래스를 저장하고 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

   아래와 같은 에러메시지가 나옵니다.

    ```text
    Error Message: Docker.DotNet.DockerApiException : Docker API responded with status code=NotFound, response={"message":"pull access denied for e
          shoplite-productapi-test, repository does not exist or may require 'docker login': denied: requested access to the resource is denied"}
    ```

   이 에러는 `eshoplite-productapi-test` 이미지가 없기 때문에 발생합니다.

1. 아래 명령어를 실행시켜 컨테이너 이미지를 빌드합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi-test:latest
    ```

1. 다시 아래 명령어를 통해 테스트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

1. 모든 테스트를 성공적으로 통과했는지 확인합니다.
1. 아래 명령어를 통해 테스트 컨테이너 이미지를 삭제합니다.

    ```bash
    docker rmi eshoplite-productapi-test:latest
    ```

## 테스트 프로젝트 생성: Weather API

`eShopLite.WeatherApi.Tests` 프로젝트를 설치하고 나면 전체 솔루션 구조는 아래와 같이 바뀝니다.

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
```

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `eShopLite.ProductApi.Tests` 테스트 프로젝트와 마찬가지로 `eShopLite.WeatherApi.Tests` 프로젝트를 생성하고 테스트를 작성해 보세요.
>
> 아래 명령어를 통해 테스트를 통과해야 합니다.
> 
>     ```bash
>     cd $REPOSITORY_ROOT/workshop
> 
>     dotnet test ./test/eShopLite.WeatherApi.Tests
>     ```

## API 통합 테스트

1. 아래 명령어를 통해 모든 테스트 프로젝트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 통과했는지 확인합니다.

## 컨테이너 이미지 자동 빌드

컨테이너를 테스트할 때 자동으로 컨테이너 이미지를 빌드합니다.

1. 아래 명령어를 통해 기존의 테스트용 컨테이너 이미지를 모두 삭제합니다.

    ```bash
    docker rmi eshoplite-productapi-test:latest --force
    docker rmi eshoplite-weatherapi-test:latest --force
    ```

1. `test/eShopLite.ProductApi.Tests/eShopLite.ProductApi.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `<Target>...</Target>` 노드를 입력합니다.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. `test/eShopLite.WeatherApi.Tests/eShopLite.WeatherApi.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `<Target>...</Target>` 노드를 입력합니다.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. 아래 명령어를 통해 모든 테스트 프로젝트를 실행합니다.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. 모든 테스트를 통과했는지 확인합니다.

---

축하합니다! **Testcontaienrs로 API 테스트하기** 실습이 끝났습니다. 이제 [STEP 03: Testcontaienrs로 통합 테스트하기](./step-03.md) 단계로 넘어가세요.
