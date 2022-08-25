﻿using Backend.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// ConfigureServices
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes("010242dabe0ae1e9c8ab6c0f219ae887c0ca0a1d16847b889330f9b4fb261c9c")
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    }
);

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AnyOrigin", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseSwagger(
    options =>
    {
        options.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            swagger.Servers = new List<Microsoft.OpenApi.Models.OpenApiServer>
            {
                // You can add as many OpenApiServer instances as you want by creating them like below
                new Microsoft.OpenApi.Models.OpenApiServer
                {
                    // You can set the Url from the default http request data or by hard coding it
                    // Url = $"{httpReq.Scheme}://{httpReq.Host.Value}",
                    Url = Program.config["SwaggerServerUrl"],
                    Description = "v1"
                }
            };
        });
    }
);

app.UseSwaggerUI();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AnyOrigin");

app.UseAuthentication();

app.UseAuthorization();

Authentication.Router(app);
Category.Router(app);
Main.Router(app);
Recipe.Router(app);

app.Run();

public partial class Program
{
    public static readonly IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
}

public static class Main
{
    private static IResult Index() => Results.Json(new { message = "Home Page..." });
    private static IResult About() => Results.Json(new { message = "About page ^^" });

    public static void Router(IEndpointRouteBuilder router)
    {
        router.MapGet("/", Index);
        router.MapGet("/about", About);
    }
}

public static class Authentication
{
    private static readonly Backend.Services.JsonService _service = new(
        Program.config["UsersPath"],
        Program.config["RecipesPath"],
        Program.config["CategoriesPath"]
    );

    public static void Router(IEndpointRouteBuilder router)
    {
        router.MapPost("/register", RegisterUser);
        router.MapPost("/login", UserLogin);
        router.MapGet("/users", ListUsers);
        router.MapGet("/users/{id:guid}", GetUser);
        //router.MapPut("/users/{id:guid}", UpdateUser);
        router.MapDelete("/users/{id:guid}", DeleteUser);
    }

    [Authorize]
    private static IResult ListUsers()
    {
        return Results.Json(_service.ListUsers(), statusCode: 200);
    }

    private static IResult GetUser(Guid id)
    {
        return Results.Json(_service.GetUser(id), statusCode: 200);
    }

    private static IResult RegisterUser([FromBody] Backend.Models.UserDTO user)
    {
        bool valid = (
            !string.IsNullOrEmpty(user.Username) &&
            !string.IsNullOrEmpty(user.Password) &&
            !_service.ListUsers().Any(u => u.Username == user.Username)
        );
        if (valid)
        {
            return Results.Json(
                _service.Register(user),
                statusCode: 200
            );
        }
        else
        {
            List<string> msgs = new();
            msgs.Add(
                $"Username and Password can't be Empty, or this Username is taken!"
            );
            return Results.Json(
                msgs,
                statusCode: 400
            );
        }
    }

    //private static IResult UpdateUser([FromBody] Backend.Models.User user)
    //{
    //    return Results.Json(_service.UpdateUser(user.Id, user.Username, ), statusCode: 200);
    //}

    private static IResult DeleteUser(Guid id)
    {
        _service.DeleteUser(id);
        return Results.Json("Deleted Successfully",statusCode: 200);
    }

    private static IResult UserLogin([FromBody] Backend.Models.UserDTO user)
    {
        bool valid = _service.ListUsers().Any(
            u => u.Username==user.Username &&
            u.VerifyPassword(user.Password)
        );
        if (valid)
        {
            return Results.Json(
                _service.GetTocken(user),
                statusCode: 200
            );
        }
        else
        {
            List<string> msgs = new();
            msgs.Add(
                $"Invalid username or password!"
            );
            return Results.Json(
                msgs,
                statusCode: 400
            );
        }
    }
}


public static class Recipe
{
    private static readonly Backend.Services.JsonService _service = new(
        Program.config["UsersPath"],
        Program.config["RecipesPath"],
        Program.config["CategoriesPath"]
    );

    public static void Router(IEndpointRouteBuilder router)
    {
        router.MapGet("/recipes", ListRecipes);
        router.MapPost("/recipes", CreateRecipe);
        router.MapGet("/recipes/{id:guid}", GetRecipe);
        router.MapPut("/recipes/{id:guid}", UpdateRecipe);
        router.MapDelete("/recipes/{id:guid}", DeleteRecipe);
    }

    private static IResult ListRecipes()
    {
        return Results.Json(_service.ListRecipes(), statusCode: 200);
    }

