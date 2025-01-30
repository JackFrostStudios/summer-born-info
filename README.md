[![license](https://img.shields.io/github/license/JackFrostStudios/summer-born-info?style=plastic&logo=GitHub&logoColor=black&labelColor=DDDDDD&color=627254)](https://github.com/JackFrostStudios/summer-born-info)

# *Summer Born Info*

Summer Born Info is a project with the aim of providing more information to parents of Summer Born Children.

Children born from 1 April to 31 August do not have to start school until the September after their fifth birdthday meaning they could have missed a full year of school.

To enable parents to access their right to start a child at compulsory school age parents can choose to delay school start and join reception the following year.

There are a lot of myths and confusion around this process, the aim of this project is to provide a single source to find information as well as providing a system for parent to share their specific experiences with local schools.


## Getting Started

### Project Outline

- Design:
    - The project has been written with a vertical slice architecture in mind.
    - Each endpoint is mostly self contained with database entities and configuration being shared across endpoints.
    - An interface between database access (or repository pattern implementation) has not been used to keep the project simple and allow each endpoint to directly access the methods offered by Entity Framework. 
- Technology
    - The web api uses the [Fast Endpoints](https://github.com/FastEndpoints/FastEndpoints) library to define the API.
    - Entity Framework is used for database access.
    - Test Containers allow integration tests to be executed against a real database.
- Code Quality and Style
    - The Roslyn analyzers should be used as a guide for code quality and style.
    - Any analyzer issues must be resolved before raising a pull request.