# การอบรมเชิงปฏิบัติการ Testcontainers

การทดสอบแอปพลิเคชันในเครื่องก่อนที่จะนำไปใส่ในคอนเทนเนอร์นั้นไม่ใช่เรื่องยากมากนัก แต่ถ้าแอปพลิเคชันนั้นถูกสร้างเป็นคอนเทนเนอร์แล้วล่ะ? การทดสอบในเครื่องจะซับซ้อนมากขึ้น ด้วย Testcontainers คุณสามารถทดสอบแอปพลิเคชันที่อยู่ในคอนเทนเนอร์ผ่านโค้ดได้ เหมือนกับการเขียน Unit Test นอกจากนี้ .NET Aspire ยังช่วยให้การทดสอบในสถานการณ์ที่มีการจัดการคอนเทนเนอร์เป็นเรื่องง่ายขึ้น

## เป้าหมายของการอบรม

- เรียนรู้วิธีการใส่แอปพลิเคชันในคอนเทนเนอร์โดยใช้ Dockerfile
- เรียนรู้วิธีการจัดการคอนเทนเนอร์สำหรับแอปพลิเคชันโดยใช้ Docker Compose
- ทดสอบแอปพลิเคชันในคอนเทนเนอร์โดยใช้ Testcontainers
- ใช้ .NET Aspire เพื่อทำให้การจัดการคอนเทนเนอร์ง่ายขึ้น
- ใช้ .NET Aspire เพื่อทดสอบแอปพลิเคชันในสถานการณ์การจัดการคอนเทนเนอร์ได้อย่างสะดวก
- ทดสอบ UI โดยใช้ Playwright

## สิ่งที่ต้องเตรียมก่อนการอบรม

- ติดตั้ง [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- ติดตั้ง [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- ติดตั้ง [git CLI](https://git-scm.com/downloads)
- ติดตั้ง [GitHub CLI](https://cli.github.com/)
- ติดตั้ง [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- ติดตั้ง [Visual Studio Code](https://code.visualstudio.com/)

## คำแนะนำในการอบรม

- การอบรมจะดำเนินไปทีละขั้นตอน โดยแต่ละขั้นตอนคุณจะเรียนรู้ด้วยตัวเองตามที่กำหนดไว้ โดยมีผู้สอนให้คำแนะนำสั้นๆ ก่อนเริ่มแต่ละขั้นตอน

  | ลำดับ                         | หัวข้อ                                           |
  |-------------------------------|-------------------------------------------------|
  | [STEP 00](./docs/step-00.md) | การตั้งค่าสภาพแวดล้อมสำหรับการพัฒนา            |
  | [STEP 01](./docs/step-01.md) | การสร้างไฟล์ Dockerfile และ Docker Compose      |
  | [STEP 02](./docs/step-02.md) | การทดสอบ API ด้วย Testcontainers                |
  | [STEP 03](./docs/step-03.md) | การทดสอบ Integration ด้วย Testcontainers        |
  | [STEP 04](./docs/step-04.md) | การจัดการคอนเทนเนอร์ด้วย .NET Aspire           |
  | [STEP 05](./docs/step-05.md) | การทดสอบ Integration ด้วย .NET Aspire           |

- คุณสามารถดูผลลัพธ์ที่บันทึกไว้ในแต่ละขั้นตอนได้ที่ [Save Points](../../../../save-points)

## เอกสารในภาษาอื่นๆ

การอบรมนี้มีให้บริการในภาษาต่อไปนี้!

| ภาษา     | รหัส      | ลิงก์เอกสาร                              | อัปเดตล่าสุด |
|----------|-----------|--------------------------------------------|--------------|
| 한국어  | ko        | [한국어 문서](../../README.md)                 | 2025-03-09  |
| English | en        | [English Translation](../en/README.md)       | 2025-03-09  |
| ไทย     | th        | [การแปลภาษาไทย](./README.md) | 2025-03-09  |


## เอกสารอ้างอิงเพิ่มเติม

- [Testcontainers](https://www.testcontainers.org/) – [.NET](https://dotnet.testcontainers.org/)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)
- [Playwright](https://playwright.dev/) – [.NET](https://playwright.dev/dotnet/)

**คำเตือน**:  
เอกสารนี้ได้รับการแปลโดยใช้บริการแปลภาษาด้วย AI อัตโนมัติ แม้ว่าเราจะพยายามให้มีความถูกต้องมากที่สุด แต่โปรดทราบว่าการแปลด้วยระบบอัตโนมัติอาจมีข้อผิดพลาดหรือความไม่ถูกต้อง เอกสารต้นฉบับในภาษาดั้งเดิมควรถูกพิจารณาเป็นแหล่งข้อมูลที่เชื่อถือได้ สำหรับข้อมูลที่สำคัญ แนะนำให้ใช้บริการแปลภาษาโดยมืออาชีพ เราไม่รับผิดชอบต่อความเข้าใจผิดหรือการตีความที่เกิดจากการใช้การแปลนี้