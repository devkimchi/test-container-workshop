# STEP 01: สร้างไฟล์ Dockerfile และ Docker Compose

ในขั้นตอนนี้ เราจะสร้างไฟล์ `Dockerfile` เพื่อคอนเทนเนอร์ไลซ์แอปแต่ละตัวจากโปรเจกต์พื้นฐาน และไฟล์ `Docker Compose` สำหรับการจัดการออร์เคสเตรชั่นของคอนเทนเนอร์เหล่านั้น

## สิ่งที่ต้องเตรียมล่วงหน้า

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

ตรวจสอบการติดตั้งของแต่ละรายการได้ที่เอกสาร [STEP 00: ตั้งค่าพื้นฐานสำหรับการพัฒนา](./step-00.md)

## คัดลอกโปรเจกต์พื้นฐาน

เราได้เตรียมโปรเจกต์พื้นฐานที่จำเป็นสำหรับเวิร์กชอปนี้ไว้ให้แล้ว ตรวจสอบว่าโปรเจกต์นี้สามารถทำงานได้ตามปกติ โครงสร้างโปรเจกต์พื้นฐานมีดังนี้:

```text
eShopLite
└── src
    ├── eShopLite.WebApp
    ├── eShopLite.WeatherApi
    ├── eShopLite.ProductApi
    ├── eShopLite.ProductData
    └── eShopLite.DataEntities
```

1. เปิดเทอร์มินัลและรันคำสั่งด้านล่างตามลำดับ เพื่อสร้างไดเรกทอรีสำหรับการฝึกและคัดลอกโปรเจกต์พื้นฐาน

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

## บิลด์และรันโปรเจกต์พื้นฐาน

1. ใช้คำสั่งด้านล่างเพื่อบิลด์โปรเจกต์ทั้งหมด

    ```bash
    cd $REPOSITORY_ROOT/workshop

    dotnet restore && dotnet build
    ```

1. เปิดหน้าต่างเทอร์มินัลสามหน้าต่าง และรันคำสั่งด้านล่างในแต่ละหน้าต่างเพื่อรันแอปพลิเคชัน

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

   > ในบางครั้ง เทอร์มินัลอาจไม่รู้จักค่า `$REPOSITORY_ROOT` ให้รันคำสั่งด้านล่างในแต่ละเทอร์มินัลอีกครั้ง
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

1. เว็บเบราว์เซอร์จะเปิดโดยอัตโนมัติที่ `https://localhost:7000` 또는 `http://localhost:5000` 주소로 접속합니다. 만약 자동으로 웹 브라우저가 열리지 않았다면 수동으로 주소를 입력합니다.
1. 웹브라우저에서 `/weather` 또는 `/products` 경로로 접속하셔 각각 페이지가 정상적으로 보이는지 확인합니다.
1. 각 터미널 창에서 `Ctrl`+`C` กดเพื่อหยุดแอปพลิเคชัน
1. ปิดเทอร์มินัลทั้งหมด เหลือเพียงหน้าต่างเดียว

## สร้างอิมเมจคอนเทนเนอร์ด้วย `Dockerfile`

โปรเจกต์พื้นฐานมีการพึ่งพากันระหว่างแอปพลิเคชัน ซึ่งโครงสร้างการพึ่งพานี้มีดังนี้:

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

ดังนั้น การสร้างอิมเมจคอนเทนเนอร์สำหรับแต่ละแอปพลิเคชัน ต้องแก้ไขการพึ่งพาเหล่านี้ก่อน

> จากจุดนี้ ต้องเปิดใช้งานแอป Docker Desktop หากไม่เห็นไอคอน Docker บน Taskbar ให้เปิด Docker Desktop ขึ้นมา

### สร้าง `Dockerfile`: `eShopLite.WebApp`

1. ไปยังไดเรกทอรีรูทของเวิร์กชอป

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. ใช้คำสั่งด้านล่างเพื่อสร้างไฟล์ `Dockerfile.webapp`

    ```bash
    # Bash/Zsh
    touch Dockerfile.webapp
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path Dockerfile.webapp -Force
    ```

1. เปิดไฟล์ `Dockerfile.webapp` และใส่เนื้อหาด้านล่าง

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

   > **หมายเหตุ**: `Dockerfile.webapp`은 `eShopLite.WebApp` ใช้สำหรับบิลด์โปรเจกต์เป็นอิมเมจคอนเทนเนอร์
   > 
   > - `Dockefile.webapp`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.
   > - `eShopLite.WebApp` 프로젝트와 `eShopLite.DataEntities` 프로젝트를 모두 복사해서 의존성을 해결합니다.

