# STEP 01: สร้างไฟล์ Dockerfile และ Docker Compose File

ในขั้นตอนนี้ คุณจะสร้างไฟล์ `Dockerfile` เพื่อจัดการแอปพลิเคชันแต่ละตัวในโปรเจกต์พื้นฐาน และไฟล์ `Docker Compose` เพื่อจัดการการทำงานร่วมกันของแอปพลิเคชันเหล่านั้น

## สิ่งที่ต้องเตรียม

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ดูเอกสาร [STEP 00: การตั้งค่าพื้นฐานสำหรับการพัฒนา](./step-00.md) เพื่อเช็คสถานะการติดตั้งของแต่ละสิ่งที่ต้องเตรียม

## การคัดลอกโปรเจกต์พื้นฐาน

โปรเจกต์พื้นฐานที่จำเป็นสำหรับเวิร์กช็อปนี้ได้ถูกเตรียมไว้แล้ว ตรวจสอบว่าโปรเจกต์ทำงานได้ถูกต้อง โครงสร้างของโปรเจกต์พื้นฐานมีดังนี้:

```text
eShopLite
└── src
    ├── eShopLite.WebApp
    ├── eShopLite.WeatherApi
    ├── eShopLite.ProductApi
    ├── eShopLite.ProductData
    └── eShopLite.DataEntities
```

1. เปิดเทอร์มินัลและรันคำสั่งด้านล่างทีละขั้นตอนเพื่อสร้างไดเรกทอรีสำหรับฝึกปฏิบัติและคัดลอกโปรเจกต์พื้นฐาน:

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

## การสร้างและรันโปรเจกต์พื้นฐาน

1. สร้างโปรเจกต์ทั้งหมดโดยใช้คำสั่งด้านล่าง:

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

1. เปิดหน้าต่างเทอร์มินัลสามหน้าต่างและรันคำสั่งด้านล่างในแต่ละหน้าต่างเพื่อเปิดแอปพลิเคชัน:

    ```bash
    # Terminal 1
    cd $REPOSITORY_ROOT/workshop
    dotnet run --project ./src/eShopLite.WeatherApi
    ```

    ```bash
    # Terminal 2
    cd $REPOSITORY_ROOT/workshop
    dotnet run --project ./src/eShopLite.ProductApi
    ```

    ```bash
    # Terminal 3
    cd $REPOSITORY_ROOT/workshop
    dotnet watch run --project ./src/eShopLite.WebApp
    ```

   > แต่ละเทอร์มินัลอาจไม่สามารถรับรู้ค่า `$REPOSITORY_ROOT` หากเกิดกรณีนี้ ให้รันคำสั่งด้านล่างแยกกันในแต่ละเทอร์มินัล:
   >
   > ```bash
   > # Bash/Zsh
   > REPOSITORY_ROOT=$(git rev-parse --show-toplevel)
   > ```
   >
   > ```powershell
   > # PowerShell
   > $REPOSITORY_ROOT = git rev-parse --show-toplevel
   > ```

1. เว็บเบราว์เซอร์จะเปิดขึ้นโดยอัตโนมัติ แสดงผล `https://localhost:7000` or `http://localhost:5000`. If your web browser doesn't automatically open, enter the URL manually.
1. Navigate to `/weather` or `/products` page and confirm both pages are properly rendering.
1. On each terminal, press `Ctrl+C` เพื่อหยุดแอปพลิเคชัน
1. ปิดเทอร์มินัลทั้งหมด ยกเว้นหนึ่งหน้าต่าง

## การสร้างภาพคอนเทนเนอร์โดยใช้ `Dockerfile`

โปรเจกต์พื้นฐานมีการพึ่งพาสำหรับแต่ละแอปพลิเคชัน โครงสร้างการพึ่งพามีดังนี้:

```text
eShopLite
└── src
    ├── eShopLite.WebApp
    │   └── eShopLite.DataEntities
    ├── eShopLite.ProductApi
    │   └── eShopLite.ProductData
    │       └── eShopLite.DataEntities
    └── eShopLite.WeatherApi
        └── eShopLite.DataEntities
```

