# ขั้นตอนที่ 03: การทดสอบการเชื่อมต่อด้วย Testcontainers

ในขั้นตอนนี้ เราจะใช้ [Testcontainers](https://dotnet.testcontainers.org/) เพื่อทำการทดสอบการเชื่อมต่อสำหรับแอปพลิเคชันทั้งหมดที่ทำงานอยู่ในคอนเทนเนอร์

## สิ่งที่ต้องเตรียม

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ดูเอกสาร [STEP 00: การตั้งค่าสภาพแวดล้อมการพัฒนา](./step-00.md) เพื่อยืนยันการติดตั้งสิ่งที่ต้องเตรียมทั้งหมด

## คัดลอกโปรเจกต์ก่อนหน้า

คุณสามารถใช้แอปพลิเคชันจากขั้นตอนก่อนหน้า หรือสร้างสำเนาใหม่จากจุดบันทึกโดยใช้คำสั่งด้านล่าง หากเลือกที่จะคัดลอกใหม่ ให้ใช้คำสั่งต่อไปนี้

1. เปิด terminal และเรียกใช้คำสั่งต่อไปนี้เรียงตามลำดับเพื่อสร้างไดเรกทอรีงานและคัดลอกโปรเจกต์ก่อนหน้า

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

1. สร้างโปรเจกต์ทั้งหมดโดยใช้คำสั่งต่อไปนี้

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## สร้างโปรเจกต์ทดสอบ: Web App

เมื่อโปรเจกต์ `eShopLite.WebApp.Tests` ถูกติดตั้งแล้ว โครงสร้างของ solution โดยรวมจะมีลักษณะดังนี้:

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

1. สร้างโปรเจกต์ทดสอบและเพิ่มเข้าไปใน solution โดยใช้คำสั่งต่อไปนี้

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit-playwright -n eShopLite.WebApp.Tests -o test/eShopLite.WebApp.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.WebApp.Tests
    ```

   > ในการทดสอบการเชื่อมต่อนี้ เราจะใช้ไลบรารี Playwright เพื่อทำการทดสอบ UI ดังนั้นเราจะใช้เทมเพลต `dotnet new nunit-playwright`

1. ติดตั้งแพ็คเกจ NuGet ที่จำเป็นสำหรับการทดสอบในโปรเจกต์ทดสอบโดยใช้คำสั่งต่อไปนี้

    ```bash
    dotnet add ./test/eShopLite.WebApp.Tests package FluentAssertions
    dotnet add ./test/eShopLite.WebApp.Tests package TestContainers
    ```

1. เพิ่ม `<Target>...</Target>` node in `test/eShopLite.WebApp.Tests/eShopLite.WebApp.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `` เพื่อสร้างภาพคอนเทนเนอร์ก่อนการรันการทดสอบ

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.webapp -t eshoplite-webapp-test:latest"/>
      </Target>
    
    </Project>
    ```

   > เนื่องจากแอป frontend ต้องเรียกใช้ API ของ backend ทั้งหมด ภาพคอนเทนเนอร์ทั้งหมดจึงต้องถูกสร้างขึ้น

1. เพิ่ม node `<Target>...</Target>` ในลักษณะเดียวกันเพื่อทำการติดตั้ง Playwright หลังจากโปรเจกต์ถูกสร้างขึ้น

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## เขียนโค้ดการทดสอบ: Web App

### เขียนโค้ดการทดสอบ: Product Page

1. สร้างคลาสทดสอบสำหรับหน้า `/products` ในโปรเจกต์ทดสอบโดยใช้คำสั่งต่อไปนี้

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

1. เปิดไฟล์ `test/eShopLite.WebApp.Tests/Components/Pages/ProductsPageTests.cs` และเพิ่มโค้ดต่อไปนี้

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

1. เพิ่มโค้ดต่อไปนี้โดยตรงด้านล่างเมธอด `ContextOptions()`

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

   > - เมธอด `Setup()` method runs only once before running all the test methods in this test class.
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

1. Add the following code directly below the `SetUp()`

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

   > - เมธอด `Init()` method runs every time before running each test method in the test class.
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

1. Add the following test code directly below the `Init()`

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

1. Add the following code directly below the `Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()` เมธอด

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

1. `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()` Add the following code directly below the `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()` เมธอด

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

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Date()`: tests whether to render the product ID value of each record when navigating to `/products`

1. บันทึกคลาสทดสอบและรันการทดสอบโดยใช้คำสั่งต่อไปนี้

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

### เขียนโค้ดการทดสอบ: Weather Page

> **🚨🚨🚨 ความท้าทาย‼️ 🚨🚨🚨**
> 
> เขียนคลาสทดสอบสำหรับหน้า `/weather` โดยใช้วิธีการเดียวกัน

1. บันทึกคลาสทดสอบและรันการทดสอบโดยใช้คำสั่งต่อไปนี้

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

---

ยินดีด้วย! คุณได้ทำแบบฝึกหัด **การทดสอบการเชื่อมต่อด้วย Testcontainers** เสร็จสิ้นแล้ว ตอนนี้ไปต่อที่ [STEP 04: การจัดการคอนเทนเนอร์ด้วย .NET Aspire](./step-04.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติด้วย AI แม้ว่าเราจะพยายามอย่างเต็มที่เพื่อให้การแปลมีความถูกต้อง โปรดทราบว่าการแปลโดยระบบอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาต้นทางควรถือเป็นแหล่งข้อมูลที่เชื่อถือได้ สำหรับข้อมูลสำคัญ ขอแนะนำให้ใช้บริการแปลภาษามืออาชีพโดยมนุษย์ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่ผิดพลาดอันเกิดจากการใช้การแปลนี้