# STEP 04: การจัดการ Container ด้วย .NET Aspire

ในขั้นตอนนี้ คุณจะจัดการแอปทั้งหมดโดยใช้ [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)

## สิ่งที่ต้องเตรียมก่อนเริ่ม

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

เพื่อยืนยันการติดตั้งของแต่ละสิ่งที่ต้องเตรียม ดูเอกสาร [STEP 00: Setting Up the Development Environment](./step-00.md)

## คัดลอกโปรเจกต์พื้นฐาน

คุณสามารถใช้แอปจากขั้นตอนก่อนหน้า หรือคัดลอกเวอร์ชันใหม่จากจุดบันทึกโดยใช้คำสั่งด้านล่าง หากต้องการคัดลอกเวอร์ชันใหม่ ให้ใช้คำสั่งต่อไปนี้:

1. เปิด terminal และรันคำสั่งต่อไปนี้ตามลำดับเพื่อสร้างไดเรกทอรีสำหรับฝึกฝนและคัดลอกโปรเจกต์พื้นฐาน:

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

1. สร้างโปรเจกต์ทั้งหมดโดยใช้คำสั่งต่อไปนี้:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

## เพิ่มโปรเจกต์ .NET Aspire สำหรับการจัดการ

แตกต่างจากการฝึกฝนครั้งก่อน ครั้งนี้คุณจะใช้ .NET Aspire ในการจัดการ container

เมื่อคุณติดตั้งโปรเจกต์ .NET Aspire สำหรับการจัดการ โครงสร้างโดยรวมของ solution จะเปลี่ยนไปดังที่แสดงด้านล่าง:

```text
eShopLite
└── src
    ├── eShopLite.AppHost
    │   ├── eShopLite.WebApp
    │   ├── eShopLite.ProductApi
    │   └── eShopLite.WeatherApi
    ├── eShopLite.ServiceDefaults
    ├── eShopLite.WebApp
    │   ├── eShopLite.DataEntities
    │   └── eShopLite.ServiceDefaults
    ├── eShopLite.WeatherApi
    │   ├── eShopLite.DataEntities
    │   └── eShopLite.ServiceDefaults
    └── eShopLite.ProductApi
        ├── eShopLite.ProductData
        │   └── eShopLite.DataEntities
        └── eShopLite.ServiceDefaults
```

### เพิ่มโปรเจกต์ .NET Aspire

1. รันคำสั่งต่อไปนี้เพื่อเพิ่มโปรเจกต์ .NET Aspire orchestrator:

    ```bash
    dotnet new aspire-apphost -n eShopLite.AppHost -o src/eShopLite.AppHost
    dotnet sln eShopLite.sln add ./src/eShopLite.AppHost
    ```

1. ใช้คำสั่งต่อไปนี้เพื่อเพิ่มแอปทั้งหมดในโปรเจกต์ .NET Aspire orchestrator:

    ```bash
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.WebApp
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.ProductApi
    dotnet add ./src/eShopLite.AppHost reference ./src/eShopLite.WeatherApi
    ```

1. ใช้คำสั่งด้านล่างเพื่อเพิ่มโปรเจกต์ .NET Aspire base service:

    ```bash
    dotnet new aspire-servicedefaults -n eShopLite.ServiceDefaults -o src/eShopLite.ServiceDefaults
    dotnet sln eShopLite.sln add ./src/eShopLite.ServiceDefaults
    ```

1. รันคำสั่งต่อไปนี้เพื่อเพิ่มโปรเจกต์ .NET Aspire base service ในแต่ละแอป:

    ```bash
    dotnet add ./src/eShopLite.WebApp reference ./src/eShopLite.ServiceDefaults
    dotnet add ./src/eShopLite.ProductApi reference ./src/eShopLite.ServiceDefaults
    dotnet add ./src/eShopLite.WeatherApi reference ./src/eShopLite.ServiceDefaults
    ```

### แก้ไข `eShopLite.WebApp` Project

1. Open `src/eShopLite.WebApp/Program.cs`, find `var builder = WebApplication.CreateBuilder(args);` และเพิ่มเนื้อหาต่อไปนี้ทันทีด้านล่าง:

    ```csharp
    builder.AddServiceDefaults();
    ```

   > การตั้งค่านี้ช่วยให้สามารถใช้บริการเริ่มต้นที่จัดเตรียมโดย .NET Aspire

