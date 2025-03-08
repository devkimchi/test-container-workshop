# STEP 03: ทดสอบการรวมระบบด้วย Testcontainers

ในขั้นตอนนี้ เราจะใช้ [Testcontainers](https://dotnet.testcontainers.org/) เพื่อทดสอบการรวมระบบของแอปพลิเคชันทั้งหมดที่ทำงานภายในคอนเทนเนอร์

## สิ่งที่ต้องเตรียมล่วงหน้า

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ตรวจสอบการติดตั้งตามรายการที่เตรียมไว้ได้ในเอกสาร [STEP 00: ตั้งค่าพื้นฐานสำหรับการพัฒนา](./step-00.md)

## คัดลอกโปรเจกต์จากขั้นตอนก่อนหน้า

คุณสามารถใช้แอปพลิเคชันจากขั้นตอนก่อนหน้า หรือคัดลอกใหม่จากเซฟพอยต์ด้วยคำสั่งด้านล่างนี้ หากต้องการคัดลอกใหม่ ให้ใช้คำสั่งต่อไปนี้:

1. เปิดเทอร์มินอลแล้วรันคำสั่งตามลำดับเพื่อสร้างไดเรกทอรีสำหรับการทดลองและคัดลอกโปรเจกต์จากขั้นตอนก่อนหน้า:

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

1. รันคำสั่งด้านล่างเพื่อบิลด์โปรเจกต์ทั้งหมด:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## สร้างโปรเจกต์สำหรับการทดสอบ: Web App

เมื่อคุณติดตั้งโปรเจกต์ `eShopLite.WebApp.Tests` แล้ว โครงสร้างของโซลูชันทั้งหมดจะเปลี่ยนเป็นดังนี้:

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

1. ใช้คำสั่งด้านล่างเพื่อสร้างโปรเจกต์สำหรับการทดสอบและเพิ่มเข้าไปในโซลูชัน:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit-playwright -n eShopLite.WebApp.Tests -o test/eShopLite.WebApp.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.WebApp.Tests
    ```

   > สำหรับการทดสอบการรวมระบบครั้งนี้ เราจะใช้ไลบรารี Playwright เพื่อทดสอบ UI ด้วย ดังนั้นจึงใช้เทมเพลต `dotnet new nunit-playwright`

1. ใช้คำสั่งด้านล่างเพื่อติดตั้งแพ็กเกจ NuGet ที่จำเป็นสำหรับการทดสอบเข้าไปในโปรเจกต์ทดสอบ:

    ```bash
    dotnet add ./test/eShopLite.WebApp.Tests package FluentAssertions
    dotnet add ./test/eShopLite.WebApp.Tests package TestContainers
    ```

1. เพิ่มโหนด `<Target>` ในไฟล์ `test/eShopLite.WebApp.Tests/eShopLite.WebApp.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `<Target>...</Target>` เพื่อบิลด์ภาพคอนเทนเนอร์ก่อนรันการทดสอบ:

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.webapp -t eshoplite-webapp-test:latest"/>
      </Target>
    
    </Project>
    ```

   > เนื่องจากแอปฟรอนต์เอนด์ต้องเรียกใช้ API ของแบ็กเอนด์ทั้งหมด เราจึงต้องบิลด์ภาพคอนเทนเนอร์ทั้งหมด

1. เพิ่มโหนด `</Project>` 바로 위에 다음 `<Target>...</Target>` เพื่อให้ติดตั้ง Playwright หลังจากบิลด์โปรเจกต์:

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## เขียนโค้ดสำหรับการทดสอบ: Web App

### เขียนโค้ดสำหรับการทดสอบ: Product Page

1. ใช้คำสั่งด้านล่างเพื่อสร้างคลาสสำหรับการทดสอบหน้า `/products` ในโปรเจกต์ทดสอบ:

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

1. เปิดไฟล์ `test/eShopLite.WebApp.Tests/Components/Pages/ProductsPageTests.cs` แล้วเพิ่มโค้ดต่อไปนี้:

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

1. เพิ่มโค้ดด้านล่างนี้ใต้เมธอด `ContextOptions()`:

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

1. `SetUp()` เพิ่มโค้ดต่อไปนี้ใต้เมธอดนี้:

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

1. `Init()` เพิ่มโค้ดทดสอบต่อไปนี้ใต้เมธอดนี้:

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

1. `Given_PageUrl_When_Invoked_Then_It_Should_Return_Table()` เพิ่มโค้ดต่อไปนี้ใต้เมธอดนี้:

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

1. `Given_PageUrl_When_Invoked_Then_It_Should_Return_TableRows()` เพิ่มโค้ดต่อไปนี้ใต้เมธอดนี้:

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

   > - `Given_PageUrl_When_Invoked_Then_It_Should_Return_Date()`: `/products` 페이지를 방문했을 때, `Table` ตรวจสอบว่าแต่ละเรคคอร์ดในตารางมีค่า Product ID ที่เรนเดอร์อย่างถูกต้อง

1. บันทึกคลาสสำหรับการทดสอบแล้วรันคำสั่งด้านล่างเพื่อรันการทดสอบ:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จหรือไม่

### เขียนโค้ดสำหรับการทดสอบ: Weather Page

> **🚨🚨🚨 ท้าทาย‼️ 🚨🚨🚨**
> 
> ลองเขียนคลาสสำหรับการทดสอบหน้า `/weather` ด้วยวิธีการเดียวกัน

1. บันทึกคลาสสำหรับการทดสอบแล้วรันคำสั่งด้านล่างเพื่อรันการทดสอบ:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จหรือไม่

---

ยินดีด้วย! คุณได้จบการทดลอง **ทดสอบการรวมระบบด้วย Testcontainers** แล้ว ตอนนี้ไปต่อที่ขั้นตอน [STEP 04: ใช้ .NET Aspire สำหรับการจัดการคอนเทนเนอร์](./step-04.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาด้วย AI อัตโนมัติ แม้ว่าเราจะพยายามให้การแปลมีความถูกต้องที่สุด แต่โปรดทราบว่าการแปลด้วยระบบอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้องเกิดขึ้นได้ เอกสารต้นฉบับในภาษาดั้งเดิมควรถูกพิจารณาให้เป็นแหล่งข้อมูลที่น่าเชื่อถือที่สุด สำหรับข้อมูลสำคัญ แนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญมืออาชีพ เราจะไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความผิดที่เกิดจากการใช้การแปลนี้