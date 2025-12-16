# Publishing to NuGet

This guide explains how to publish the UuidByString package to NuGet.org.

## Prerequisites

1. A NuGet.org account
2. A NuGet API key (get it from https://www.nuget.org/account/apikeys)

## Setup

1. Copy the environment template file:
   ```powershell
   Copy-Item .env.example .env
   ```

2. Edit the `.env` file and replace `your-api-key-here` with your actual NuGet API key:
   ```
   NUGET_API_KEY=your-actual-api-key
   ```

3. **Important**: The `.env` file is already in `.gitignore` to prevent accidentally committing your API key.

## Publishing

Run the publish script:

```powershell
.\publish.ps1
```

The script will:
1. Clean previous builds
2. Build the project in Release configuration
3. Pack the NuGet package
4. Ask for confirmation
5. Publish to NuGet.org

### Custom Configuration

You can specify a different configuration:

```powershell
.\publish.ps1 -Configuration Release
```

Or use a different environment file:

```powershell
.\publish.ps1 -EnvFile .env.prod
```

## After Publishing

- It may take a few minutes for the package to appear on NuGet.org
- Check the package status at: https://www.nuget.org/packages/UuidByString
- You can verify the package by installing it in a test project:
  ```bash
  dotnet add package UuidByString
  ```

## Troubleshooting

- If publishing fails, check that your API key has the "Push" permission
- Make sure you're not trying to publish a version that already exists
- For version conflicts, update the `<PackageVersion>` in `UuidByString.csproj`