2. อัปเดตโค้ดดังนี้:

    ```csharp
    // Before
    builder.Services.AddHttpClient<ProductApiClient>(client =>
    {
        client.BaseAddress = new("http://localhost:5051");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("http://localhost:5050");
    });
    ```

    ```csharp
    // After
    builder.Services.AddHttpClient<ProductApiClient>(client =>
    {
        client.BaseAddress = new("https+http://productapi");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("https+http://weatherapi");
    });
    ```

   > แก้ไขเพื่อใช้การค้นหาบริการที่จัดเตรียมโดย .NET Aspire

3. เพิ่มเนื้อหาต่อไปนี้ก่อน `app.Run();`:

    ```csharp
    app.MapDefaultEndpoints();
    ```

   > การตั้งค่านี้ช่วยให้สามารถใช้ endpoint ตรวจสอบสถานะที่จัดเตรียมโดย .NET Aspire

### แก้ไข `eShopLite.ProductApi` Project

1. Open `src/eShopLite.ProductApi/Program.cs`, find `var builder = WebApplication.CreateBuilder(args);` และเพิ่มเนื้อหาต่อไปนี้ทันทีด้านล่าง:

    ```csharp
    builder.AddServiceDefaults();
    ```

1. เพิ่มบรรทัดต่อไปนี้ก่อน `app.Run();`:

    ```csharp
    app.MapDefaultEndpoints();
    ```

### แก้ไข `eShopLite.WeatherApi` Project

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> Like modifying the `eShopLite.ProductApi` project, modify the `eShopLite.WeatherApi` project.

### Modify `eShopLite.AppHost` Project

1. Open `src/eShopLite.AppHost/Program.cs`, find `var builder = DistributedApplication.CreateBuilder(args);` และเพิ่มบรรทัดต่อไปนี้ทันทีด้านล่าง:

    ```csharp
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi");
    var weatherapi = builder.AddProject<Projects.eShopLite_WeatherApi>("weatherapi");
    ```

   > เพิ่มทั้ง `ProductApi` and `WeatherApi` to the orchestrator project, `AppHost`

1. เพิ่มเนื้อหาต่อไปนี้ในบรรทัดถัดไป:

    ```csharp
    builder.AddProject<Projects.eShopLite_WebApp>("webapp")
           .WithExternalHttpEndpoints()
           .WithReference(productapi)
           .WithReference(weatherapi)
           .WaitFor(productapi)
           .WaitFor(weatherapi);
    ```

   > ตั้งค่า `WebApp` project in the orchestrator, `AppHost`
   > 
   > - `.WithExternalHttpEndpoints()`: exposes for the public access.
   > - `.WithReference(productapi)`: lets `WebApp` discover `ProductApi`.
   > - `.WithReference(weatherapi)`: lets `WebApp` discover `WeatherApi`.
   > - `.WaitFor(productapi)`: lets `WebApp` wait for `ProductApi` being up and running.
   > - `.WaitFor(weatherapi)`: lets `WebApp` wait for `WeatherApi` ให้พร้อมใช้งาน

### รัน .NET Aspire Orchestrator

1. ใช้คำสั่งต่อไปนี้เพื่อรัน .NET Aspire orchestrator:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet watch run --project ./src/eShopLite.AppHost
    ```

1. เบราว์เซอร์จะเปิดขึ้นโดยอัตโนมัติและแสดงแดชบอร์ด ซึ่งจะแสดง `productapi`, `weatherapi` and `webapp` resources.

    ![Aspire Dashboard](../../../../../docs/images/aspire-dashboard-1.png)

   > You might be seeing this login screen.
   > 
   > ![Aspire Dashboard Login](../../../../../docs/images/aspire-dashboard-login.png)
   > 
   > Click the link and follow the instructions to get into the dashboard.

1. Click each endpoint of `productapi` and `weatherapi` to see their respective OpenAPI document.
1. Click the enpoint of `webapp` to see the web application. Navigate to both `/products` and `/weather` pages and see whether they are properly up.
1. Type `Ctrl`+`C` in the terminal and stop the .NET Aspire orchestrator.

## Replace Database through .NET Aspire Orchestrator

Let's change the database from SQLite to PostgreSQL.

### Modify `eShopLite.AppHost` Project

1. Run the following command to add the PostgreSQL package to the `eShopLite.AppHost` project

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet add ./src/eShopLite.AppHost package Aspire.Hosting.PostgreSQL
    ```