ดังนั้น เมื่อสร้างภาพคอนเทนเนอร์แยกสำหรับแต่ละแอปพลิเคชัน ต้องแก้ไขการพึ่งพาเหล่านี้ให้เรียบร้อย

> ณ จุดนี้ Docker Desktop ต้องทำงานอยู่ หากไม่มีไอคอน Docker ปรากฏในแถบงาน ให้เปิด Docker Desktop

### การสร้าง `Dockerfile`: `eShopLite.WebApp`

1. ไปยังไดเรกทอรีรากของเวิร์กช็อป:

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. ใช้คำสั่งด้านล่างเพื่อสร้าง `Dockerfile.webapp`:

    ```bash
    # Bash/Zsh
    touch Dockerfile.webapp
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path Dockerfile.webapp -Force
    ```

1. เปิด `Dockerfile.webapp` และใส่เนื้อหาด้านล่าง:

    ```dockerfile
    # syntax=docker/dockerfile:1
    
    FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

    COPY ./src/eShopLite.WebApp /source/eShopLite.WebApp
    COPY ./src/eShopLite.DataEntities /source/eShopLite.DataEntities
    
    WORKDIR /source/eShopLite.WebApp
    
    RUN dotnet publish -c Release -o /app
    
    FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
    
    WORKDIR /app
    
    COPY --from=build /app .
    
    USER $APP_UID
    
    ENTRYPOINT ["dotnet", "eShopLite.WebApp.dll"]
    ```

   > **หมายเหตุ**: `Dockerfile.webapp` คือ `Dockerfile` ที่ใช้สร้างภาพคอนเทนเนอร์สำหรับโปรเจกต์ `eShopLite.WebApp` project.
   > 
   > - `Dockefile.webapp` must be at the same location of the `eShopLite.sln` file.
   > - Both `eShopLite.WebApp` and `eShopLite.DataEntities` ซึ่งต้องคัดลอกไปยัง `Dockerfile` เพื่อแก้ไขการพึ่งพา

1. ใช้คำสั่งด้านล่างเพื่อสร้างภาพคอนเทนเนอร์สำหรับโปรเจกต์ `eShopLite.WebApp`:

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. รันคำสั่งด้านล่างเพื่อเปิดภาพคอนเทนเนอร์ที่สร้างขึ้น:

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    ```

1. เปิดเว็บเบราว์เซอร์และไปที่ `http://localhost:3000` and confirm whether the web app is up and running.
1. Navigate to `/weather` or `/products` เพื่อ **ตรวจสอบข้อผิดพลาดในแต่ละหน้า**

   > ณ จุดนี้ควรมีข้อผิดพลาดเกิดขึ้น

1. รันคำสั่งด้านล่างเพื่อหยุดคอนเทนเนอร์และลบคอนเทนเนอร์รวมถึงภาพคอนเทนเนอร์:

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    docker rmi eshoplite-webapp:latest --force
    ```

### การสร้าง `Dockerfile`: `eShopLite.ProductApi`

1. ไปยังไดเรกทอรีรากของเวิร์กช็อป:

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. ใช้คำสั่งด้านล่างเพื่อสร้าง `Dockerfile.productapi`:

    ```bash
    # Bash/Zsh
    touch Dockerfile.productapi
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path Dockerfile.productapi -Force
    ```

1. เปิด `Dockerfile.productapi` และใส่เนื้อหาด้านล่าง:

    ```dockerfile
    # syntax=docker/dockerfile:1
    
    FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
    
    COPY ./src/eShopLite.ProductApi /source/eShopLite.ProductApi
    COPY ./src/eShopLite.ProductData /source/eShopLite.ProductData
    COPY ./src/eShopLite.DataEntities /source/eShopLite.DataEntities
    
    WORKDIR /source/eShopLite.ProductApi
    
    RUN dotnet publish -c Release -o /app
    
    FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
    
    WORKDIR /app
    
    COPY --from=build /app .
    
    RUN chown $APP_UID /app
    
    USER $APP_UID
    
    RUN touch /app/Database.db
    
    ENTRYPOINT ["dotnet", "eShopLite.ProductApi.dll"]
    ```

   > **หมายเหตุ**: `Dockerfile.productapi` คือ `Dockerfile` ที่ใช้สร้างภาพคอนเทนเนอร์สำหรับโปรเจกต์ `eShopLite.ProductApi` project.
   > 
   > - `Dockefile.productapi` must be at the same location as the `eShopLite.sln` file.
   > - All `eShopLite.ProductApi`, `eShopLite.ProductData` and `eShopLite.DataEntities` projects must be copied to sort out dependencies.

1. Use the following command to build the container image for the `eShopLite.ProductApi`

    ```bash
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    ```

1. รันคำสั่งด้านล่างเพื่อเปิดภาพคอนเทนเนอร์ที่สร้างขึ้น:

    ```bash
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์และไปที่ `http://localhost:3030` and confirm whether to **see the 404 (not found) error**.
1. Navigate to `/api/products` เพื่อยืนยันว่าข้อมูลแสดงผลได้ถูกต้อง
1. รันคำสั่งด้านล่างเพื่อหยุดคอนเทนเนอร์และลบคอนเทนเนอร์รวมถึงภาพคอนเทนเนอร์:

    ```bash
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    docker rmi eshoplite-productapi:latest --force
    ```

