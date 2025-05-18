# KnowledgeBox Auth Service Tests

This project contains integration and unit tests for the KnowledgeBox Auth Service.

## Running the Tests

### Using Visual Studio
1. Open the solution in Visual Studio
2. Right-click on the test project in Solution Explorer and select "Run Tests"

### Using the Command Line
1. Navigate to the test project directory
```bash
cd tests/KnowledgeBox.Auth.Tests
```

2. Run the tests
```bash
dotnet test
```

## Test Categories

### Integration Tests
These tests verify that the endpoints work correctly by making HTTP requests to the API.
- `AuthControllerTests.cs` - Tests for the Auth controller endpoints

### Unit Tests
These tests verify individual components in isolation.
- `UserSignupRequestTests.cs` - Tests for model validation

## Adding New Tests
- Place controller tests in the `Controllers` folder
- Place model tests in the `Models` folder
- Use test helpers located in the `Helpers` folder 