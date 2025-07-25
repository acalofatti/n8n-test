
# PersonalCreditCollectionsWorker

A .NET 8 Worker Service that publishes personal credit collection events to a RabbitMQ queue..

## ?? Overview

This service connects to a SQL Server database, retrieves pending credit collection records using stored procedures, and publishes them to a RabbitMQ queue for downstream processing.

It is part of the modernization initiative for the **Personal Credit** domain within the RPV ecosystem.

## ?? Technologies Used

- [.NET 8](https://dotnet.microsoft.com/en-us/)
- [RabbitMQ](https://www.rabbitmq.com/)
- SQL Server
- Worker Service Template (`IHostedService`)

## ?? Configuration

Settings are defined in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "defaultConnection": "Data Source=your-sql-server;Initial Catalog=Middleware;User Id=sa;Password=your-password;Encrypt=True;TrustServerCertificate=True"
  },
  "RabbitMq": {
    "HostName": "your-rabbitmq-host",
    "UserName": "admin",
    "Password": "your-password",
    "AMQPPort": 5672,
    "VirtualHost": "/",
    "QueueName": "cobranzas_creditos_personales"
  }
}
?? Running the Project
Visual Studio
Open PersonalCreditCollectionsWorker.sln

Set the Worker project as startup

Press F5 or run with Ctrl + F5

CLI
dotnet build
dotnet run --project PersonalCreditCollectionsWorker

?? Project Structure
pgsql

/Config          ? Configuration models (e.g., RabbitMqSettings)
/Contracts       ? Interfaces (e.g., IQueuePublisher, ISqlDataExtractor)
/Infrastructure  ? Implementations for DB and RabbitMQ access
/Models          ? Domain models (e.g., Novedad)
/Services        ? Business logic (e.g., NovedadesProcessor)
Program.cs       ? Entry point and DI setup
Worker.cs        ? Background loop that runs the processor

? Features
Worker runs continuously to check and publish collection data

Integration with RabbitMQ using queue declaration and publishing

SQL Server data extraction using stored procedures

Configurable via appsettings.json

Supports dependency injection and logging

?? Future Improvements
Add retry policies or dead-letter queue support

Add Serilog for structured logging

Expose health checks or metrics endpoint

Dockerize for deployment

?? Author
Ariel Calofatti
