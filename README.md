### Service Generator CLI

ServiceGeneratorWebbilir is a command-line interface (CLI) tool designed for internal use in [Webbilir](https://webbilir.com/). Package aim to automate the creation of services and implementations in the Unit of Work pattern.

![Nuget](https://img.shields.io/nuget/v/ServiceGeneratorWebbilir)

[Package Page](https://www.nuget.org/packages/ServiceGeneratorWebbilir/)

#### Installation

To install ServiceGeneratorWebbilir, follow these steps:

1. Open your terminal.
2. Run the following command:

   ```bash
   dotnet tool install --global ServiceGeneratorWebbilir --version 1.1.2
   ```

   This command installs the tool globally on your system.
   
#### What Service Generator CLI Does

1. Abstract and Concrete Generation: Automatically generates service interface (I{ServiceName}Service.cs) and Concrete (Ef{ServiceName}Service.cs) files.
2. Unit of Work Updates: Updates the IUnitOfWork and EfUnitOfWork.cs files to include new service dependencies, ensuring the application follows the Unit of Work design pattern.
3. Project Formatting: Automates project formatting using the dotnet format command, promoting code consistency across the project.

Overall, this tool aims to streamline the development process by automating the creation of boilerplate service and implementation files.

#### Usage

Once installed, you can use ServiceGeneratorWebbilir to generate files and code by executing commands like `service generate Blog`.

#### Documentation

For more information and detailed usage instructions, visit the [ServiceGeneratorWebbilir NuGet package page](https://www.nuget.org/packages/ServiceGeneratorWebbilir).
