# Automated Custom Connector

This is a show-case of automating maintaining a Power Platform Custom Connector from a .NET minimal web api, it can easily be adjusted to any _type_ of .NET API, and the pipeline can _easily_ be adjusted to acquire an OpenAPI document from another type of API or another pipeline platform than Azure DevOps.

## Highlights

- Operation ID is generated based on method, path and query parameters (`src/Program.cs:18,29`).
- Web API is configured to output OpenAPI document as Swagger 2.0 at build-time (`src/WebApi.csproj:9,17-21`).
- Web API is configured to output OpenAPI document as both OpenAPI 3.0 and Swagger 2.0 at runtime (`src/Program.cs:15`).
- Sample Azure DevOps pipeline to automate the process of updating the Custom Connector (`.azdo/sample-pipeline.yml`).
- Sample PowerShell script to automate the process of updating the Custom Connector (`.azdo/sample-script.ps1`).


The web api is modified from [Tutorial: Create a minimal API with ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api).

