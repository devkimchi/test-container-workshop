# STEP 02: การทดสอบ API ด้วย Testcontainers

ในขั้นตอนนี้ เราจะใช้ [Testcontainers](https://dotnet.testcontainers.org/) เพื่อทดสอบแอปพลิเคชัน API ที่ทำงานภายในคอนเทนเนอร์

## สิ่งที่ต้องเตรียม

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ดูเอกสาร [STEP 00: Setting up the Development Environment](./step-00.md) เพื่อให้แน่ใจว่าทุกสิ่งที่ต้องเตรียมถูกติดตั้งเรียบร้อยแล้ว

## การคัดลอกโปรเจกต์ก่อนหน้า

คุณสามารถใช้แอปพลิเคชันจากขั้นตอนก่อนหน้า หรือสร้างสำเนาใหม่จากจุดที่บันทึกไว้ด้วยคำสั่งด้านล่าง หากต้องการสร้างสำเนาใหม่ ให้ใช้คำสั่งต่อไปนี้:

1. เปิดเทอร์มินัลและรันคำสั่งด้านล่างเพื่อสร้างไดเรกทอรีสำหรับการฝึกฝนและคัดลอกโปรเจกต์ก่อนหน้า

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

1. สร้างโปรเจกต์ทั้งหมดด้วยคำสั่งด้านล่าง

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## การสร้างโปรเจกต์ทดสอบ: Product API

หลังจากเพิ่มโปรเจกต์ `eShopLite.ProductApi.Tests` โครงสร้างของโซลูชันทั้งหมดจะมีลักษณะดังนี้:

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

1. ใช้คำสั่งต่อไปนี้เพื่อสร้างโปรเจกต์ทดสอบและเพิ่มเข้าไปในโซลูชัน

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit -n eShopLite.ProductApi.Tests -o test/eShopLite.ProductApi.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.ProductApi.Tests
    ```

1. ติดตั้งแพ็กเกจ NuGet ที่จำเป็นสำหรับการทดสอบในโปรเจกต์ทดสอบด้วยคำสั่งด้านล่าง

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests package FluentAssertions
    dotnet add ./test/eShopLite.ProductApi.Tests package TestContainers
    ```

1. เพิ่มโปรเจกต์อ้างอิงที่จำเป็นในโปรเจกต์ทดสอบด้วยคำสั่งต่อไปนี้

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests reference ./src/eShopLite.DataEntities
    ```

## การเขียนโค้ดทดสอบ: Product API

1. สร้างคลาสทดสอบในโปรเจกต์ทดสอบด้วยคำสั่งต่อไปนี้

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

1. เปิดไฟล์ `test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs` และใส่โค้ดดังต่อไปนี้:

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

1. ใต้บรรทัด `private IContainer? _container;` เพิ่มโค้ดต่อไปนี้:

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

   > - เมธอด `Setup()` method is run only once before running all the test in this class.
   >   - It creates an `IContainer` instance.
   >     - `.WithImage("eshoplite-productapi-test:latest")`: declares the container image.
   >     - `.WithPortBinding(8080, true)`: binds the container's 8080 port to a random one on the host.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: waits until the container is available to use the 8080 port.
   > - The `Teardown()` method is run only once after completing all the tests in this class.
   >   - It deletes the `IContainer` instance.

1. Right below the `SetUp()` method, add the following `Init()` and `Cleanup()` 

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

   > - เมธอด `Init()` method runs before running each test method.
   >   - Starts the container.
   > - The `Cleanup()` method runs after completing each test method.
   >   - Stops the container.

1. Right after the `Init()` เพิ่มโค้ดทดสอบดังต่อไปนี้

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

   > - เมธอด `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()`: checks whether to receive the 200 (OK) response when calling the `GET /api/products` endpoint.

1. Right after the `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()` เพิ่มโค้ดทดสอบดังต่อไปนี้

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

   > - `Given_Endpoint_When_Invoked_Then_It_Should_Return_Collection()`: checks whether to receive the list of `Product` when calling the `GET /api/products` endpoint

1. บันทึกคลาสทดสอบและรันคำสั่งต่อไปนี้เพื่อรันการทดสอบ

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

   คุณจะพบข้อความแสดงข้อผิดพลาดดังต่อไปนี้:

    ```text
    Error Message: Docker.DotNet.DockerApiException : Docker API responded with status code=NotFound, response={"message":"pull access denied for e
          shoplite-productapi-test, repository does not exist or may require 'docker login': denied: requested access to the resource is denied"}
    ```

   ข้อผิดพลาดนี้เกิดขึ้นเพราะภาพ `eshoplite-productapi-test` ยังไม่มีอยู่

1. สร้างภาพคอนเทนเนอร์ด้วยคำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi-test:latest
    ```

1. รันการทดสอบอีกครั้งด้วยคำสั่งด้านล่าง:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านเรียบร้อยแล้ว
1. ลบภาพคอนเทนเนอร์ทดสอบด้วยคำสั่งต่อไปนี้:

    ```bash
    docker rmi eshoplite-productapi-test:latest
    ```

## การสร้างโปรเจกต์ทดสอบ: Weather API

หลังจากเพิ่มโปรเจกต์ `eShopLite.WeatherApi.Tests` โครงสร้างของโซลูชันทั้งหมดจะมีลักษณะดังนี้:

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

> **🚨🚨🚨 ความท้าทาย‼️ 🚨🚨🚨**
> 
> เช่นเดียวกับ `eShopLite.ProductApi.Tests` projects, create a test project, `eShopLite.WeatherApi.Tests`.

## Writing Test Codes: Weather API

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> Like `eShopLite.ProductApi.Tests` projects, create a test project, `eShopLite.WeatherApi.Tests` เขียนการทดสอบ
>
> ใช้คำสั่งต่อไปนี้เพื่อให้แน่ใจว่าการทดสอบผ่าน:
> 
>     ```bash
>     cd $REPOSITORY_ROOT/workshop
> 
>     dotnet test ./test/eShopLite.WeatherApi.Tests
>     ```

## การทดสอบการเชื่อมต่อ API

1. รันโปรเจกต์ทดสอบทั้งหมดด้วยคำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านเรียบร้อยแล้ว

## การสร้างภาพคอนเทนเนอร์โดยอัตโนมัติ

สร้างภาพคอนเทนเนอร์โดยอัตโนมัติระหว่างการทดสอบ

1. ลบภาพคอนเทนเนอร์ทดสอบทั้งหมดที่มีอยู่ด้วยคำสั่งต่อไปนี้:

    ```bash
    docker rmi eshoplite-productapi-test:latest --force
    docker rmi eshoplite-weatherapi-test:latest --force
    ```

1. เพิ่ม `<Target>...</Target>` node just above `</Project>` to the `test/eShopLite.ProductApi.Tests/eShopLite.ProductApi.Tests.csproj` ไฟล์

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. เพิ่ม `<Target>...</Target>` node just above `</Project>` to the `test/eShopLite.WeatherApi.Tests/eShopLite.WeatherApi.Tests.csproj` ไฟล์

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. รันโปรเจกต์ทดสอบทั้งหมดด้วยคำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านเรียบร้อยแล้ว

---

ขอแสดงความยินดี! คุณได้ทำ **การทดสอบ API ด้วย Testcontainers** สำเร็จแล้ว ตอนนี้ไปที่ [STEP 03: Integration Testing with Testcontainers](./step-03.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษา AI แบบอัตโนมัติ แม้ว่าเราจะพยายามให้การแปลมีความถูกต้องมากที่สุด แต่โปรดทราบว่าการแปลโดยอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาต้นทางควรถือเป็นแหล่งข้อมูลที่ถูกต้องที่สุด สำหรับข้อมูลที่สำคัญ ขอแนะนำให้ใช้บริการแปลภาษาโดยผู้เชี่ยวชาญ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่คลาดเคลื่อนอันเกิดจากการใช้การแปลนี้