1. 아래 명령어를 실행시켜 `eShopLite.WebApp` เป็นส่วนที่เกี่ยวข้องในกระบวนการบิลด์

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. ใช้คำสั่งด้านล่างเพื่อรันอิมเมจคอนเทนเนอร์ที่บิลด์แล้ว

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    ```

1. เปิดเว็บเบราว์เซอร์ที่ `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` และตรวจสอบว่าเกิดข้อผิดพลาดในแต่ละหน้า

   > ในขั้นตอนนี้ ควรเกิดข้อผิดพลาด

1. ใช้คำสั่งด้านล่างเพื่อหยุดคอนเทนเนอร์และลบคอนเทนเนอร์รวมถึงอิมเมจคอนเทนเนอร์

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    docker rmi eshoplite-webapp:latest --force
    ```

### สร้าง `Dockerfile`: `eShopLite.ProductApi`

1. ไปยังไดเรกทอรีรูทของเวิร์กชอป

    ```bash
    cd $REPOSITORY_ROOT/workshop
    ```

1. ใช้คำสั่งด้านล่างเพื่อสร้างไฟล์ `Dockerfile.productapi`

    ```bash
    # Bash/Zsh
    touch Dockerfile.productapi
    ```

    ```powershell
    # PowerShell
    New-Item -Type File -Path Dockerfile.productapi -Force
    ```

1. เปิดไฟล์ `Dockerfile.productapi` และใส่เนื้อหาด้านล่าง

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

   > **หมายเหตุ**: `Dockerfile.productapi`은 `eShopLite.ProductApi` ใช้สำหรับบิลด์โปรเจกต์เป็นอิมเมจคอนเทนเนอร์
   > 
   > - `Dockefile.productapi`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.
   > - `eShopLite.ProductApi` 프로젝트와 `eShopLite.ProductData`, `eShopLite.DataEntities` 프로젝트를 모두 복사해서 의존성을 해결합니다.

1. 아래 명령어를 실행시켜 `eShopLite.ProductApi` เป็นส่วนที่เกี่ยวข้องในกระบวนการบิลด์

    ```bash
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    ```

1. ใช้คำสั่งด้านล่างเพื่อรันอิมเมจคอนเทนเนอร์ที่บิลด์แล้ว

    ```bash
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์ที่ `http://localhost:3030` 주소로 접속하여 **404 에러가 보이는지 확인합니다**.
1. `/api/products` และตรวจสอบว่าแสดงข้อมูลได้ถูกต้อง
1. ใช้คำสั่งด้านล่างเพื่อหยุดคอนเทนเนอร์และลบคอนเทนเนอร์รวมถึงอิมเมจคอนเทนเนอร์

    ```bash
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    docker rmi eshoplite-productapi:latest --force
    ```

### สร้าง `Dockerfile`: `eShopLite.WeatherApi`

> **🚨🚨🚨 도전‼️ 🚨🚨🚨**
> 
> 위의 `Dockerfile.webapp`과 `Dockerfile.productapi` 작성 방법을 참고하여 `eShopLite.WeatherApi` 프로젝트를 컨테이너 이미지로 빌드하는 `Dockerfile.weatherapi`를 작성해보세요.
>
> - `Dockerfile.weatherapi`는 `eShopLite.WeatherApi` ใช้สำหรับบิลด์โปรเจกต์เป็นอิมเมจคอนเทนเนอร์
> - `Dockefile.weatherapi`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.

`Dockerfile.weatherapi`를 작성했다면 아래 순서대로 실행해보세요.

1. 아래 명령어를 실행시켜 `eShopLite.WeatherApi` เป็นส่วนที่เกี่ยวข้องในกระบวนการบิลด์

    ```bash
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. ใช้คำสั่งด้านล่างเพื่อรันอิมเมจคอนเทนเนอร์ที่บิลด์แล้ว

    ```bash
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์ที่ `http://localhost:3031` 주소로 접속하여 **404 에러가 보이는지 확인합니다**.
1. `/api/weatherforecast` และตรวจสอบว่าแสดงข้อมูลได้ถูกต้อง
1. ใช้คำสั่งด้านล่างเพื่อหยุดคอนเทนเนอร์และลบคอนเทนเนอร์รวมถึงอิมเมจคอนเทนเนอร์

    ```bash
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## การจัดการออร์เคสเตรชั่นคอนเทนเนอร์: Docker Network

ใช้ `Dockerfile` ทั้งสามไฟล์ที่สร้างไว้ก่อนหน้านี้เพื่อจัดการออร์เคสเตรชั่นคอนเทนเนอร์

1. ใช้คำสั่งด้านล่างเพื่อบิลด์อิมเมจคอนเทนเนอร์สำหรับ `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi`

    ```bash
    cd $REPOSITORY_ROOT/workshop
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    docker build . -f ./Dockerfile.productapi -t eshoplite-productapi:latest
    docker build . -f ./Dockerfile.weatherapi -t eshoplite-weatherapi:latest
    ```

1. ใช้คำสั่งด้านล่างเพื่อรันอิมเมจคอนเทนเนอร์ที่บิลด์แล้ว

    ```bash
    docker run -d -p 3000:8080 --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์ที่ `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` และตรวจสอบว่าเกิดข้อผิดพลาดในแต่ละหน้า

   > ในขั้นตอนนี้ ควรเกิดข้อผิดพลาด

