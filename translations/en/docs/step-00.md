# STEP 00: Setting Up the Development Environment

In this step, you will configure the development environment necessary for the workshop.

## Prerequisites

- Install [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Install [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell)
- Install [git CLI](https://git-scm.com/downloads)
- Install [GitHub CLI](https://cli.github.com/)
- Install [Docker Desktop](https://docs.docker.com/get-started/introduction/get-docker-desktop/)
- Install [Visual Studio Code](https://code.visualstudio.com/)

## Verify .NET SDK Installation

1. Run the following command in the terminal to check if .NET SDK is installed.

    ```bash
    # Bash/Zsh
    which dotnet
    ```

    ```bash
    # PowerShell
    Get-Command dotnet
    ```

   If you don’t see a path where `dotnet` can be executed, it means .NET SDK is not installed. If not installed, download the latest version from the [.NET SDK installation page](https://dotnet.microsoft.com/download/dotnet/9.0).

1. Run the following command in the terminal to verify the installed .NET SDK version.

    ```bash
    dotnet --list-sdks
    ```

   The version should be `9.0.200` or higher. If it’s not, download the latest version from the [.NET SDK installation page](https://dotnet.microsoft.com/download/dotnet/9.0).

1. Run the command below to install the local machine development HTTPS certificate.

    ```bash
    dotnet dev-certs https --trust
    ```

1. Run the command below to update the .NET Aspire project template to the latest version.

    ```bash
    dotnet new install Aspire.ProjectTemplates --force
    ```

1. Run the command below to update the .NET Aspire project library to the latest version.

    ```bash
    dotnet new update
    ```

## Verify PowerShell Installation

1. Run the following command in the terminal to check if PowerShell is installed.

    ```bash
    # Bash/Zsh
    which pwsh
    ```

    ```bash
    # PowerShell
    Get-Command pwsh
    ```

   If you don’t see a path where `pwsh` can be executed, it means PowerShell is not installed. If not installed, download the latest version from the [PowerShell installation page](https://learn.microsoft.com/powershell/scripting/install/installing-powershell).

1. Run the following command in the terminal to verify the installed PowerShell version.

    ```bash
    pwsh --version
    ```

   The version should be `7.5.0` or higher. If it’s not, download the latest version from the [PowerShell installation page](https://learn.microsoft.com/powershell/scripting/install/installing-powershell).

## Verify git CLI Installation

1. Run the following command in the terminal to check if git CLI is installed.

    ```bash
    # Bash/Zsh
    which git
    ```

    ```bash
    # PowerShell
    Get-Command git
    ```

   If you don’t see a path where `git` can be executed, it means git CLI is not installed. If not installed, download the latest version from the [git CLI installation page](https://git-scm.com/downloads).

1. Run the following command in the terminal to verify the installed git CLI version.

    ```bash
    git --version
    ```

   As of the time of writing this workshop documentation, the latest version is `2.39.5`. If your version is older, download the latest version from the [git CLI installation page](https://git-scm.com/downloads).

## Verify GitHub CLI Installation

1. Run the following command in the terminal to check if GitHub CLI is installed.

    ```bash
    # Bash/Zsh
    which gh
    ```

    ```bash
    # PowerShell
    Get-Command gh
    ```

   If you don’t see a path where `gh` can be executed, it means GitHub CLI is not installed. If not installed, download the latest version from the [GitHub CLI installation page](https://cli.github.com/).

1. Run the following command in the terminal to verify the installed GitHub CLI version.

    ```bash
    gh --version
    ```

   As of the time of writing this workshop documentation, the latest version is `2.68.1`. If your version is older, download the latest version from the [GitHub CLI installation page](https://cli.github.com/).

## Verify Docker Desktop Installation

1. Run the following command in the terminal to check if Docker Desktop is installed.

    ```bash
    # Bash/Zsh
    which docker
    ```

    ```bash
    # PowerShell
    Get-Command docker
    ```

   If you don’t see a path where `docker` can be executed, it means Docker Desktop is not installed. If not installed, download the latest version from the [Docker Desktop installation page](https://docs.docker.com/get-started/introduction/get-docker-desktop/).

1. Run the following command in the terminal to verify the installed Docker Desktop version.

    ```bash
    docker --version
    ```

   As of the time of writing this workshop documentation, the latest version is `28.0.1`. If your version is older, download the latest version from the [Docker Desktop installation page](https://docs.docker.com/get-started/introduction/get-docker-desktop/).

## Verify Visual Studio Code Installation

1. Run the following command in the terminal to check if Visual Studio Code is installed.

    ```bash
    # Bash/Zsh
    which code
    ```

    ```bash
    # PowerShell
    Get-Command code
    ```

   If you don’t see a path where `code` can be executed, it means Visual Studio Code is not installed. If not installed, download the latest version from the [Visual Studio Code installation page](https://code.visualstudio.com/).

1. Run the following command in the terminal to verify the installed Visual Studio Code version.

    ```bash
    code --version
    ```

   As of the time of writing this workshop documentation, the latest version is `1.98.0`. If your version is older, download the latest version from the [Visual Studio Code installation page](https://code.visualstudio.com/).

   > If you can’t execute the `code` command, refer to [this guide](https://code.visualstudio.com/docs/setup/mac#_launching-from-the-command-line) for configuration.

## Launch Visual Studio Code

1. Navigate to the directory where you will work.
1. Run the following command in the terminal to fork this repository to your GitHub account and clone it to your computer.

    ```bash
    gh repo fork devkimchi/test-container-workshop --clone
    ```

1. Run the following command in the terminal to navigate to the cloned directory.

    ```bash
    cd test-container-workshop
    ```

1. Run the following command in the terminal to launch Visual Studio Code.

    ```bash
    code .
    ```

1. Open the terminal in Visual Studio Code and run the following command to check the clone status of the current repository.

    ```bash
    git remote -v
    ```

   After running this command, you should see the following result. If you see `devkimchi` at `origin`, you need to clone the repository again from your account.

    ```bash
    origin  https://github.com/<Your GitHub ID>/test-container-workshop.git (fetch)
    origin  https://github.com/<Your GitHub ID>/test-container-workshop.git (push)
    upstream        https://github.com/devkimchi/test-container-workshop.git (fetch)
    upstream        https://github.com/devkimchi/test-container-workshop.git (push)
    ```

1. In the Visual Studio Code terminal, run the following command to verify if the [C# Dev Kit extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) is installed.

    ```bash
    # Bash/Zsh
    code --list-extensions | grep "ms-dotnettools.csdevkit"
    ```

    ```bash
    # PowerShell
    code --list-extensions | Select-String "ms-dotnettools.csdevkit"
    ```

   If you don’t see any message, it means the extension is not installed yet. Run the following command to install it.

    ```bash
    code --install-extension "ms-dotnettools.csdevkit" --force
    ```

---

Congratulations! You’ve completed the **Setting Up the Development Environment** exercise. Now, proceed to [STEP 01: Create Dockerfile and Docker Compose Files](./step-01.md).

**Disclaimer**:  
This document has been translated using machine-based AI translation services. While we aim for accuracy, please note that automated translations may include errors or inaccuracies. The original document in its native language should be regarded as the authoritative source. For critical information, professional human translation is advised. We are not responsible for any misunderstandings or misinterpretations resulting from the use of this translation.