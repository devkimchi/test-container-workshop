# STEP 02: Testing APIs with Testcontainers

In this step, we will use [Testcontainers](https://dotnet.testcontainers.org/) to test an API application running inside a container.

## Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

Refer to the [STEP 00: Setting up the Development Environment](./step-00.md) document to ensure all prerequisites are installed.

## Copying the Previous Project

You can continue using the app from the previous step or create a fresh copy from the save point using the commands below. To create a new copy, use the following commands:

1. Open the terminal and execute the commands below to create the practice directory and copy the previous project.

    ```bash
    # Bash/Zsh
    REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
    cd $REPOSITORY_ROOT

    mkdir -p workshop && cp -a save-points/step-01/. workshop/
    ```

    ```powershell
    # PowerShell
    $REPOSITORY_ROOT = git rev-parse --show-toplevel
    cd $REPOSITORY_ROOT

    New-Item -Type Directory -Path workshop -Force && Copy-Item -Path ./save-points/step-01/* -Destination ./workshop -Recurse -Force
    ```

1. Build the entire project using the following command.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## Creating the Test Project: Product API

After adding the `eShopLite.ProductApi.Tests` project, the overall solution structure will look like this:

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

1. Use the following command to create a test project and add it to the solution.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit -n eShopLite.ProductApi.Tests -o test/eShopLite.ProductApi.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.ProductApi.Tests
    ```

1. Install the required NuGet packages for testing in the test project using the command below.

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests package FluentAssertions
    dotnet add ./test/eShopLite.ProductApi.Tests package TestContainers
    ```

1. Add the necessary reference project to the test project using the following command.

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests reference ./src/eShopLite.DataEntities
    ```

## Writing Test Code: Product API

1. Create a test class in the test project using the following commands.

    ```bash
    # Bash/Zsh
    mkdir -p $REPOSITORY_ROOT/workshop/test/eShopLite.ProductApi.Tests/Endpoints
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type Directory -Path $REPOSITORY_ROOT/workshop/test/eShopLite.ProductApi.Tests/Endpoints -Force
    New-Item -Type File -Path $REPOSITORY_ROOT/workshop/test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs -Force
    ```

1. Open the `test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs` file and input the following code:

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

1. Directly below the line `private IContainer? _container;`, add the following code:

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

   > - Add the following code directly below the `Setup()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행하기 전에 딱 한 번 실행합니다.
   >   - `IContainer` 인스턴스를 생성합니다.
   >     - `.WithImage("eshoplite-productapi-test:latest")`: 컨테이너를 생성할 때 사용할 이미지를 지정합니다.
   >     - `.WithPortBinding(8080, true)`: 컨테이너의 8080 포트를 호스트의 임의의 포트에 바인딩합니다.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: 컨테이너가 8080 포트를 사용할 수 있을 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `IContainer` 인스턴스를 삭제합니다.

1. `SetUp()` method.

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

   > - Insert the following test code directly below the `Init()` 메서드는 테스트 클래스의 각 테스트 메서드를 실행하기 전에 실행합니다.
   >   - 컨테이너를 시작합니다.
   > - `Cleanup()` 메서드는 테스트 클래스의 각 테스트 메서드를 실행한 후에 실행합니다.
   >   - 컨테이너를 중지합니다.

1. `Init()` method.

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

   > - Add the following code directly below the `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()`: `GET /api/products` 엔드포인트를 호출했을 때 200 OK 응답을 받는지 확인합니다.

1. `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()` method.

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

   > - Verify that the `Given_Endpoint_When_Invoked_Then_It_Should_Return_Collection()`: `GET /api/products` 엔드포인트를 호출했을 때 `Product` collection is returned.

1. Save the test class and run the following command to execute the test.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

   You will encounter the following error message:

    ```text
    Error Message: Docker.DotNet.DockerApiException : Docker API responded with status code=NotFound, response={"message":"pull access denied for e
          shoplite-productapi-test, repository does not exist or may require 'docker login': denied: requested access to the resource is denied"}
    ```

   This error occurs because the `eshoplite-productapi-test` image does not exist.

1. Build the container image using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi-test:latest
    ```

1. Re-run the test using the command below:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

1. Verify that all tests have successfully passed.
1. Delete the test container image using the following command:

    ```bash
    docker rmi eshoplite-productapi-test:latest
    ```

## Creating the Test Project: Weather API

After adding the `eShopLite.WeatherApi.Tests` project, the overall solution structure will look like this:

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

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> Create and write tests for the `eShopLite.ProductApi.Tests` 테스트 프로젝트와 마찬가지로 `eShopLite.WeatherApi.Tests` 프로젝트를 생성해 보세요.

## 테스트 코드 작성: Weather API

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `eShopLite.ProductApi.Tests` 테스트 프로젝트와 마찬가지로 `eShopLite.WeatherApi.Tests` projects mentioned above.
>
> Use the following command to ensure the tests pass:
> 
>     ```bash
>     cd $REPOSITORY_ROOT/workshop
> 
>     dotnet test ./test/eShopLite.WeatherApi.Tests
>     ```

## API Integration Testing

1. Execute all test projects using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Verify that all tests have passed.

## Automatic Container Image Build

Automatically build container images during testing.

1. Delete all existing test container images using the following command:

    ```bash
    docker rmi eshoplite-productapi-test:latest --force
    docker rmi eshoplite-weatherapi-test:latest --force
    ```

1. Add the `<Target>...</Target>` node to the `test/eShopLite.ProductApi.Tests/eShopLite.ProductApi.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `` file.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. Add the `<Target>...</Target>` node to the `test/eShopLite.WeatherApi.Tests/eShopLite.WeatherApi.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `` file.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. Execute all test projects using the following command:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Verify that all tests have passed.

---

Congratulations! You have completed the **Testing APIs with Testcontainers** practice. Now proceed to [STEP 03: Integration Testing with Testcontainers](./step-03.md).

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While we strive for accuracy, please note that automated translations may contain errors or inaccuracies. The original document in its native language should be regarded as the authoritative source. For critical information, professional human translation is recommended. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.