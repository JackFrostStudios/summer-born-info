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

This project follows a clean architecture pattern with the following layers:

- **Domain**: Contains business entities and domain logic
- **Features**: Implements use cases and application logic
- **Infrastructure**: Handles data persistence and external dependencies
- **Web**: Contains API controllers and application entry point

## Getting Started

### Prerequisites
- .NET 10.0
- Docker

### Running the Application
1. Navigate to the API directory:
   ```bash
   cd API
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. The API will be available at `https://localhost:5001`


## Testing
The project includes unit tests and integration tests. The integration tests automatically start a postgres database using test containers.

Run all tests:
```bash
dotnet test
```

## Development

This project uses Docker for development environment setup. The `.devcontainer` configuration provides a consistent development environment.
Ensure that your visual studio code has the [dev container extension](https://code.visualstudio.com/docs/devcontainers/containers) installed and open the folder, this should launch the docker containers required and automatically connect.
The Cline plugin is automatically included, AI may be used in the development process but all AI generated changes should be reviewed by a human before submitting a pull request.

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