    private static IResult CreateRecipe([FromBody] Backend.Models.Recipe recipe)
    {
        Backend.Models.RecipeValidator validator = new();
        ValidationResult results = validator.Validate(
            new Backend.Models.Recipe(
                recipe.Name,
                recipe.Ingredients,
                recipe.Instructions,
                recipe.CategoriesIds
            )
        );
        if (results.IsValid)
        {
            return Results.Json(
                _service.CreateRecipe(
                    recipe.Name,
                    recipe.Ingredients,
                    recipe.Instructions,
                    recipe.CategoriesIds
                ),
                statusCode: 200
            );
        }
        else
        {
            List<string> msgs = new();
            foreach (var failure in results.Errors)
                msgs.Add(
                    $"Property {failure.PropertyName}: {failure.ErrorMessage}"
                );
            return Results.Json(
                msgs,
                statusCode: 400
            );
        }
    }

    private static IResult GetRecipe(Guid id)
    {
        return Results.Json(_service.GetRecipe(id), statusCode: 200);
    }

    private static IResult UpdateRecipe(Guid id, [FromBody] Backend.Models.Recipe recipe)
    {
        Backend.Models.RecipeValidator validator = new();
        ValidationResult results = validator.Validate(
            new Backend.Models.Recipe(
                recipe.Name,
                recipe.Ingredients,
                recipe.Instructions,
                recipe.CategoriesIds
            )
        );
        if (results.IsValid)
        {
            return Results.Json(
                _service.UpdateRecipe(
                    id,
                    recipe.Name,
                    recipe.Ingredients,
                    recipe.Instructions,
                    recipe.CategoriesIds
                ),
                statusCode: 200
            );
        }
        else
        {
            List<string> msgs = new();
            foreach (var failure in results.Errors)
                msgs.Add(
                    $"Property {failure.PropertyName}: {failure.ErrorMessage}"
                );
            return Results.Json(
                msgs,
                statusCode: 400
            );
        }
    }

    private static IResult DeleteRecipe(Guid id)
    {
        _service.DeleteRecipe(id);
        return Results.Json(new { message = "Deleted Successfully" }, statusCode: 200);
    }
}

public static class Category
{
    private static readonly Backend.Services.JsonService _service = new(
        Program.config["UsersPath"],
        Program.config["RecipesPath"],
        Program.config["CategoriesPath"]
    );

    public static void Router(IEndpointRouteBuilder router)
    {
        router.MapGet("/categories", ListCategories);
        router.MapPost("/categories", CreateCategory);
        router.MapGet("/categories/{id:guid}", GetCategory);
        router.MapPut("/categories/{id:guid}", UpdateCategory);
        router.MapDelete("/categories/{id:guid}", DeleteCategory);
    }

    private static IResult ListCategories()
    {
        return Results.Json(_service.ListCategories(), statusCode: 200);
    }

    private static IResult CreateCategory([FromBody] string name)
    {
        Backend.Models.CategoryValidator validator = new();
        ValidationResult results = validator.Validate(
            new Backend.Models.Category(name)
        );
        if (results.IsValid)
        {
            return Results.Json(
                _service.CreateCategory(name),
                statusCode: 200
            );
        }
        else
        {
            List<string> msgs = new();
            foreach (var failure in results.Errors)
                msgs.Add(
                    $"Property {failure.PropertyName}: {failure.ErrorMessage}"
                );
            return Results.Json(
                msgs,
                statusCode: 400
            );
        }
    }

    private static IResult GetCategory(Guid id)
    {
        return Results.Json(_service.GetCategory(id), statusCode: 200);
    }

    private static IResult UpdateCategory(Guid id, [FromBody] Backend.Models.Category category)
    {
        Backend.Models.CategoryValidator validator = new();
        ValidationResult results = validator.Validate(
            new Backend.Models.Category(category.Name)
        );
        if (results.IsValid)
        {
            return Results.Json(
                _service.UpdateCategory(
                    id,
                    category.Name
                ),
                statusCode: 200
            );
        }
        else
        {
            List<string> msgs = new();
            foreach (var failure in results.Errors)
                msgs.Add(
                    $"Property {failure.PropertyName}: {failure.ErrorMessage}"
                );
            return Results.Json(
                msgs,
                statusCode: 400
            );
        }
    }

    private static IResult DeleteCategory(Guid id)
    {
        _service.DeleteCategory(id);
        return Results.Json(
            new { message = "Deleted Successfully!" },
            statusCode: 200
        );
    }
}
