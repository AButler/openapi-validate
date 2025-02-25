# OpenApi Validate

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/AButler/openapi-validate/main.yaml)](https://github.com/AButler/openapi-validate/actions/workflows/main.yaml)
[![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/AButler/openapi-validate?include_prereleases)](https://github.com/AButler/openapi-validate/releases)
[![GitHub](https://img.shields.io/github/license/AButler/openapi-validate)](https://github.com/AButler/openapi-validate/blob/main/LICENSE)

## Introduction

This package is used to validate a HTTP request/response against an OpenAPI specification.

This can be integrated in to a test suite to ensure the responses returned by your API conform to the OpenAPI specification.

## Installation

```bash
dotnet add package OpenApiValidate
```

## Usage

See more detailed documentation for setup of the validator.

```csharp
// Load the OpenApiDocument
var documentLoadResult = await OpenApiDocument.LoadAsync("https://petstore.swagger.io/v2/swagger.json");

// Create the OpenApiValidator
var validator = new OpenApiValidator(documentLoadResult.Document);

// Validate the request and response
var response = await httpClient.GetAsync("https://petstore.swagger.io/v2/store/inventory");
await openApiValidator.Validate(response); // will throw an exception if fails validation
```