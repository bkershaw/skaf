namespace Skaf.cli;

public static class InitializeExample
{
  public static void Create(string directoryPath)
  {
    var filePath = Path.Combine(directoryPath, "structure.yaml");

    if (File.Exists(filePath))
    {
      Console.WriteLine("structure.yaml already exists. Aborting.");
      return;
    }

    var exampleYaml = GetExampleYaml();
    File.WriteAllText(filePath, exampleYaml);

    Console.WriteLine($"structure.yaml created at: {filePath}");
  }

  private static string GetExampleYaml()
        {
            return """
                   devops:
                     pipelines:
                       - azure-pipelines.yml
                       - build.yml
                   
                   services:
                     - name: ExampleService
                       solutionName: ExampleService
                       src:
                         namespace: Company.Project.Services.ExampleService
                         projects:
                           - name: Api
                             type: dotnet
                             version: net9.0
                             template: webapi
                             dependsOn:
                               - Application
                               - Infrastructure
                             packages:
                               - name: Microsoft.AspNetCore.Authentication.JwtBearer
                                 version: 6.0.0
                               - name: AutoMapper.Extensions.Microsoft.DependencyInjection
                           - name: Application
                             type: dotnet
                             version: net9.0
                             template: classlib
                             dependsOn:
                               - Domain
                           - name: Domain
                             type: dotnet
                             version: net9.0
                             template: classlib
                           - name: Infrastructure
                             type: dotnet
                             version: net9.0
                             template: classlib
                             dependsOn:
                               - Application
                               - Domain
                       tests:
                         namespace: Company.Project.Services.OrderService.Tests
                         generateForEachProject: true
                         template: xunit
                   
                   components:
                     - name: ExampleProcessor
                       solutionName: ExampleProcessor
                       src:
                         namespace: Company.Project.Components.ExampleProcessor
                         projects:
                           - name: Runtime
                             type: dotnet
                             version: net9.0
                             template: worker
                             dependsOn:
                               - Application
                               - Infrastructure
                           - name: Application
                             type: dotnet
                             version: net9.0
                             template: classlib
                             dependsOn:
                               - Domain
                           - name: Domain
                             type: dotnet
                             version: net9.0
                             template: classlib
                           - name: Infrastructure
                             type: dotnet
                             version: net9.0
                             template: classlib
                             dependsOn:
                               - Application
                               - Domain
                       tests:
                         namespace: Company.Project.Components.ExampleProcessor.Tests
                         generateForEachProject: true
                         template: xunit
                   
                   web:
                     - name: ExampleReact
                       solutionName: ExampleReact
                       type: react
                       namespace: Company.Project.Web.ExampleReact
                       command: npm create vite@latest example-react -- --template react-ts
                       postCreate:
                         - npm install
                   """;
        }
    }