# STEP 05: ทดสอบการรวมระบบด้วย .NET Aspire

ในขั้นตอนนี้ เราจะใช้ [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) เพื่อทดสอบการรวมระบบของแอปทั้งหมด

## สิ่งที่ต้องเตรียมล่วงหน้า

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ตรวจสอบการติดตั้งตามรายการข้างต้นได้ในเอกสาร [STEP 00: ตั้งค่าพื้นฐานสำหรับการพัฒนา](./step-00.md)

## คัดลอกโปรเจกต์จากขั้นตอนก่อนหน้า

คุณสามารถใช้แอปที่เคยสร้างไว้ในขั้นตอนก่อนหน้า หรือจะคัดลอกใหม่จากเซฟพอยต์โดยใช้คำสั่งด้านล่างนี้ก็ได้ หากต้องการคัดลอกใหม่ ให้ใช้คำสั่งดังต่อไปนี้:

1. เปิดเทอร์มินอลและรันคำสั่งด้านล่างตามลำดับเพื่อสร้างไดเรกทอรีสำหรับการฝึก และคัดลอกโปรเจกต์ก่อนหน้า

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

1. รันคำสั่งด้านล่างเพื่อบิลด์โปรเจกต์ทั้งหมด

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## สร้างโปรเจกต์ทดสอบ: AppHost

หลังจากติดตั้งโปรเจกต์ `eShopLite.AppHost.Tests` โครงสร้างของโซลูชันทั้งหมดจะเปลี่ยนเป็นดังนี้:

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

1. ใช้คำสั่งด้านล่างเพื่อสร้างโปรเจกต์ทดสอบและเพิ่มเข้าไปในโซลูชัน

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new aspire-nunit -n eShopLite.AppHost.Tests -o test/eShopLite.AppHost.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.AppHost.Tests
    ```

   > ในการทดสอบการรวมระบบครั้งนี้ เราจะใช้เทมเพลต `dotnet new aspire-nunit` สำหรับ .NET Aspire

1. ติดตั้ง NuGet แพ็กเกจที่จำเป็นสำหรับการทดสอบลงในโปรเจกต์ทดสอบโดยใช้คำสั่งด้านล่าง

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests package FluentAssertions
    dotnet add ./test/eShopLite.AppHost.Tests package Microsoft.Playwright.NUnit
    ```

1. ใช้คำสั่งด้านล่างเพื่อเพิ่มการอ้างอิงไปยังโปรเจกต์ `AppHost`

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests reference ./src/eShopLite.AppHost
    ```

1. เปิดไฟล์ `test/eShopLite.AppHost.Tests/eShopLite.AppHost.Tests.csproj` และแก้ไขเนื้อหาตามด้านล่าง

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

1. เพิ่มโหนด `</Project>` 바로 위에 다음 `<Target>...</Target>` เพื่อให้ติดตั้ง Playwright หลังจากการบิลด์โปรเจกต์

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## เขียนโค้ดสำหรับการทดสอบ: Home Page

1. ใช้คำสั่งด้านล่างเพื่อสร้างคลาสทดสอบสำหรับหน้า `/`

    ```bash
    # Bash/Zsh
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs -Force
    ```

1. เปิดไฟล์ `test/eShopLite.AppHost.Tests/HomePageTests.cs` และเพิ่มโค้ดตามด้านล่าง

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

1. เพิ่มโค้ดด้านล่างเข้าไปใต้เมธอด `ContextOptions()`

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

   > - เพิ่มโค้ดสำหรับเมธอด `Setup()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행하기 전에 딱 한 번 실행합니다.
   >   - `DistributedApplication` 인스턴스를 생성합니다.
   >   - `ResourceNotificationService` 인스턴스를 생성합니다.
   >   - `DistributedApplication` 인스턴스를 시작합니다.
   >   - `ResourceNotificationService` 인스턴스를 사용하여 각 컨테이너가 실행될 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `DistributedApplication` 인스턴스를 삭제합니다.

1. `SetUp()` และเพิ่มโค้ดสำหรับการทดสอบดังนี้:

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

   > - ทดสอบ `Given_PageUrl_When_Invoked_Then_It_Should_Return_Heading1()`: 홈페이지를 방문했을 때, `H1` เพื่อยืนยันว่าเรนเดอร์เอลิเมนต์ได้ถูกต้อง

1. บันทึกคลาสทดสอบและรันคำสั่งด้านล่างเพื่อรันการทดสอบ

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบให้แน่ใจว่าการทดสอบทั้งหมดผ่านสำเร็จ

## เขียนโค้ดสำหรับการทดสอบ: Product Page

> **🚨🚨🚨 ท้าทาย‼️ 🚨🚨🚨**
> 
> อ้างอิงจากการทดสอบหน้าโฮมเพจด้านบนและ [STEP 03: ทดสอบการรวมระบบด้วย Testcontainers](./step-03.md) เพื่อเขียนคลาสทดสอบสำหรับหน้า `/products` ด้วยวิธีการเดียวกัน

1. บันทึกคลาสทดสอบและรันคำสั่งด้านล่างเพื่อรันการทดสอบ

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบให้แน่ใจว่าการทดสอบทั้งหมดผ่านสำเร็จ

## เขียนโค้ดสำหรับการทดสอบ: Weather Page

> **🚨🚨🚨 ท้าทาย‼️ 🚨🚨🚨**
> 
> อ้างอิงจากการทดสอบหน้าโฮมเพจ การทดสอบหน้า Products และ [STEP 03: ทดสอบการรวมระบบด้วย Testcontainers](./step-03.md) เพื่อเขียนคลาสทดสอบสำหรับหน้า `/weather` ด้วยวิธีการเดียวกัน

1. บันทึกคลาสทดสอบและรันคำสั่งด้านล่างเพื่อรันการทดสอบ

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบให้แน่ใจว่าการทดสอบทั้งหมดผ่านสำเร็จ

---

ยินดีด้วย! คุณได้เสร็จสิ้นการฝึก **ทดสอบการรวมระบบด้วย .NET Aspire**

---

การฝึกทั้งหมดในเวิร์กชอปนี้เสร็จสิ้นแล้ว หากมีข้อสงสัยหรือปัญหาเกิดขึ้นระหว่างการฝึก สามารถกลับไปดู [เซฟพอยต์](../../../save-points) เพื่อทำตามอีกครั้ง

**คำชี้แจง**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติที่ขับเคลื่อนด้วย AI แม้ว่าเราจะพยายามให้การแปลมีความถูกต้องมากที่สุด แต่โปรดทราบว่าการแปลอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้องเกิดขึ้นได้ เอกสารต้นฉบับในภาษาต้นทางควรถือเป็นแหล่งข้อมูลที่ถูกต้องที่สุด สำหรับข้อมูลที่มีความสำคัญ ขอแนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญที่เป็นมนุษย์ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความผิดที่เกิดขึ้นจากการใช้การแปลนี้