### การสร้าง `Dockerfile`: `eShopLite.WeatherApi`

> **🚨🚨🚨 Challenge‼️ 🚨🚨🚨**
> 
> By referring to `Dockerfile.webapp` and `Dockerfile.productapi`, write `Dockerfile.weatherapi` to build the container image for the `eShopLite.WeatherApi` project.
>
> - `Dockerfile.weatherapi` คือ `Dockerfile` ที่ใช้สร้างภาพคอนเทนเนอร์สำหรับโปรเจกต์ `eShopLite.WeatherApi` project.
> - `Dockefile.weatherapi` must be at the same location as the `eShopLite.sln` file.

Once complete writing up `Dockerfile.weatherapi`, follow the sequence below.

1. Run the following command `eShopLite.WeatherApi` เพื่อสร้างภาพคอนเทนเนอร์สำหรับโปรเจกต์นี้

    ```bash
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. รันคำสั่งด้านล่างเพื่อเปิดภาพคอนเทนเนอร์ที่สร้างขึ้น:

    ```bash
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์และไปที่ `http://localhost:3031` and confirm whether to **see the 404 (not found) error**.
1. `/api/weatherforecast` เพื่อยืนยันว่าข้อมูลแสดงผลได้ถูกต้อง
1. รันคำสั่งด้านล่างเพื่อหยุดคอนเทนเนอร์และลบคอนเทนเนอร์รวมถึงภาพคอนเทนเนอร์:

    ```bash
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## การจัดการคอนเทนเนอร์: Docker Network

ใช้ไฟล์ `Dockerfile` ทั้งสามไฟล์ที่สร้างขึ้นก่อนหน้านี้เพื่อจัดการคอนเทนเนอร์

1. รันคำสั่งด้านล่างเพื่อสร้างภาพคอนเทนเนอร์สำหรับ `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi`:

    ```bash
    cd $REPOSITORY_ROOT/workshop
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. รันคำสั่งด้านล่างเพื่อเปิดภาพคอนเทนเนอร์ที่สร้างขึ้น:

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์และไปที่ `http://localhost:3000` and see whether the app is up and running.
1. Navigate to `/weather` or `/products` เพื่อ **ตรวจสอบข้อผิดพลาดในแต่ละหน้า**

   > ณ จุดนี้ควรมีข้อผิดพลาดเกิดขึ้น

1. รันคำสั่งด้านล่างเพื่อหยุดและลบคอนเทนเนอร์ทั้งหมด:

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force

    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force

    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    ```

1. เปิดไฟล์ `src/eShopLite.WebApp/Program.cs` และแก้ไขโค้ดดังนี้:

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
        client.BaseAddress = new("http://productapi:8080");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("http://weatherapi:8080");
    });
    ```

