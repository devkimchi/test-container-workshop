# Testcontainers Workshop

<!--
Please include the following content in this README file:

- Workshop introduction
- Workshop goals
- Workshop prerequisites
- Workshop instructions
- Additional reference materials
-->

Testing an application locally before containerizing it isn’t too challenging. But what if the app has already been built as a container? Testing it locally becomes more complex. With Testcontainers, you can test containerized applications using code, just like writing unit tests. Additionally, .NET Aspire makes it easy to test even in container orchestration scenarios.

## Workshop Goals

- Learn how to containerize an existing application using a Dockerfile.
- Learn how to orchestrate containers for an existing application using Docker Compose.
- Test containerized applications using Testcontainers.
- Use .NET Aspire to simplify container orchestration.
- Use .NET Aspire to easily test applications in container orchestration scenarios.
- Perform UI testing using Playwright.

## Workshop Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [git CLI](https://git-scm.com/downloads)
- Install [GitHub CLI](https://cli.github.com/)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

## Workshop Instructions

- The workshop will be conducted step by step. For each step, you will engage in self-paced learning, with the facilitator providing brief guidance before starting each phase.

  | Order                         | Title                                         |
  |-------------------------------|-----------------------------------------------|
  | [STEP 00](./docs/step-00.md) | Setting up the development environment        |
  | [STEP 01](./docs/step-01.md) | Creating Dockerfile and Docker Compose files  |
  | [STEP 02](./docs/step-02.md) | Testing APIs with Testcontainers              |
  | [STEP 03](./docs/step-03.md) | Performing integration tests with Testcontainers |
  | [STEP 04](./docs/step-04.md) | Orchestrating containers with .NET Aspire     |
  | [STEP 05](./docs/step-05.md) | Conducting integration tests with .NET Aspire |

- You can find intermediate results for each step at [Save Points](../../save-points).

## Documentation in Other Languages

This workshop is also available in the following languages!

| Language | Code      | Document Link                              | Last Updated |
|----------|-----------|--------------------------------------------|--------------|
| English  | en        | [English Translation](./README.md) | 2025-03-09   |
| Thai     | th        | [Thai Translation](../th/README.md)     | 2025-03-09   |

## Additional Reference Materials

- [Testcontainers](https://www.testcontainers.org/) – [.NET](https://dotnet.testcontainers.org/)
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview)
- [Playwright](https://playwright.dev/) – [.NET](https://playwright.dev/dotnet/)

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While we strive for accuracy, please note that automated translations may contain errors or inaccuracies. The original document in its native language should be regarded as the authoritative source. For critical information, professional human translation is recommended. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.