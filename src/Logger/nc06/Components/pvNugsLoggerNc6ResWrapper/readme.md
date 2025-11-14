# pvNugsLoggerNc6ResWrapper

A .NET 6.0 library providing robust result wrapper classes for HTTP operations and method executions. This package helps standardize the way method results and HTTP responses are handled, with built-in support for notifications, severity levels, and mutation tracking.

## Features

- **HTTP Result Wrapping**: Standardized wrapper for HTTP responses with `DsoHttpResult` and `DsoHttpResult<T>`
- **Method Result Handling**: Comprehensive result wrapper with `MethodResult` and `MethodResult<T>`
- **Severity-based Status**: Automatic HTTP status code mapping based on severity levels
- **Mutation Tracking**: Built-in support for tracking Create, Update, Delete operations
- **Rich Notification System**: Support for multiple notifications with severity levels
- **Exception Integration**: Seamless conversion of exceptions to result objects
- **Pagination Support**: Built-in handling for paginated results
- **Strongly Typed**: Generic support for typed result data

## Installation
```shell
dotnet add package pvNugsLoggerNc6ResWrapper
```
## Usage

### Basic HTTP Result
```
csharp
// Create a successful result
var result = new DsoHttpResult();

// Create a result with specific severity
var errorResult = new DsoHttpResult(SeverityEnu.Error);

// Add notifications
result.AddNotification("Operation completed", SeverityEnu.Info);
```
### Typed HTTP Result
```
csharp
// Return data with success status
var result = new DsoHttpResult<User>(user);

// Return data with pagination
var pagedResult = new DsoHttpResult<List<User>>(users, hasMoreResults: true);

// Return data with mutation tracking
var createResult = new DsoHttpResult<User>(newUser, DsoHttpResultMutationEnu.Create);
```
### Method Result
```
csharp
// Create a successful result
var result = MethodResult.Ok;

// Create a result with error
var errorResult = new MethodResult("Operation failed", SeverityEnu.Error);

// Handle exceptions
try
{
// Some operation
}
catch (Exception ex)
{
return new MethodResult(ex);
}
```
### Typed Method Result
```
csharp
// Return data
var result = new MethodResult<User>(user);

// Return null data
var nullResult = MethodResult<User>.Null;

// Multiple notifications
var result = new MethodResult<User>(
new[] { "Warning 1", "Warning 2" },
SeverityEnu.Warning
);
```
## Dependencies

- .NET 6.0
- pvNugsLoggerNc6Abstractions (>= 6.0.0)

## License

This project is licensed under the terms of the license provided by Pierre Van Wallendael.

## Author

Pierre Van Wallendael

## Repository

[GitHub Repository](https://github.com/licheez/pvWayNugs)

## Version History

### 6.0.0
- Initial release
- Full .NET 6.0 support
- Complete result wrapper implementation
