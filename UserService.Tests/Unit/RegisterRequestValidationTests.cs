using System.ComponentModel.DataAnnotations;
using UserService.DTOs;

namespace UserService.Tests.Unit;

public class RegisterRequestValidationTests
{
    [Fact]
    public void RegisterRequest_ShortPassword_IsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "short",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            errors,
            error => error.MemberNames.Contains(
                nameof(RegisterRequestDto.Password)
            )
        );
    }
    
    [Fact]
    public void RegisterRequest_NoNumberPassword_IsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "Password!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            errors,
            error => error.MemberNames.Contains(
                nameof(RegisterRequestDto.Password)
            )
        );
    }
    
    [Fact]
    public void RegisterRequest_NoUppercaseLetterPassword_IsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "shortpass1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            errors,
            error => error.MemberNames.Contains(
                nameof(RegisterRequestDto.Password)
            )
        );
    }
    
    [Fact]
    public void RegisterRequest_NoSpecialCharacterPassword_IsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "shortpass1",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            errors,
            error => error.MemberNames.Contains(
                nameof(RegisterRequestDto.Password)
            )
        );
    }
    
    [Fact]
    public void RegisterRequest_InvalidEmail_IsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "validexample.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            errors,
            error => error.MemberNames.Contains(
                nameof(RegisterRequestDto.Email)
            )
        );
    }
    
    [Fact]
    public void RegisterRequest_InvalidPhoneNumber_IsInvalid()
    {
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "PhoneNumber12"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(
            errors,
            error => error.MemberNames.Contains(
                nameof(RegisterRequestDto.PhoneNumber)
            )
        );
    }
    
    [Fact]
    public void RegisterRequest_AllFieldsMissing_IsInvalid()
    {
        var request = new RegisterRequestDto();

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        var expectedFields = new[]
        {
            nameof(RegisterRequestDto.Email),
            nameof(RegisterRequestDto.Password),
            nameof(RegisterRequestDto.FirstName),
            nameof(RegisterRequestDto.LastName),
            nameof(RegisterRequestDto.PhoneNumber)
        };
        
        Assert.False(isValid);
        
        var fieldsWithErrors = errors
            .SelectMany(error => error.MemberNames)
            .ToHashSet();
        
        Assert.All(expectedFields, field => Assert.Contains(field, fieldsWithErrors));
    }
    
    [Fact]
    public void RegisterRequest_IsValid()
    {
        var request = new RegisterRequestDto
        {
            Email = "valid@example.com",
            Password = "Password1!",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "555-123-4567"
        };

        var errors = new List<ValidationResult>();
        var context = new ValidationContext(request);

        var isValid = Validator.TryValidateObject(
            request,
            context,
            errors,
            validateAllProperties: true);

        Assert.True(isValid);
    }
}