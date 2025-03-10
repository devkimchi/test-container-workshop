# STEP 03: Integration Testing with Testcontainers

In this step, we will use [Testcontainers](https://dotnet.testcontainers.org/) to perform integration testing for all applications running inside containers.

## Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

Refer to the [STEP 00: Setting up the development environment](./step-00.md) document to verify the installation of each prerequisite.

## Copy Previous Project

You can continue using the app from the previous step, or you can create a fresh copy from the save point using the commands below. If you choose to copy anew, use the following commands.

1. Open the terminal and execute the following commands sequentially to create the working directory and copy the previous project.

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

1. Build the entire project using the following command.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## Create Test Project: Web App

Once the `eShopLite.WebApp.Tests` project is installed, the overall solution structure will look like this:

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

1. Create the test project and include it in the solution using the following command.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit-playwright -n eShopLite.WebApp.Tests -o test/eShopLite.WebApp.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.WebApp.Tests
    ```

   > In this integration test, we will use the Playwright library to perform UI testing as well. Therefore, we will use the `dotnet new nunit-playwright` template.

1. Install the necessary NuGet packages for testing in the test project using the following command.

    ```bash
    dotnet add ./test/eShopLite.WebApp.Tests package FluentAssertions
    dotnet add ./test/eShopLite.WebApp.Tests package TestContainers
    ```

1. Add the `<Target>...</Target>` node in `test/eShopLite.WebApp.Tests/eShopLite.WebApp.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `` to build container images before running tests.

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.webapp -t eshoplite-webapp-test:latest"/>
      </Target>
    
    </Project>
    ```

   > Since the frontend app needs to call all backend APIs, all container images must be built.

1. Similarly, add the `<Target>...</Target>` node to install Playwright after the project is built.

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## Write Test Code: Web App

### Write Test Code: Product Page

1. Create a test class for the `/products` page in the test project using the following commands.

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

1. Open the `test/eShopLite.WebApp.Tests/Components/Pages/ProductsPageTests.cs` file and add the following code.

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

1. Add the following code directly below the `ContextOptions()` method.

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

   > - The `Setup()` method runs only once before running all the test methods in this test class.
   >   - It creates the `INetwork` instance.
   >     - `.WithName(Guid.NewGuid().ToString("D"))`: generates a network with a random name.
   >   - It creates the `ContainerBuilder` instance for the Product API container.
   >     - `.WithImage("eshoplite-productapi-test:latest")`: declares the container image.
   >     - `.WithName("productapi")`: declares the container name.
   >     - `.WithNetwork(this._network)`: declares the network declared above.
   >     - `.WithNetworkAliases("productapi")`: declares the alias of the container.
   >     - `.WithPortBinding(8080, true)`: binds the container's port number to the random port number of the host.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: waits for the container is available to use the 8080 port.
   >   - It creates the `ContainerBuilder` instance for the Web App container.
   >     - `.WithImage("eshoplite-webapp-test:latest")`: declares the container image.
   >     - `.WithNetwork(this._network)`: declares the network declared above.
   >     - `.WithNetworkAliases("webapp")`: declares the alias of the container.
   >     - `.WithPortBinding(8080, true)`: binds the container's port number to the random port number of the host.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: waits for the container is available to use the 8080 port.
   > - The `Teardown()` method runs only once after completing all the test methods in this test class.
   >   - Delete the `INetwork` instance.

1. Add the following code directly below the `SetUp()` method.

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

   > - The `Init()` method runs every time before running each test method in the test class.
   >   - It creates the `IContainer` instance for the Product API.
   >   - It creates the `IContainer` instance for the Web App.
   >   - It creates the network instance.
   >   - It starts the Product API container.
   >   - It starts the Web App container.
   > - The `Cleanup()` method runs every time after running each test method in the test class.
   >   - It stops the Web App container.
   >   - It stops the Product API container.
   >   - It deletes the network.
   >   - It deletes the Web App container.
   >   - It deletes the Product API container.

1. Add the following test code directly below the `Init()` method.

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

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()`: tests whether to render the `Table` element when navigating to `/products`.

1. Add the following code directly below the `Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()` method.

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

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()`: tests whether to render the records fetched from the database when navigating to `/products`.

1. `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()` Add the following code directly below the `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()` method.

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

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Date()`: tests whether to render the product ID value of each record when navigating to `/products`.

1. Save the test class and execute the tests using the following command.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Verify that all tests pass successfully.

### Write Test Code: Weather Page

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> Write a test class for the `/weather` page using the same approach.

1. Save the test class and execute the tests using the following command.

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. Verify that all tests pass successfully.

---

Congratulations! You've completed the **Integration Testing with Testcontainers** exercise. Now, proceed to [STEP 04: Container Orchestration with .NET Aspire](./step-04.md).

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While efforts are made to ensure accuracy, please note that automated translations may include errors or inaccuracies. The original document in its native language should be regarded as the authoritative source. For crucial information, professional human translation is advised. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.