1. เปิด `src/eShopLite.AppHost/Program.cs`, find `var builder = DistributedApplication.CreateBuilder(args);` และเพิ่มเนื้อหาต่อไปนี้ทันทีด้านล่าง:

    ```csharp
    var productsdb = builder.AddPostgres("pg")
                            .WithPgAdmin()
                            .AddDatabase("productsdb");
    ```

   > เพิ่มฐานข้อมูล PostgreSQL
   > 
   > - `.AddPostgres("pg")`: adds a container for PostgreSQL database.
   > - `.WithPgAdmin()`: adds a container for PGAdmin dashboard.
   > - `.AddDatabase("productsdb")`: adds a new database called `productsdb`

1. แก้ไขเนื้อหาต่อไปนี้:

    ```csharp
    // Before
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi");
    ```

    ```csharp
    // After
    var productapi = builder.AddProject<Projects.eShopLite_ProductApi>("productapi")
                            .WithReference(productsdb);
    ```

   > เพิ่มฐานข้อมูล PostgreSQL ในโปรเจกต์ `ProductApi` project.
   > 
   > - `.WithReference(productsdb)`: let `ProductApi` discover the PostgreSQL database, `productsdb`.

### Modify `eShopLite.ProductApi` Project

1. Run the following command to add a PostgreSQL database packaget to the `eShopLite.ProductApi`

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet add ./src/eShopLite.ProductApi package Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
    ```

1. เปิดไฟล์ `src/eShopLite.ProductApi/appsettings.json`, remove the `ConnectionStrings` section completely. The `appsettings.json` และแก้ไขให้เป็นดังนี้:

    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
    
      "AllowedHosts": "*"
    }
    ```

   > ตรวจสอบให้แน่ใจว่าไม่มี `ConnectionStrings` section any longer.

1. Open the `src/eShopLite.ProductApi/Program.cs` และแก้ไขไฟล์ดังนี้:

    ```csharp
    // Before
    builder.Services.AddDbContext<ProductDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("ProductsContext") ?? throw new InvalidOperationException("Connection string 'ProductsContext' not found.");
        options.UseSqlite(connectionString);
    });
    ```

    ```csharp
    // After
    builder.AddNpgsqlDbContext<ProductDbContext>("productsdb");
    ```

   > อัปเดตเพื่อใช้ connection string ของฐานข้อมูล PostgreSQL
   > 
   > - ชื่อ `productsdb`is the reference name that `AppHost` ใช้งาน

### รัน .NET Aspire Orchestrator

1. ใช้คำสั่งต่อไปนี้เพื่อรัน .NET Aspire orchestrator:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet watch run --project ./src/eShopLite.AppHost
    ```

1. เบราว์เซอร์จะเปิดขึ้นโดยอัตโนมัติและแสดงแดชบอร์ด ซึ่งจะแสดง `pg`, `pg-pgadmin`, `productsdb`, `productapi`, `weatherapi` and `webapp` resources.

    ![Aspire Dashboard](../../../../../docs/images/aspire-dashboard-2.png)

   > You might be seeing the login screen.
   > 
   > ![Aspire Dashboard Login](../../../../../docs/images/aspire-dashboard-login.png)
   > 
   > Click the link and follow the instructions so that you can access to the dashboard.

1. Click the endpoint of `pg-pgadmin` to see the admin dashboard for the PostgreSQL database.
1. Click the endpoint of both `productapi` and `weatherapi` to see respective OpenAPI document.
1. Click the endpoint of `webapp` to see the web app. Navigate to both `/products` and `/weather` pages to see they are properly showing up.
1. Type `Ctrl`+`C` ใน terminal เพื่อหยุด .NET Aspire orchestrator

---

ยินดีด้วย! คุณได้ทำการฝึกฝน **Container Orchestration with .NET Aspire** เสร็จสมบูรณ์แล้ว ดำเนินการต่อในขั้นตอนถัดไป: [STEP 05: Integration Testing with .NET Aspire](./step-05.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษา AI ที่ใช้เครื่องจักร แม้ว่าเราจะพยายามอย่างเต็มที่เพื่อความถูกต้อง โปรดทราบว่าการแปลอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาดั้งเดิมควรถือเป็นแหล่งข้อมูลที่มีความถูกต้องมากที่สุด สำหรับข้อมูลสำคัญ ขอแนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญ เราจะไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่ผิดพลาดซึ่งเกิดจากการใช้การแปลนี้