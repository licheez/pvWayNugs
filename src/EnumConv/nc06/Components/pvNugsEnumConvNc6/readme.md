# pvNugsEnumConvNc6

A .NET utility library that provides enhanced enum conversion capabilities, focusing on Description attribute-based conversions and flexible matching strategies.

## Features

- Extract codes from enum Description attributes
- Convert string codes back to enum values
- Support for multiple codes per enum value (comma-separated in Description)
- Case-insensitive matching by default
- Custom matching strategy support
- Default value fallback support

## Installation

Install the package via NuGet:
```
shell
dotnet add package pvNugsEnumConvNc9
```
## Usage

### Basic Usage
```
csharp
// Define an enum with Description attributes
public enum Status
{
[Description("A")]
Active,

    [Description("I")]
    Inactive,
    
    [Description("P,PEND")]  // Multiple codes
    Pending
}

// Get code from enum value
string code = Status.Active.GetCode();  // Returns "A"

// Convert code back to enum
Status status = EnumConvert.GetValue<Status>("A", Status.Inactive);  // Returns Status.Active
```
### Multiple Codes Support

The library supports multiple codes in the Description attribute, separated by commas. The first code is considered the primary code:
```
csharp
Status pending = EnumConvert.GetValue<Status>("PEND", Status.Active);  // Returns Status.Pending
```
### Custom Matching

You can provide your own matching strategy:
```
csharp
bool CustomMatcher(string x, string y) => x.StartsWith(y);
Status status = EnumConvert.GetValue("PEN", Status.Active, CustomMatcher);  // Returns Status.Pending
```
## Error Handling

- `ArgumentNullException`: Thrown when the provided code is null or empty
- `ArgumentOutOfRangeException`: Thrown when:
  - No matching enum value is found for the provided code
  - An enum value lacks a Description attribute

## Requirements

- .NET 6.0 or higher
- C# 13.0 or higher

## License

[License details should be added here]

## Contributing

[Contributing guidelines should be added here]