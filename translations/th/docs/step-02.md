# STEP 02: ทดสอบ API ด้วย Testcontainers

ในขั้นตอนนี้ เราจะใช้ [Testcontainers](https://dotnet.testcontainers.org/) เพื่อทดสอบแอปพลิเคชัน API ที่ทำงานอยู่ภายในคอนเทนเนอร์

## สิ่งที่ต้องเตรียมล่วงหน้า

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ตรวจสอบการติดตั้งสิ่งที่ต้องเตรียมล่วงหน้าทั้งหมดได้ที่เอกสาร [STEP 00: ตั้งค่าพื้นฐานสำหรับการพัฒนา](./step-00.md)

## คัดลอกโปรเจกต์ก่อนหน้า

คุณสามารถใช้แอปที่ทำในขั้นตอนก่อนหน้าได้เลย หรือจะคัดลอกใหม่จากเซฟพอยต์โดยใช้คำสั่งด้านล่างนี้ก็ได้

1. เปิดเทอร์มินัลและรันคำสั่งด้านล่างเพื่อสร้างไดเรกทอรีสำหรับการทดลองและคัดลอกโปรเจกต์ก่อนหน้า

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

1. ใช้คำสั่งด้านล่างเพื่อบิลด์โปรเจกต์ทั้งหมด

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## สร้างโปรเจกต์สำหรับการทดสอบ: Product API

เมื่อสร้างโปรเจกต์ `eShopLite.ProductApi.Tests` แล้ว โครงสร้างของโซลูชันทั้งหมดจะเปลี่ยนไปเป็นดังนี้

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

1. ใช้คำสั่งด้านล่างเพื่อสร้างโปรเจกต์สำหรับการทดสอบและเพิ่มเข้าไปในโซลูชัน

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet new nunit -n eShopLite.ProductApi.Tests -o test/eShopLite.ProductApi.Tests
    dotnet sln eShopLite.sln add ./test/eShopLite.ProductApi.Tests
    ```

1. ใช้คำสั่งด้านล่างเพื่อติดตั้งแพ็กเกจ NuGet ที่จำเป็นสำหรับการทดสอบในโปรเจกต์ทดสอบ

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests package FluentAssertions
    dotnet add ./test/eShopLite.ProductApi.Tests package TestContainers
    ```

1. ใช้คำสั่งด้านล่างเพื่อติดตั้งเรฟเฟอเรนซ์โปรเจกต์ที่จำเป็นในโปรเจกต์ทดสอบ

    ```bash
    dotnet add ./test/eShopLite.ProductApi.Tests reference ./src/eShopLite.DataEntities
    ```

## เขียนโค้ดสำหรับการทดสอบ: Product API

1. ใช้คำสั่งด้านล่างเพื่อสร้างคลาสสำหรับการทดสอบในโปรเจกต์ทดสอบ

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

1. เปิดไฟล์ `test/eShopLite.ProductApi.Tests/Endpoints/ProductEndpointsTests.cs` และป้อนโค้ดดังนี้

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

1. ใส่โค้ดด้านล่างไว้ใต้บรรทัด `private IContainer? _container;`

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
   >     - `.WithImage("eshoplite-productapi-test:latest")`: 컨테이너를 생성할 때 사용할 이미지를 지정합니다.
   >     - `.WithPortBinding(8080, true)`: 컨테이너의 8080 포트를 호스트의 임의의 포트에 바인딩합니다.
   >     - `.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(8080))`: 컨테이너가 8080 포트를 사용할 수 있을 때까지 대기합니다.
   > - `Teardown()` 메서드는 테스트 클래스의 모든 테스트 메서드를 실행한 후에 딱 한 번 실행합니다.
   >   - `IContainer` 인스턴스를 삭제합니다.

1. `SetUp()` เพิ่มโค้ดด้านล่างไว้ใต้เมธอดนี้

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

1. `Init()` เพิ่มโค้ดทดสอบด้านล่างไว้ใต้เมธอดนี้

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

1. `Given_Endpoint_When_Invoked_Then_It_Should_Return_OK()` เพิ่มโค้ดด้านล่างไว้ใต้เมธอดนี้

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

   > - `Given_Endpoint_When_Invoked_Then_It_Should_Return_Collection()`: `GET /api/products` 엔드포인트를 호출했을 때 `Product` ตรวจสอบว่าได้รับคอลเลกชันนี้หรือไม่

1. บันทึกคลาสสำหรับการทดสอบและรันคำสั่งด้านล่างเพื่อรันการทดสอบ

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

   จะพบข้อความแสดงข้อผิดพลาดดังนี้

    ```text
    Error Message: Docker.DotNet.DockerApiException : Docker API responded with status code=NotFound, response={"message":"pull access denied for e
          shoplite-productapi-test, repository does not exist or may require 'docker login': denied: requested access to the resource is denied"}
    ```

   ข้อผิดพลาดนี้เกิดจากการไม่มีภาพ `eshoplite-productapi-test`

1. ใช้คำสั่งด้านล่างเพื่อบิลด์ภาพคอนเทนเนอร์

    ```bash
    cd $REPOSITORY_ROOT/workshop

    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi-test:latest
    ```

1. รันคำสั่งด้านล่างอีกครั้งเพื่อรันการทดสอบ

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test ./test/eShopLite.ProductApi.Tests
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ
1. ใช้คำสั่งด้านล่างเพื่อลบภาพคอนเทนเนอร์สำหรับการทดสอบ

    ```bash
    docker rmi eshoplite-productapi-test:latest
    ```

## สร้างโปรเจกต์สำหรับการทดสอบ: Weather API

เมื่อสร้างโปรเจกต์ `eShopLite.WeatherApi.Tests` แล้ว โครงสร้างของโซลูชันทั้งหมดจะเปลี่ยนไปเป็นดังนี้

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

> **🚨🚨🚨 ท้าทาย‼️ 🚨🚨🚨**
> 
> สร้างโปรเจกต์ `eShopLite.ProductApi.Tests` 테스트 프로젝트와 마찬가지로 `eShopLite.WeatherApi.Tests` 프로젝트를 생성해 보세요.

## 테스트 코드 작성: Weather API

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `eShopLite.ProductApi.Tests` 테스트 프로젝트와 마찬가지로 `eShopLite.WeatherApi.Tests` และเขียนโค้ดสำหรับการทดสอบ
>
> ใช้คำสั่งด้านล่างเพื่อให้การทดสอบผ่าน
> 
>     ```bash
>     cd $REPOSITORY_ROOT/workshop
> 
>     dotnet test ./test/eShopLite.WeatherApi.Tests
>     ```

## การทดสอบ API แบบรวม

1. ใช้คำสั่งด้านล่างเพื่อรันโปรเจกต์ทดสอบทั้งหมด

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

## การบิลด์ภาพคอนเทนเนอร์แบบอัตโนมัติ

เมื่อทดสอบคอนเทนเนอร์ จะมีการบิลด์ภาพคอนเทนเนอร์แบบอัตโนมัติ

1. ใช้คำสั่งด้านล่างเพื่อลบภาพคอนเทนเนอร์สำหรับการทดสอบที่มีอยู่ทั้งหมด

    ```bash
    docker rmi eshoplite-productapi-test:latest --force
    docker rmi eshoplite-weatherapi-test:latest --force
    ```

1. เพิ่มโหนด `test/eShopLite.ProductApi.Tests/eShopLite.ProductApi.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `<Target>...</Target>` ดังนี้

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.productapi -t eshoplite-productapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. เพิ่มโหนด `test/eShopLite.WeatherApi.Tests/eShopLite.WeatherApi.Tests.csproj` 파일을 열고 `</Project>` 바로 위에 다음 `<Target>...</Target>` ดังนี้

    ```xml
      <Target Name="BuildContainerImage" BeforeTargets="PrepareForBuild">
        <Exec Command="docker build $(SolutionDir) -f $(SolutionDir)Dockerfile.weatherapi -t eshoplite-weatherapi-test:latest"/>
      </Target>
    
    </Project>
    ```

1. ใช้คำสั่งด้านล่างเพื่อรันโปรเจกต์ทดสอบทั้งหมด

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet test .
    ```

1. ตรวจสอบว่าการทดสอบทั้งหมดผ่านสำเร็จ

---

ยินดีด้วย! **การทดสอบ API ด้วย Testcontainers** เสร็จสมบูรณ์แล้ว ตอนนี้คุณสามารถไปต่อที่ขั้นตอน [STEP 03: ทดสอบแบบรวมด้วย Testcontainers](./step-03.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติด้วย AI แม้ว่าเราจะพยายามให้การแปลมีความถูกต้อง แต่โปรดทราบว่าการแปลโดยอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาดั้งเดิมควรถือเป็นแหล่งข้อมูลที่เชื่อถือได้ สำหรับข้อมูลสำคัญ แนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความผิดที่เกิดจากการใช้การแปลนี้