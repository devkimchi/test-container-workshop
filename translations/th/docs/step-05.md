# STEP 05: การทดสอบการเชื่อมต่อด้วย .NET Aspire

ในขั้นตอนนี้ เราจะใช้ [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) เพื่อทำการทดสอบการเชื่อมต่อสำหรับแอปทั้งหมด

## ข้อกำหนดเบื้องต้น

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

เพื่อยืนยันว่าข้อกำหนดเบื้องต้นทั้งหมดถูกติดตั้งแล้ว ให้ดูที่เอกสาร [STEP 00: Setting Up the Development Environment](./step-00.md)

## การคัดลอกโปรเจกต์ก่อนหน้า

คุณสามารถใช้งานแอปจากขั้นตอนก่อนหน้าได้ หรือคัดลอกใหม่จากจุดบันทึกโดยใช้คำสั่งด้านล่างนี้ หากต้องการคัดลอกใหม่ ให้ใช้คำสั่งดังนี้:

1. เปิดเทอร์มินัลและรันคำสั่งต่อไปนี้ตามลำดับเพื่อสร้างไดเรกทอรีสำหรับฝึกฝนและคัดลอกโปรเจกต์ก่อนหน้า

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

1. สร้างโปรเจกต์ทั้งหมดโดยใช้คำสั่งด้านล่าง

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## การสร้างโปรเจกต์ทดสอบ: AppHost

เมื่อโปรเจกต์ `eShopLite.AppHost.Tests` ถูกตั้งค่าแล้ว โครงสร้างโดยรวมของโซลูชันจะมีลักษณะดังนี้:

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

1. ใช้คำสั่งต่อไปนี้เพื่อสร้างโปรเจกต์ทดสอบและเพิ่มเข้าไปในโซลูชัน

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new aspire-nunit -n eShopLite.AppHost.Tests -o test/eShopLite.AppHost.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.AppHost.Tests
    ```

   > สำหรับการทดสอบการเชื่อมต่อนี้ เราจะใช้ `.NET Aspire` template `dotnet new aspire-nunit`

1. ติดตั้ง NuGet packages ที่จำเป็นสำหรับการทดสอบเข้าไปในโปรเจกต์ทดสอบโดยใช้คำสั่งต่อไปนี้

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests package FluentAssertions
    dotnet add ./test/eShopLite.AppHost.Tests package Microsoft.Playwright.NUnit
    ```

1. เพิ่มการอ้างอิงไปยังโปรเจกต์ `AppHost` โดยใช้คำสั่งด้านล่าง

    ```bash
    dotnet add ./test/eShopLite.AppHost.Tests reference ./src/eShopLite.AppHost
    ```

1. เปิดไฟล์ `test/eShopLite.AppHost.Tests/eShopLite.AppHost.Tests.csproj` และแก้ไขตามนี้:

    ```xml
    <!-- Before -->
      <ItemGroup>
        <Using Include="System.Net" />
        <Using Include="Microsoft.Extensions.DependencyInjection" />
        <Using Include="Aspire.Hosting.ApplicationModel" />
        <Using Include="Aspire.Hosting.Testing" />
        <Using Include="NUnit.Framework" />
      </ItemGroup>
    ```

    ```xml
    <!-- After -->
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

1. เพิ่มแท็ก `<Target>...</Target>` node right above the `</Project>` เพื่อทำการติดตั้ง Playwright หลังจากการสร้างโปรเจกต์

    ```xml
      <Target Name="InstallPlaywright" AfterTargets="Build">
        <Exec Command="pwsh $(ProjectDir)/bin/Debug/net9.0/playwright.ps1 install"/>
      </Target>
    
    </Project>
    ```

## การเขียนโค้ดทดสอบ: หน้าแรก

1. ใช้คำสั่งต่อไปนี้เพื่อสร้างคลาสทดสอบสำหรับหน้า `/` ในโปรเจกต์ทดสอบ

    ```bash
    # Bash/Zsh
    touch $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path $REPOSITORY_ROOT/workshop/test/eShopLite.AppHost.Tests/HomePageTests.cs -Force
    ```

1. เปิดไฟล์ `test/eShopLite.AppHost.Tests/HomePageTests.cs` และใส่โค้ดต่อไปนี้:

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

1. เพิ่มโค้ดด้านล่างนี้โดยตรงใต้เมธอด `ContextOptions()`

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

   > - เมธอด `Setup()` method runs only once before running all the test methods in the test class.
   >   - It creates the `DistributedApplication` instance.
   >   - It creates the `ResourceNotificationService` instance.
   >   - It starts the `DistributedApplication` instance.
   >   - It lets the `ResourceNotificationService` instance wait for all the containers up and running.
   > - The `Teardown()` method runs only once after running all the test methods in the test class.
   >   - It disposes the `ResourceNotificationService` instance.
   >   - It deletes the `DistributedApplication` instance.

1. Add the following test codes right below the `SetUp()`:

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

   > - ตรวจสอบว่าองค์ประกอบ `Given_PageUrl_When_Invoked_Then_It_Should_Return_Heading1()`: checks the `H1` ถูกเรนเดอร์อย่างถูกต้องในหน้าแรก

1. บันทึกคลาสทดสอบและรันการทดสอบโดยใช้คำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

## การเขียนโค้ดทดสอบ: หน้าสินค้า

> **🚨🚨🚨 ความท้าทาย‼️ 🚨🚨🚨**
> 
> ดูตัวอย่างการทดสอบหน้าแรกด้านบนและ [STEP 03: Integration Testing with Testcontainers](./step-03.md) เพื่อสร้างคลาสทดสอบสำหรับหน้า `/products` ในลักษณะเดียวกัน

1. บันทึกคลาสทดสอบและรันการทดสอบโดยใช้คำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

## การเขียนโค้ดทดสอบ: หน้าสภาพอากาศ

> **🚨🚨🚨 ความท้าทาย‼️ 🚨🚨🚨**
> 
> ดูตัวอย่างการทดสอบหน้าแรก การทดสอบหน้าสินค้า และ [STEP 03: Integration Testing with Testcontainers](./step-03.md) เพื่อสร้างคลาสทดสอบสำหรับหน้า `/weather` ในลักษณะเดียวกัน

1. บันทึกคลาสทดสอบและรันการทดสอบโดยใช้คำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

---

ยินดีด้วย! คุณได้ทำแบบฝึกหัด **Integration Testing with .NET Aspire** เสร็จสมบูรณ์แล้ว

---

แบบฝึกหัดในเวิร์กช็อปทั้งหมดเสร็จสมบูรณ์แล้ว หากคุณมีคำถามหรือพบปัญหาระหว่างการทำแบบฝึกหัด สามารถกลับไปดูที่ [Save Points](../../../save-points) และลองใหม่ได้

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติด้วย AI แม้ว่าเราจะพยายามให้การแปลมีความถูกต้อง แต่โปรดทราบว่าการแปลโดยอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาต้นทางควรถูกพิจารณาว่าเป็นแหล่งข้อมูลที่ถูกต้องที่สุด สำหรับข้อมูลที่สำคัญ ขอแนะนำให้ใช้บริการแปลภาษาโดยมืออาชีพ เราจะไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่ผิดพลาดอันเกิดจากการใช้การแปลนี้