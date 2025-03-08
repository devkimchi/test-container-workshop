# STEP 00: ตั้งค่าระบบสำหรับการพัฒนา

ในขั้นตอนนี้ เราจะตั้งค่าระบบที่จำเป็นสำหรับการทำเวิร์กช็อป

## สิ่งที่ต้องเตรียมล่วงหน้า

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [git CLI](https://git-scm.com/downloads)
- ติดตั้ง [GitHub CLI](https://cli.github.com/)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

## ตรวจสอบการติดตั้ง .NET SDK

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบว่ามีการติดตั้ง .NET SDK แล้วหรือไม่

    ```bash
    # Bash/Zsh
    which dotnet
    ```

    ```bash
    # PowerShell
    Get-Command dotnet
    ```

   หากไม่พบเส้นทางที่สามารถรัน `dotnet` ได้ แสดงว่ายังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง .NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบเวอร์ชันของ .NET SDK ที่ติดตั้งอยู่

    ```bash
    dotnet --list-sdks
    ```

   ควรมีเวอร์ชัน `9.0.200` หรือสูงกว่า หากไม่ใช่ ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง .NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

1. รันคำสั่งต่อไปนี้เพื่อติดตั้ง HTTPS certificate สำหรับการพัฒนาในเครื่อง

    ```bash
    dotnet dev-certs https --trust
    ```

1. รันคำสั่งต่อไปนี้เพื่ออัปเดตเทมเพลตโครงการ .NET Aspire ให้เป็นเวอร์ชันล่าสุด

    ```bash
    dotnet new install Aspire.ProjectTemplates --force
    ```

1. รันคำสั่งต่อไปนี้เพื่ออัปเดตไลบรารีของ .NET Aspire ให้เป็นเวอร์ชันล่าสุด

    ```bash
    dotnet new update
    ```

## ตรวจสอบการติดตั้ง PowerShell

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบว่ามีการติดตั้ง PowerShell แล้วหรือไม่

    ```bash
    # Bash/Zsh
    which pwsh
    ```

    ```bash
    # PowerShell
    Get-Command pwsh
    ```

   หากไม่พบเส้นทางที่สามารถรัน `pwsh` ได้ แสดงว่ายังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบเวอร์ชันของ PowerShell ที่ติดตั้งอยู่

    ```bash
    pwsh --version
    ```

   ควรมีเวอร์ชัน `7.4.0` หรือสูงกว่า หากไม่ใช่ ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

## ตรวจสอบการติดตั้ง git CLI

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบว่ามีการติดตั้ง git CLI แล้วหรือไม่

    ```bash
    # Bash/Zsh
    which git
    ```

    ```bash
    # PowerShell
    Get-Command git
    ```

   หากไม่พบเส้นทางที่สามารถรัน `git` ได้ แสดงว่ายังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง git CLI](https://git-scm.com/downloads)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบเวอร์ชันของ git CLI ที่ติดตั้งอยู่

    ```bash
    git --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `2.39.5` หากเวอร์ชันต่ำกว่า ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง git CLI](https://git-scm.com/downloads)

## ตรวจสอบการติดตั้ง GitHub CLI

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบว่ามีการติดตั้ง GitHub CLI แล้วหรือไม่

    ```bash
    # Bash/Zsh
    which gh
    ```

    ```bash
    # PowerShell
    Get-Command gh
    ```

   หากไม่พบเส้นทางที่สามารถรัน `gh` ได้ แสดงว่ายังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง GitHub CLI](https://cli.github.com/)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบเวอร์ชันของ GitHub CLI ที่ติดตั้งอยู่

    ```bash
    gh --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `2.62.0` หากเวอร์ชันต่ำกว่า ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง GitHub CLI](https://cli.github.com/)

## ตรวจสอบการติดตั้ง Docker Desktop

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบว่ามีการติดตั้ง Docker Desktop แล้วหรือไม่

    ```bash
    # Bash/Zsh
    which docker
    ```

    ```bash
    # PowerShell
    Get-Command docker
    ```

   หากไม่พบเส้นทางที่สามารถรัน `docker` ได้ แสดงว่ายังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบเวอร์ชันของ Docker Desktop ที่ติดตั้งอยู่

    ```bash
    docker --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `27.3.1` หากเวอร์ชันต่ำกว่า ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)

## ตรวจสอบการติดตั้ง Visual Studio Code

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบว่ามีการติดตั้ง Visual Studio Code แล้วหรือไม่

    ```bash
    # Bash/Zsh
    which code
    ```

    ```bash
    # PowerShell
    Get-Command code
    ```

   หากไม่พบเส้นทางที่สามารถรัน `code` ได้ แสดงว่ายังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ให้ดาวน์โหลดเวอร์ชันล่าสุดจาก [หน้าติดตั้ง Visual Studio Code](https://code.visualstudio.com/)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อตรวจสอบเวอร์ชันของ Visual Studio Code ที่ติดตั้งอยู่

    ```bash
    code --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `1.95.3`입니다. 만약 버전이 낮다면 [Visual Studio Code 설치 페이지](https://code.visualstudio.com/)에서 최신 버전을 다운로드 받아 설치합니다.

   > 만약 터미널에서 `code` หากไม่สามารถรันคำสั่ง `code` ได้ ให้ดู [เอกสารนี้](https://code.visualstudio.com/docs/setup/mac#_launching-from-the-command-line) เพื่อทำการตั้งค่า

## เริ่มต้นใช้งาน Visual Studio Code

1. ย้ายไปยังไดเรกทอรีที่คุณต้องการทำงาน
1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อ fork รีโพสิทอรีนี้ไปยังบัญชี GitHub ของคุณ และ clone ลงบนคอมพิวเตอร์ของคุณ

    ```bash
    gh repo fork devkimchi/test-container-workshop --clone
    ```

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อย้ายไปยังไดเรกทอรีที่ clone มา

    ```bash
    cd test-container-workshop
    ```

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเปิด Visual Studio Code

    ```bash
    code .
    ```

1. เปิดเทอร์มินัลใน Visual Studio Code และรันคำสั่งต่อไปนี้เพื่อตรวจสอบสถานะของรีโพสิทอรีที่ clone มา

    ```bash
    git remote -v
    ```

   เมื่อรันคำสั่งนี้ คุณควรเห็นผลลัพธ์ดังนี้ หากพบ `origin`에 `devkimchi` คุณต้อง clone รีโพสิทอรีใหม่จากรีโพสิทอรีของคุณเอง

    ```bash
    origin  https://github.com/<자신의 GitHub ID>/test-container-workshop.git (fetch)
    origin  https://github.com/<자신의 GitHub ID>/test-container-workshop.git (push)
    upstream        https://github.com/devkimchi/test-container-workshop.git (fetch)
    upstream        https://github.com/devkimchi/test-container-workshop.git (push)
    ```

1. เปิดเทอร์มินัลใน Visual Studio Code และรันคำสั่งต่อไปนี้เพื่อตรวจสอบว่ามีการติดตั้ง [C# Dev Kit Extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) แล้วหรือไม่

    ```bash
    # Bash/Zsh
    code --list-extensions | grep "ms-dotnettools.csdevkit"
    ```

    ```bash
    # PowerShell
    code --list-extensions | Select-String "ms-dotnettools.csdevkit"
    ```

   หากไม่มีข้อความใดๆ ปรากฏ แสดงว่ายังไม่ได้ติดตั้ง ให้รันคำสั่งต่อไปนี้เพื่อติดตั้ง

    ```bash
    code --install-extension "ms-dotnettools.csdevkit" --force
    ```

---

ยินดีด้วย! คุณได้เสร็จสิ้นการตั้งค่าระบบสำหรับการพัฒนาแล้ว ตอนนี้ไปยังขั้นตอนต่อไป [STEP 01: สร้าง Dockerfile และ Docker Compose file](./step-01.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษา AI อัตโนมัติ แม้ว่าเราจะพยายามอย่างเต็มที่เพื่อให้การแปลมีความถูกต้อง โปรดทราบว่าการแปลโดยระบบอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาที่ใช้ควรถือว่าเป็นแหล่งข้อมูลที่ถูกต้องที่สุด สำหรับข้อมูลสำคัญ ขอแนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่ผิดพลาดซึ่งเกิดจากการใช้การแปลนี้