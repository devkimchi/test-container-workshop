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

   > - The `Setup()` method is run only once before running all the test in this class.
   >   - It creates an `IContainer` instance.
   >     - `.WithImage("eshoplite-productapi-test:latest")`: declares the container image.
   >     - `.WithPortBinding(8080, true)`: binds the container's 8080 port to a random one on the host.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: waits until the container is available to use the 8080 port.
   > - The `Teardown()` method is run only once after completing all the tests in this class.
   >   - It deletes the `IContainer` instance.

1. Right below the `SetUp()` method, add the following `Init()` and `Cleanup()` methods.

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

   > - The `Init()` method runs before running each test method.
   >   - Starts the container.
   > - The `Cleanup()` method runs after completing each test method.
   >   - Stops the container.

1. Right after the `Init()` method, add the following test codes.

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

   > - `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()`: checks whether to receive the 200 (OK) response when calling the `GET /api/products` endpoint.

1. Right after the `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()` method, add the following test codes.

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

   > - `Given_Endpoint_When_Invoked_Then_It_Should_Return_Collection()`: checks whether to receive the list of `Product` when calling the `GET /api/products` endpoint.

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
> Like the `eShopLite.ProductApi.Tests` projects, create a test project, `eShopLite.WeatherApi.Tests`.

## Writing Test Codes: Weather API

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> Like `eShopLite.ProductApi.Tests` projects, create a test project, `eShopLite.WeatherApi.Tests` and write tests.
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

1. Add the `<Target>...</Target>` node just above `</Project>` to the `test/eShopLite.ProductApi.Tests/eShopLite.ProductApi.Tests.csproj` file.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. Add the `<Target>...</Target>` node just above `</Project>` to the `test/eShopLite.WeatherApi.Tests/eShopLite.WeatherApi.Tests.csproj` file.

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