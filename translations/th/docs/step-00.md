# STEP 00: การตั้งค่าสภาพแวดล้อมการพัฒนา

ในขั้นตอนนี้ คุณจะตั้งค่าสภาพแวดล้อมการพัฒนาที่จำเป็นสำหรับการทำเวิร์กช็อป

## สิ่งที่ต้องเตรียม

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [git CLI](https://git-scm.com/downloads)
- ติดตั้ง [GitHub CLI](https://cli.github.com/)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

## ตรวจสอบการติดตั้ง .NET SDK

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเช็คว่า .NET SDK ถูกติดตั้งหรือยัง

    ```bash
    # Bash/Zsh
    which dotnet
    ```

    ```bash
    # PowerShell
    Get-Command dotnet
    ```

   หากไม่พบ path ที่สามารถรัน `dotnet` ได้ แสดงว่า .NET SDK ยังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง .NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อยืนยันเวอร์ชันของ .NET SDK ที่ติดตั้งแล้ว

    ```bash
    dotnet --list-sdks
    ```

   เวอร์ชันควรเป็น `9.0.200` หรือสูงกว่า หากไม่ใช่ ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง .NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

1. รันคำสั่งด้านล่างเพื่อทำการติดตั้งใบรับรอง HTTPS สำหรับการพัฒนาบนเครื่อง

    ```bash
    dotnet dev-certs https --trust
    ```

1. รันคำสั่งด้านล่างเพื่ออัปเดตเทมเพลตโครงการ .NET Aspire ให้เป็นเวอร์ชันล่าสุด

    ```bash
    dotnet new install Aspire.ProjectTemplates --force
    ```

1. รันคำสั่งด้านล่างเพื่ออัปเดตไลบรารีโครงการ .NET Aspire ให้เป็นเวอร์ชันล่าสุด

    ```bash
    dotnet new update
    ```

## ตรวจสอบการติดตั้ง PowerShell

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเช็คว่า PowerShell ถูกติดตั้งหรือยัง

    ```bash
    # Bash/Zsh
    which pwsh
    ```

    ```bash
    # PowerShell
    Get-Command pwsh
    ```

   หากไม่พบ path ที่สามารถรัน `pwsh` ได้ แสดงว่า PowerShell ยังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อยืนยันเวอร์ชันของ PowerShell ที่ติดตั้งแล้ว

    ```bash
    pwsh --version
    ```

   เวอร์ชันควรเป็น `7.5.0` หรือสูงกว่า หากไม่ใช่ ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง PowerShell](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)

## ตรวจสอบการติดตั้ง git CLI

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเช็คว่า git CLI ถูกติดตั้งหรือยัง

    ```bash
    # Bash/Zsh
    which git
    ```

    ```bash
    # PowerShell
    Get-Command git
    ```

   หากไม่พบ path ที่สามารถรัน `git` ได้ แสดงว่า git CLI ยังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง git CLI](https://git-scm.com/downloads)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อยืนยันเวอร์ชันของ git CLI ที่ติดตั้งแล้ว

    ```bash
    git --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `2.39.5` หากเวอร์ชันของคุณเก่ากว่า ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง git CLI](https://git-scm.com/downloads)

## ตรวจสอบการติดตั้ง GitHub CLI

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเช็คว่า GitHub CLI ถูกติดตั้งหรือยัง

    ```bash
    # Bash/Zsh
    which gh
    ```

    ```bash
    # PowerShell
    Get-Command gh
    ```

   หากไม่พบ path ที่สามารถรัน `gh` ได้ แสดงว่า GitHub CLI ยังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง GitHub CLI](https://cli.github.com/)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อยืนยันเวอร์ชันของ GitHub CLI ที่ติดตั้งแล้ว

    ```bash
    gh --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `2.68.1` หากเวอร์ชันของคุณเก่ากว่า ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง GitHub CLI](https://cli.github.com/)

## ตรวจสอบการติดตั้ง Docker Desktop

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเช็คว่า Docker Desktop ถูกติดตั้งหรือยัง

    ```bash
    # Bash/Zsh
    which docker
    ```

    ```bash
    # PowerShell
    Get-Command docker
    ```

   หากไม่พบ path ที่สามารถรัน `docker` ได้ แสดงว่า Docker Desktop ยังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อยืนยันเวอร์ชันของ Docker Desktop ที่ติดตั้งแล้ว

    ```bash
    docker --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `28.0.1` หากเวอร์ชันของคุณเก่ากว่า ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)

## ตรวจสอบการติดตั้ง Visual Studio Code

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเช็คว่า Visual Studio Code ถูกติดตั้งหรือยัง

    ```bash
    # Bash/Zsh
    which code
    ```

    ```bash
    # PowerShell
    Get-Command code
    ```

   หากไม่พบ path ที่สามารถรัน `code` ได้ แสดงว่า Visual Studio Code ยังไม่ได้ติดตั้ง หากยังไม่ได้ติดตั้ง ดาวน์โหลดเวอร์ชันล่าสุดจากหน้า [การติดตั้ง Visual Studio Code](https://code.visualstudio.com/)

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อยืนยันเวอร์ชันของ Visual Studio Code ที่ติดตั้งแล้ว

    ```bash
    code --version
    ```

   ณ เวลาที่เขียนเอกสารนี้ เวอร์ชันล่าสุดคือ `1.98.0`. If your version is older, download the latest version from the [Visual Studio Code installation page](https://code.visualstudio.com/).

   > If you can’t execute the `code` หากไม่สามารถรันคำสั่ง `code` ได้ ดู [คู่มือการตั้งค่า](https://code.visualstudio.com/docs/setup/mac#_launching-from-the-command-line) สำหรับการตั้งค่า

## เปิด Visual Studio Code

1. ไปยังไดเรกทอรีที่คุณจะทำงาน
1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อ fork repository นี้ไปยังบัญชี GitHub ของคุณและ clone ลงในคอมพิวเตอร์ของคุณ

    ```bash
    gh repo fork devkimchi/test-container-workshop --clone
    ```

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเข้าสู่ไดเรกทอรีที่ clone มา

    ```bash
    cd test-container-workshop
    ```

1. รันคำสั่งต่อไปนี้ในเทอร์มินัลเพื่อเปิด Visual Studio Code

    ```bash
    code .
    ```

1. เปิดเทอร์มินัลใน Visual Studio Code และรันคำสั่งต่อไปนี้เพื่อตรวจสอบสถานะการ clone ของ repository ปัจจุบัน

    ```bash
    git remote -v
    ```

   หลังจากรันคำสั่งนี้ คุณควรเห็นผลลัพธ์ตามที่กำหนด หากคุณเห็น `devkimchi` at `origin` คุณต้อง clone repository อีกครั้งจากบัญชีของคุณ

    ```bash
    origin  https://github.com/<Your GitHub ID>/test-container-workshop.git (fetch)
    origin  https://github.com/<Your GitHub ID>/test-container-workshop.git (push)
    upstream        https://github.com/devkimchi/test-container-workshop.git (fetch)
    upstream        https://github.com/devkimchi/test-container-workshop.git (push)
    ```

1. ในเทอร์มินัลของ Visual Studio Code ให้รันคำสั่งต่อไปนี้เพื่อยืนยันว่า [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) ถูกติดตั้งแล้ว

    ```bash
    # Bash/Zsh
    code --list-extensions | grep "ms-dotnettools.csdevkit"
    ```

    ```bash
    # PowerShell
    code --list-extensions | Select-String "ms-dotnettools.csdevkit"
    ```

   หากไม่มีข้อความแสดง แสดงว่ายังไม่ได้ติดตั้ง extension นี้ รันคำสั่งต่อไปนี้เพื่อติดตั้ง

    ```bash
    code --install-extension "ms-dotnettools.csdevkit" --force
    ```

---

ยินดีด้วย! คุณได้ทำแบบฝึกหัด **การตั้งค่าสภาพแวดล้อมการพัฒนา** เสร็จเรียบร้อยแล้ว ตอนนี้ไปต่อที่ [STEP 01: สร้าง Dockerfile และ Docker Compose Files](./step-01.md)

**ข้อจำกัดความรับผิดชอบ**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาอัตโนมัติที่ขับเคลื่อนด้วย AI แม้ว่าเราจะพยายามอย่างเต็มที่เพื่อให้การแปลมีความถูกต้อง แต่โปรดทราบว่าการแปลด้วยระบบอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้องเกิดขึ้นได้ เอกสารต้นฉบับในภาษาดั้งเดิมควรถือเป็นแหล่งข้อมูลที่เชื่อถือได้มากที่สุด สำหรับข้อมูลที่สำคัญ แนะนำให้ใช้บริการแปลภาษาจากผู้เชี่ยวชาญที่เป็นมนุษย์ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความผิดที่เกิดจากการใช้การแปลนี้