1. ใช้คำสั่งด้านล่างเพื่อหยุดและลบคอนเทนเนอร์ทั้งหมด

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force

    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force

    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    ```

1. เปิดไฟล์ `src/eShopLite.WebApp/Program.cs` และแก้ไขโค้ดตามด้านล่าง

    ```csharp
    // 변경 전
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
    // 변경 후
    builder.Services.AddHttpClient<ProductApiClient>(client =>
    {
        client.BaseAddress = new("http://productapi:8080");
    });
    
    builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        client.BaseAddress = new("http://weatherapi:8080");
    });
    ```

1. บิลด์อิมเมจคอนเทนเนอร์ของโปรเจกต์ `eShopLite.WebApp` อีกครั้ง

    ```bash
    docker build . -f ./Dockerfile.webapp -t eshoplite-webapp:latest
    ```

1. ใช้คำสั่งด้านล่างเพื่อสร้างเครือข่ายใหม่

    ```bash
    docker network create eshoplite
    ```

1. ใช้คำสั่งด้านล่างเพื่อรันอิมเมจคอนเทนเนอร์สำหรับ `eShopLite.WebApp`, `eShopLite.ProductApi`, `eShopLite.WeatherApi`

    ```bash
    docker run -d -p 3000:8080 --network eshoplite --network-alias webapp --name eshoplite-webapp eshoplite-webapp:latest
    docker run -d -p 3030:8080 --network eshoplite --network-alias productapi --name eshoplite-productapi eshoplite-productapi:latest
    docker run -d -p 3031:8080 --network eshoplite --network-alias weatherapi --name eshoplite-weatherapi eshoplite-weatherapi:latest
    ```

1. เปิดเว็บเบราว์เซอร์ที่ `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` และตรวจสอบว่าแต่ละหน้าทำงานได้ตามปกติ
1. ใช้คำสั่งด้านล่างเพื่อหยุดและลบคอนเทนเนอร์และเครือข่าย

    ```bash
    docker stop eshoplite-webapp
    docker rm eshoplite-webapp --force
    
    docker stop eshoplite-productapi
    docker rm eshoplite-productapi --force
    
    docker stop eshoplite-weatherapi
    docker rm eshoplite-weatherapi --force
    
    docker network rm eshoplite --force
    ```

1. ใช้คำสั่งด้านล่างเพื่อลบอิมเมจคอนเทนเนอร์

    ```bash
    docker rmi eshoplite-webapp:latest --force
    docker rmi eshoplite-productapi:latest --force
    docker rmi eshoplite-weatherapi:latest --force
    ```

## การจัดการออร์เคสเตรชั่นคอนเทนเนอร์: Docker Compose

ในขั้นตอนนี้ เราจะใช้ Docker Compose ในการจัดการออร์เคสเตรชั่นคอนเทนเนอร์

1. ใช้คำสั่งด้านล่างเพื่อสร้างไฟล์ `compose.yaml`

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

   > - เปิดไฟล์ `compose.yaml`의 위치는 `eShopLite.sln` 파일의 위치와 동일한 디렉토리에 있어야 합니다.

1. `compose.yaml` และใส่เนื้อหาด้านล่าง

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

1. ใช้คำสั่งด้านล่างเพื่อรันคอนเทนเนอร์

    ```bash
    docker compose up --build
    ```

1. เปิดเว็บเบราว์เซอร์ที่ `http://localhost:3000` 주소로 접속하여 페이지가 정상적으로 보이는지 확인합니다.
1. `/weather` 또는 `/products` 경로로 접속하여 **각 페이지가 정상적으로 보이는지 확인합니다**.
1. 터미널 창에서 `Ctrl`+`C` กดเพื่อหยุดแอปพลิเคชัน
1. ใช้คำสั่งด้านล่างเพื่อหยุดและลบคอนเทนเนอร์ เครือข่าย และอิมเมจคอนเทนเนอร์ทั้งหมดในครั้งเดียว

    ```bash
    docker compose down --rmi local
    ```

---

ยินดีด้วย! คุณได้เสร็จสิ้นการฝึก **การสร้างไฟล์ Dockerfile และ Docker Compose** แล้ว ตอนนี้ไปต่อที่ [STEP 02: ทดสอบ API ด้วย Testcontainers](./step-02.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติที่ขับเคลื่อนด้วย AI แม้ว่าเราจะพยายามอย่างเต็มที่เพื่อให้การแปลมีความถูกต้อง แต่โปรดทราบว่าการแปลโดยระบบอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาดั้งเดิมควรถือเป็นแหล่งข้อมูลที่เชื่อถือได้มากที่สุด สำหรับข้อมูลสำคัญ แนะนำให้ใช้บริการแปลภาษาจากมืออาชีพ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความผิดที่เกิดจากการใช้การแปลนี้