1. สร้างภาพคอนเทนเนอร์ใหม่สำหรับโปรเจกต์ `eShopLite.WebApp`:

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. รันคำสั่งด้านล่างเพื่อสร้างเครือข่าย:

    ```bash
    docker network create eshoplite
    ```

1. รันคำสั่งด้านล่างเพื่อเปิดภาพคอนเทนเนอร์สำหรับ `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi`:

    ```bash
    docker run -d -p 3000:8080 --network eshoplite --network-alias webapp --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --network eshoplite --network-alias productapi --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --network eshoplite --network-alias weatherapi --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์และไปที่ `http://localhost:3000` and see whether the app is up and running.
1. Navigate to `/weather` or `/products` เพื่อ **ยืนยันว่าหน้าแต่ละหน้าแสดงผลได้ถูกต้อง**
1. รันคำสั่งด้านล่างเพื่อลบคอนเทนเนอร์และเครือข่าย:

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    
    docker network rm eshoplite --force
    ```

1. รันคำสั่งด้านล่างเพื่อลบภาพคอนเทนเนอร์:

    ```bash
    docker rmi eshoplite-webapp:latest --force
    docker rmi eshoplite-productapi:latest --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## การจัดการคอนเทนเนอร์: Docker Compose

ครั้งนี้จะจัดการคอนเทนเนอร์โดยใช้ Docker Compose

1. รันคำสั่งด้านล่างเพื่อสร้างไฟล์ `compose.yaml`:

    ```bash
    # Bash/Zsh
    cd $REPOSITORY_ROOT/workshop
    touch compose.yaml
    ```

    ```powershell
    # PowerShell
    cd $REPOSITORY_ROOT/workshop
    New-Item -Type File -Path compose.yaml -Force
    ```

   > - ไฟล์ `compose.yaml` must be the same location as the `eShopLite.sln` file.

1. `compose.yaml` และใส่เนื้อหาด้านล่าง:

    ```yaml
    name: eShopLite
    
    services:
      productapi:
        container_name: productapi
        build:
          context: .
          dockerfile: ./Dockerfile.productapi
          target: final
        ports:
          - 3030:8080
      weatherapi:
        container_name: weatherapi
        build:
          context: .
          dockerfile: ./Dockerfile.weatherapi
          target: final
        ports:
          - 3031:8080
      webapp:
        container_name: webapp
        build:
          context: .
          dockerfile: ./Dockerfile.webapp
          target: final
        ports:
          - 3000:8080
        depends_on:
          - productapi
          - weatherapi
    ```

1. รันคำสั่งด้านล่างเพื่อเปิดคอนเทนเนอร์:

    ```bash
    docker compose up --build
    ```

1. เปิดเว็บเบราว์เซอร์และไปที่ `http://localhost:3000` and see the app is up and running.
1. Navigate to `/weather` or `/products` and confirm whether to **see each page displayed properly**.
1. In the terminal, press `Ctrl+C` เพื่อหยุดแอปพลิเคชัน
1. รันคำสั่งด้านล่างเพื่อลบคอนเทนเนอร์ เครือข่าย และภาพคอนเทนเนอร์ทั้งหมดในครั้งเดียว:

    ```bash
    docker compose down --rmi local
    ```

---

ยินดีด้วย! เวิร์กช็อป **Creating Dockerfile and Docker Compose File** เสร็จสมบูรณ์แล้ว ไปยังขั้นตอนถัดไป [STEP 02: Testing APIs with Testcontainers](./step-02.md)

**คำปฏิเสธความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติที่ใช้ AI แม้ว่าเราจะพยายามให้การแปลมีความถูกต้อง แต่โปรดทราบว่าการแปลอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้องเกิดขึ้นได้ เอกสารต้นฉบับในภาษาดั้งเดิมควรถือเป็นแหล่งข้อมูลที่เชื่อถือได้ สำหรับข้อมูลที่สำคัญ ขอแนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญที่เป็นมนุษย์ เราจะไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่คลาดเคลื่อนอันเกิดจากการใช้การแปลนี้