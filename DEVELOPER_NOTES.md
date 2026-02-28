# BringYourOwnAI — Developer Notes

This file documents the non-obvious setup steps, gotchas, and scripts you'll need when working on the VSIX packaging layer.

---

## Project Structure

| Project | Target | Purpose |
|---|---|---|
| `BringYourOwnAI.Core` | `netstandard2.0` | Models, interfaces, services |
| `BringYourOwnAI.Providers` | `netstandard2.0` | OpenAI / Gemini / Ollama providers |
| `BringYourOwnAI.UI` | `net8.0-windows` | WPF views, ViewModels (Remote UI) |
| `BringYourOwnAI.Package` | `net8.0-windows` | VSIX entry point, tool window, menu command |
| `BringYourOwnAI.Tests` | `net10.0` | Unit tests |
| `BringYourOwnAI.VsIntegration` | `net48` | Legacy DTE/solution service (if still used) |

## 1 — Modern Extensibility SDK (`Microsoft.VisualStudio.Extensibility.Sdk`)

Visual Studio 2026 uses the modern, out-of-process `VisualStudio.Extensibility` architecture. **Do not use the legacy VSSDK models.**

### ⚠️ Critical Architecture Rules

1. **NO `source.extension.vsixmanifest`**: The modern Extensibility SDK generates the VSIX payload entirely via Roslyn code analysis. If you include a manual `source.extension.vsixmanifest` file, the build will fail with `VSSDK1309 Cannot find source manifest`.
2. **NO `.vsct` or `Menus.ctmenu`**: Command tables and UI layouts are dead concepts. All commands, tool windows, and menus must be defined in pure C# using the `[VisualStudioContribution]` attribute. 
3. **NO `Microsoft.VSSDK.BuildTools` in `.csproj`**: The project must reference **only** `<PackageReference Include="Microsoft.VisualStudio.Extensibility.Sdk" />`. The SDK meta-package will automatically import the analyzers and compilers needed to generate the `.vsextension` deployment payload.
4. **Out-of-Process Remote UI**: All XAML files must be `Embedded Resources` and must not have C# Code-Behind files. XAML elements bind to ViewModels via strict `[DataContract]` serialized MVVM messages across the process barrier.

---

## 2 — Diagnostics: Why is my Command missing?

If you hit `F5` and your extension menu doesn't appear, the Roslyn Source Generator likely failed to evaluate your metadata.

### Step 1 — Check for Source Generator Output
During a successful build, the SDK produces a `extension.json` (or `.vsext`) payload instead of compiling a `.dll` straight to the Experimental instance.

Look at `src\BringYourOwnAI.Package\bin\Debug\net8.0-windows\.vsextension\extension.json`.
Does it contain your `controlPlacements`?
If `extension.json` is missing or empty, your `[VisualStudioContribution]` attributes were ignored by the Analyzer.

### Step 2 — Fix SDK Wrapping
Ensure your project relies strictly on the umbrella SDK packet, not individual NuGet assemblies:
```xml
<!-- ✅ CORRECT -->
<PackageReference Include="Microsoft.VisualStudio.Extensibility.Sdk" Version="17.11.350" />

<!-- ❌ WRONG (The analyzers will lie dormant) -->
<PackageReference Include="Microsoft.VisualStudio.Extensibility" Version="17.11.350" />
<PackageReference Include="Microsoft.VisualStudio.Extensibility.Build" Version="17.11.350" />
```

### Step 3 — Ensure Public Visibility
In the out-of-process model, the Visual Studio host must be able to discover and instantiate your classes via the RPC proxy.
- **Rule**: `BYOAIExtension`, `ShowChatWindowCommand`, and `ChatToolWindow` **MUST** be `public`.
- **Rule**: Any service injected via DI (e.g., `IVsSolutionService`) should also be `public`.

### Step 4 — Verify the DI Chain (Silent Failures)
The most common cause of "Unregistered tool window" errors is a **broken Dependency Injection chain**.
If your `ToolWindow` constructor requires a service that is not registered in `InitializeServices`, the RPC host will fail to instantiate it silently, and Visual Studio will report it as "Unregistered".
- **Check**: Ensure `IAiProvider`, `HttpClient`, and all core services are registered in `BYOAIExtension.InitializeServices`.

### Step 5 — Clean & Rebuild
Always run a full CLI clean when changing `ExtensionConfiguration`, DI registrations, or Menu Placements:
```powershell
dotnet clean
dotnet build
```

---

## 3 — Build & Install Workflow (17.14 SDK)

For Visual Studio 2026/17.14+, use `dotnet build` to generate the VSIX.

### Mandatory Packaging Package
The `Microsoft.VisualStudio.Extensibility.Build` NuGet package is **mandatory**. Without it, the `GenerateVsix` property is ignored and no `.vsix` file is created.

### Creating the `.vsix`
```powershell
# In the Package project directory
dotnet build /p:Configuration=Debug
```
The output will be in `bin\Debug\net8.0-windows\BringYourOwnAI.Package.vsix`.

---

## 4 — Maintenance & Safety (Note to Self)

> [!IMPORTANT]
> **Reference Preservation**: Do not remove "unused" NuGet packages or Project References from the `Package` project without tracing the full out-of-process DI chain. Even if a reference isn't used directly in the `Package` code, it may be required for the RPC host to instantiate a dependency-injected service.
> 
> **Visibility Safety**: Never change extension/command/toolwindow classes back to `internal`. This breaks discovery in the out-of-process model.
