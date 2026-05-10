# Summer Born Information

## Table of Contents
- [Introduction](#introduction)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)
- [FAQ](#faq)

## Introduction
This project provides a platform for UK parents of summer born children to access information about school admissions and delayed start options.

Summer born children are those born between April 1st and August 31st in the UK. Parents of these children often face challenges when deciding whether to start their child in school at the standard time or request a delayed start.

The platform aims to:
- Provide clear information about compulsory school age requirements
- Share experiences from other parents who have requested delayed starts
- Offer a content management system for administrators to create and manage informational articles
- Create a community where parents can connect and share their experiences


## Architecture

This project follows a vertical slice architecture. Each feature keeps its request handlers, validation, endpoint registration, and supporting code close together so related behaviour stays self-contained instead of being split across shared service layers.

The solution is split into a few clear projects:

- **Domain**: Contains entities, value objects, and domain rules that should not depend on infrastructure concerns.
- **Features**: Contains the application use cases and feature-specific API surface. In general, a feature should be self-contained here.
- **Infrastructure**: Contains database access, queue integration, large object or file storage, and other external system integrations.
- **Web**: Hosts the ASP.NET Core application and wires up the feature endpoints.
- **AppHost**: Uses Aspire to compose the local development environment and start the dependencies the app needs.

As a rule of thumb, put domain logic in `Domain`, application behaviour in `Features`, and anything that talks to PostgreSQL, queues, or external storage in `Infrastructure`.

## Getting Started

### Prerequisites
- .NET 10.0
- Visual Studio with Aspire support
- Docker Desktop or another compatible container runtime

### Running the Application
1. Open `API/SummerBornInfo.sln` in Visual Studio.
2. Set `SummerBornInfo.AppHost` as the startup project.
3. Start debugging or run the solution.

Visual Studio will start the Aspire app host, which in turn launches the PostgreSQL environment and the `SummerBornInfo.Web` API. The API also performs its development startup work in `Web/Program.cs`, including database creation and queue initialisation when running in Development.

If you prefer the command line, you can run the app host from the `API` folder with:

```bash
dotnet run --project SummerBornInfo.AppHost/SummerBornInfo.AppHost.AppHost/SummerBornInfo.AppHost.csproj
```


## Testing
The solution includes domain, infrastructure, feature, and web integration test projects. Tests are written with xUnit and are intended to be run through Visual Studio's Test Explorer / test runner.

Run all tests from Visual Studio, or from the command line with:

```bash
dotnet test API/SummerBornInfo.sln
```

The integration test projects use Testcontainers to provision PostgreSQL when required, so no separate manual database setup should be necessary for test runs.

## Development

Development is centered around Visual Studio and Aspire:

- Open the solution in Visual Studio and run `SummerBornInfo.AppHost` to start the local environment.
- Use the Solution Explorer and project references to work feature by feature.
- Keep changes vertical: feature code should stay in `Features` unless it truly belongs in `Domain` or `Infrastructure`.
- Use the Visual Studio test runner to execute and debug tests while iterating.

The app host brings up PostgreSQL and the web app together, which is the fastest way to get a consistent local stack running.

## Contributing

We welcome contributions from the community! Here's how you can help:

1. **Report Issues**: If you find any bugs or have suggestions, please open an issue in our GitHub repository.
2. **Submit Pull Requests**: Create a feature branch, add the feature you think is important and submit a pull request.
3. **Code Guidelines**: Please ensure your code follows our existing patterns and includes appropriate tests.
4. **Documentation**: Help us improve our documentation by suggesting edits or adding new content.

## License

This project is licensed under the GNU General Public License - see the [LICENSE](./LICENSE) file for details.

## FAQ

### What are summer born children?
Summer born children are those born between April 1st and August 31st in the UK educational system.

### Can I request a delayed start for my summer born child?
Yes, parents have the right to request that their summer born child starts school one year later than the standard intake. However, this request must be approved by the local authority.

### What are the benefits of delayed start?
Potential benefits include:
- Better emotional and social development
- Improved academic readiness
- Reduced pressure on young children
- Better alignment with developmental stages

### How do I apply for delayed start?
The application process varies by local authority. Generally, you need to:
1. Contact your preferred schools
2. Submit a formal request to the local authority
3. Provide supporting evidence if required
4. Wait for the decision

### Where can I find more information?
For more information about summer born admissions, visit the [UK Government's website](https://www.gov.uk/government/publications/summer-born-children-school-admission) or contact your local authority's education department.
The website will allow parents to share their experience of requesting delayed start for a specific schools or Authorities.
