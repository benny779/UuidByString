# UuidByString

A .NET library for generating deterministic [RFC-4122](https://tools.ietf.org/html/rfc4122#section-4.3) Name-Based UUIDs from strings using MD5 (v3) or SHA1 (v5) hashing algorithms.

> **Note:** This project is a port of the [uuid-by-string](https://www.npmjs.com/package/uuid-by-string) npm package for .NET.

## Installation

Install via .NET CLI:

```bash
dotnet add package UuidByString
```

## Supported Frameworks

- .NET Framework 4.0
- .NET Framework 4.8
- .NET 8.0

## Usage

The package provides static methods to generate UUIDs. The method receives any string and returns a generated hash.

### Regular Using

```csharp
using UuidByString;

string uuid = UuidGenerator.GenerateUuid("Hello world!");
// d3486ae9-136e-5856-bc42-212385ea7970
```

### Static Using

```csharp
using static UuidByString.UuidGenerator;

string uuid = GenerateUuid("Hello world!");
// d3486ae9-136e-5856-bc42-212385ea7970
```

The string `"Hello world!"` will always return `d3486ae9-136e-5856-bc42-212385ea7970`.

### Specify UUID Version

You can specify the UUID version. Available versions are 3 and 5 according to [RFC-4122](https://tools.ietf.org/html/rfc4122#section-4.3). The version determines the hashing algorithm: version 3 uses MD5, and version 5 uses SHA-1. SHA-1 is used by default if version is not specified.

```csharp
// Version 3 (MD5)
string uuidV3 = UuidGenerator.GenerateUuid("Hello world!", 3);
// 86fb269d-190d-3c85-b6e0-468ceca42a20

// Version 5 (SHA-1) - explicit
string uuidV5 = UuidGenerator.GenerateUuid("Hello world!", 5);
// d3486ae9-136e-5856-bc42-212385ea7970
```

### Using Namespaces

```csharp
// With namespace
string uuid = UuidGenerator.GenerateUuid("Hello world!", "my-namespace");

// With namespace and version
string uuid = UuidGenerator.GenerateUuid("Hello world!", "my-namespace", 3);
```

## API

### Methods

- `GenerateUuid(string target)` - Generates UUID with default version 5 (SHA-1)
- `GenerateUuid(string target, int version)` - Generates UUID with specified version (3 or 5)
- `GenerateUuid(string target, string namespace)` - Generates UUID with namespace and default version 5
- `GenerateUuid(string target, string namespace, int version)` - Generates UUID with namespace and specified version

### Parameters

- `target` - The string to generate UUID from
- `namespace` (optional) - UUID namespace
- `version` (optional) - 3 for MD5, 5 for SHA-1 (default: 5)

## License

[MIT License](LICENSE) - Copyright (c) 2025 Benny Shtemer

## Repository

[https://github.com/benny779/UuidByString](https://github.com/benny779/UuidByString)
