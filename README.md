# BringYourOwnAI (BYOAI-2)

BringYourOwnAI is a modern, out-of-process Visual Studio extension that integrates advanced AI capabilities directly into your development workflow. Built with the `VisualStudio.Extensibility` model, it provides a performant and isolated experience for interacting with various AI models.

## Features

- **Multi-Provider Support**: Seamlessly integrate with OpenAI, Ollama, and Google Gemini.
- **Remote UI**: A sleek, responsive chat interface built with modern WPF (using Remote UI).
- **Intelligent Context**: Leverages active document context and memory snippets for more relevant AI interactions.
- **Model Orchestration**: Base models can intelligently delegate tasks to specialized agent models.
- **Conversation Management**: Easily create, delete, and switch between multiple chat sessions.
- **Local Memory**: Persistent storage for code snippets and project-specific knowledge.

## Prerequisites

- **Visual Studio 2022 (v17.10 or later)**
- **.NET 8.0 SDK** (for UI and Core components)
- **.NET Framework 4.8.1** (for the Extension Package)

## Getting Started

1.  **Clone the repository**: `git clone https://github.com/your-username/BringYourOwnAI.git`
2.  **Open the solution**: Load `BringYourOwnAI.slnx` (or the `.sln`) in Visual Studio.
3.  **Restore and Build**: Run `dotnet restore` and build the solution.
4.  **Run/Debug**: Set the `BringYourOwnAI.Package` project as the startup project and press F5.

## Configuration

Navigate to the **Settings** view in the extension's chat window to configure your API keys and endpoints:
- OpenAI API Key
- Ollama Endpoint (defaults to `http://localhost:11434`)
- Google Gemini API Key

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
