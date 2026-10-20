# Cyber

Cyber is a .NET application framework focused on building lightweight web APIs with source generation and Native AOT in mind.

## Features

- **Native AOT-oriented design** — supports building applications with trimming and ahead-of-time compilation in mind.
- **Minimal API development** — provides conventions and helpers for defining HTTP endpoints with less boilerplate.
- **Source generation** — moves supported work to compile time to reduce repetitive code and runtime overhead.
- **Authentication support** — includes configurable authentication options for common application scenarios.
- **Dependency injection and HTTP clients** — integrates with the .NET hosting and service infrastructure.
- **Extensible architecture** — organized into `Cyber` and `Cyber.Common` projects.

## Requirements

- .NET SDK 10.0 or later for the current development setup.
- A compatible target platform and runtime identifier for your deployment environment.

## Getting Started

Clone the repository:

```bash
git clone https://github.com/NexoraForge01s/Cyber.git
cd Cyber
```

Open the solution in your preferred .NET IDE, or build the project with the .NET CLI:

```bash
dotnet build
```

> The exact startup and configuration steps depend on the application you create with Cyber. See the source code and project files for the APIs and configuration options available in the current version.

## Project Structure

- `Cyber/` — core framework and source-generation-related components.
- `Cyber.Common/` — shared runtime components and utilities.

## Compatibility

Cyber is designed for modern .NET applications. Native AOT compatibility can vary by feature and dependency, so validate your application's publish output for the target platform.

## Contributing

Issues and pull requests are welcome. Please include a clear description of the change and relevant reproduction steps or tests when applicable.

## License

Cyber is licensed under the [Apache License 2.0](LICENSE).
