using System;
using FluentValidation;

namespace Backend.Models;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenCreated { get; set; } = new DateTime();
    public DateTime TokenExpires { get; set; } = new DateTime();

    public User()
    {
        Id = Guid.NewGuid();
    }
}

public class UserDTO
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public UserDTO(string username, string password)
    {
        Username = username;
        Password = password;
    }
}


public class UserValidator : AbstractValidator<UserDTO>
{
    public UserValidator()
    {
        RuleFor(user => user.Username).NotEmpty();
        RuleFor(user => user.Password).NotEmpty();
    }
}