using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();

public partial class Program
{
    public static readonly IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();
}

public static class Requests
{
    private readonly static string _baseAddress = Program.config["BaseAddress"];

    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        )!;
    }

    async public static Task<List<Frontend.Models.Recipe>> ListRecipes()
    {
        var uri = new Uri($"{_baseAddress}/recipes");
        using var client = new HttpClient();
        var json = await client.GetAsync(uri);
        var result = await json.Content.ReadAsStringAsync();
        return Deserialize<List<Frontend.Models.Recipe>>(result);
    }

    async public static Task<List<Frontend.Models.Category>> ListCategories()
    {
        var uri = new Uri($"{_baseAddress}/categories");
        using var client = new HttpClient();
        var json = await client.GetAsync(uri);
        var result = await json.Content.ReadAsStringAsync();
        return Deserialize<List<Frontend.Models.Category>>(result);
    }

    async public static Task<Frontend.Models.Recipe> GetRecipe(Guid id)
    {
        var uri = new Uri($"{_baseAddress}/recipes/{id}");
        using var client = new HttpClient();
        var json = await client.GetAsync(uri);
        var result = await json.Content.ReadAsStringAsync();
        return Deserialize<Frontend.Models.Recipe>(result);
    }

    async public static Task<Frontend.Models.Category> GetCategory(Guid id)
    {
        var uri = new Uri($"{_baseAddress}/categories/{id}");
        using var client = new HttpClient();
        var json = await client.GetAsync(uri);
        var result = await json.Content.ReadAsStringAsync();
        return Deserialize<Frontend.Models.Category>(result);
    }

    async public static Task<Frontend.Models.Recipe> CreateRecipe(string name, List<string> ingredients, List<string> instructions, List<Guid> categoriesIds)
    {
        var uri = new Uri($"{_baseAddress}/recipes");
        var recipe = new Frontend.Models.Recipe(name, ingredients, instructions, categoriesIds);
        var json = JsonSerializer.Serialize(recipe);
        var payload = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var result = await client.PostAsync(uri, payload);
        var responce = await result.Content.ReadAsStringAsync();
        return Deserialize<Frontend.Models.Recipe>(json);
    }

    async public static Task<Frontend.Models.Category> CreateCategory(string name)
    {
        var uri = new Uri($"{_baseAddress}/categories");
        var category = new Frontend.Models.Category(name);
        var json = JsonSerializer.Serialize(category);
        var payload = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var result = await client.PostAsync(uri, payload);
        var responce = await result.Content.ReadAsStringAsync();
        return Deserialize<Frontend.Models.Category>(json);
    }

    async public static Task<Frontend.Models.Recipe> UpdateRecipe(Guid id, string name, List<string> ingredients, List<string> instructions, List<Guid> categoriesIds)
    {
        var uri = new Uri($"{_baseAddress}/recipes/{id}");
        var recipe = new Frontend.Models.Recipe(name, ingredients, instructions, categoriesIds);
        var json = JsonSerializer.Serialize(recipe);
        var payload = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var result = await client.PutAsync(uri, payload);
        var responce = await result.Content.ReadAsStringAsync();
        return Deserialize<Frontend.Models.Recipe>(json);
    }

    async public static Task<Frontend.Models.Category> UpdateCategory(Guid id, string name)
    {
        var uri = new Uri($"{_baseAddress}/categories/{id}");
        var category = new Frontend.Models.Category(name);
        var json = JsonSerializer.Serialize(category);
        var payload = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var client = new HttpClient();
        var result = await client.PutAsync(uri, payload);
        var responce = await result.Content.ReadAsStringAsync();
        return Deserialize<Frontend.Models.Category>(json);
    }

    async public static Task DeleteRecipe(Guid id)
    {
        var uri = new Uri($"{_baseAddress}/recipes/{id}");
        using var client = new HttpClient();
        var result = await client.DeleteAsync(uri);
        var responce = await result.Content.ReadAsStringAsync();
    }

    async public static Task DeleteCategory(Guid id)
    {
        var uri = new Uri($"{_baseAddress}/categories/{id}");
        using var client = new HttpClient();
        var result = await client.DeleteAsync(uri);
        var responce = await result.Content.ReadAsStringAsync();
    }
}