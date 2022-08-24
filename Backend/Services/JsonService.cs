using System.Security.Claims;
using System.Text.Json;

namespace Backend.Services;

public class JsonService
{
    private readonly string _fileNameCategories;
    private readonly string _fileNameRecipes;
    private readonly string _fileNameUsers;

    public JsonService(string usersPath, string recipesPath, string categoriesPath)
    {
        if (string.IsNullOrEmpty(usersPath) || string.IsNullOrEmpty(recipesPath) || string.IsNullOrEmpty(categoriesPath))
            throw new ArgumentException("Error in appsettings.json");
        // Users
        _fileNameUsers = usersPath;
        if (!File.Exists(_fileNameUsers))
            File.WriteAllText(_fileNameUsers, "[]");
        // Recipes
        _fileNameRecipes = recipesPath;
        if (!File.Exists(_fileNameRecipes))
            File.WriteAllText(_fileNameRecipes, "[]");
        // Categories
        _fileNameCategories = categoriesPath;
        if (!File.Exists(_fileNameCategories))
            File.WriteAllText(_fileNameCategories, "[]");
    }

    // Read Files
    public string ReadCategories()
    {
        return File.ReadAllText(_fileNameCategories);
    }

    public string ReadRecipes()
    {
        return File.ReadAllText(_fileNameRecipes);
    }

    public string ReadUsers()
    {
        return File.ReadAllText(_fileNameUsers);
    }

    // OverWrite files
    public void OverWriteCategories(List<Models.Category> categories)
    {
        var newString = JsonSerializer.Serialize(categories);
        File.WriteAllText(_fileNameCategories, newString);
    }

    public void OverWriteRecipes(List<Models.Recipe> recipes)
    {
        var newString = JsonSerializer.Serialize(recipes);
        File.WriteAllText(_fileNameRecipes, newString);
    }

    public void OverWriteUsers(List<Models.User> users)
    {
        var newString = JsonSerializer.Serialize(users);
        File.WriteAllText(_fileNameUsers, newString);
    }

    // Create_Helper: Add and OverWriteFile
    public Models.Category SaveCategories(Models.Category category)
    {
        var oldString = ReadCategories();
        var categories = JsonSerializer.Deserialize<List<Models.Category>>(oldString)!;
        categories.Add(category);
        OverWriteCategories(categories);
        return category;
    }

    public Models.Recipe SaveRecipes(Models.Recipe recipe)
    {
        var oldString = ReadRecipes();
        var recipes = JsonSerializer.Deserialize<List<Models.Recipe>>(oldString)!;
        recipes.Add(recipe);
        OverWriteRecipes(recipes);
        return recipe;
    }

    public Models.User SaveUser(Models.User user)
    {
        var oldString = ReadUsers();
        var users = JsonSerializer.Deserialize<List<Models.User>>(oldString)!;
        users.Add(user);
        OverWriteUsers(users);
        return user;
    }

    // List
    public List<Models.Category> ListCategories()
    {
        var jsonString = ReadCategories();
        var categories = JsonSerializer.Deserialize<List<Models.Category>>(jsonString)!;
        categories.Sort((a, b) => a.Name.CompareTo(b.Name));
        return categories;
    }

    public List<Models.Recipe> ListRecipes()
    {
        var jsonString = ReadRecipes();
        var recipes = JsonSerializer.Deserialize<List<Models.Recipe>>(jsonString)!;
        recipes.Sort((a, b) => a.Name.CompareTo(b.Name));
        return recipes;
    }

    public List<Models.User> ListUsers()
    {
        var jsonString = ReadUsers();
        var users = JsonSerializer.Deserialize<List<Models.User>>(jsonString)!;
        users.Sort((a, b) => a.Username.CompareTo(b.Username));
        return users;
    }

    // Get
    public Models.Category GetCategory(Guid id)
    {
        var oldString = ReadCategories();
        var categories = JsonSerializer.Deserialize<List<Models.Category>>(oldString)!;
        return categories.Where(c => c.Id == id).First();
    }

    public Models.Recipe GetRecipe(Guid id)
    {
        var oldString = ReadRecipes();
        var recipes = JsonSerializer.Deserialize<List<Models.Recipe>>(oldString)!;
        return recipes.Where(r => r.Id == id).First();
    }

    public Models.User GetUser(Guid id)
    {
        var oldString = ReadUsers();
        var users = JsonSerializer.Deserialize<List<Models.User>>(oldString)!;
        return users.Where(r => r.Id == id).First();
    }

    // Create
    public Models.Category CreateCategory(string name)
    {
        return SaveCategories(new Models.Category(name));
    }

    public Models.Recipe CreateRecipe(string name, List<string> ingredients, List<string> instructions, List<Guid> categoriesIds)
    {
        return SaveRecipes(new Models.Recipe(name, ingredients, instructions, categoriesIds));
    }

    public Backend.Models.User Register(Backend.Models.UserDTO user)
    {
        Models.User newUser = new Models.User();
        newUser.Register(user.Username, user.Password);
        return SaveUser(newUser);
    }

    // Update
    public Models.Category UpdateCategory(Guid id, string name)
    {
        var categories = ListCategories().FindAll(c => c.Id != id);
        var category = ListCategories().Where(c => c.Id == id).First();
        category.Name = name;
        categories.Add(category);
        OverWriteCategories(categories);
        return category;
    }

    public Models.Recipe UpdateRecipe(Guid id, string name, List<string> ingredients, List<string> instructions, List<Guid> categoriesIds)
    {
        var recipes = ListRecipes().FindAll(r => r.Id != id);
        var recipe = ListRecipes().Where(r => r.Id == id).First();
        recipe.Name = name;
        recipe.Ingredients = ingredients;
        recipe.Instructions = instructions;
        recipe.CategoriesIds = categoriesIds;
        recipes.Add(recipe);
        OverWriteRecipes(recipes);
        return recipe;
    }

    public Models.User UpdateUser(Guid id, string username, string password)
    {
        var users = ListUsers().FindAll(r => r.Id != id);
        var user = ListUsers().Where(r => r.Id == id).First();
        user.Username = username;
        user.UpdatePassword(password);
        users.Add(user);
        OverWriteUsers(users);
        return user;
    }

    // Delete
    public void DeleteCategory(Guid id)
    {
        // Cascade to Recipe
        var category = ListCategories().Where(c => c.Id == id).First();
        var jsonString = ReadRecipes();
        var recipes = JsonSerializer.Deserialize<List<Models.Recipe>>(jsonString)!;
        foreach (var recipe in recipes)
        {
            recipe.CategoriesIds.Remove(category.Id);
        }

        OverWriteRecipes(recipes);
        // Delete Category
        var categories = ListCategories().FindAll(c => c.Id != id);
        OverWriteCategories(categories);
    }

    public void DeleteRecipe(Guid id)
    {
        var recipes = ListRecipes().FindAll(r => r.Id != id);
        OverWriteRecipes(recipes);
    }

    public void DeleteUser(Guid id)
    {
        var users = ListUsers().FindAll(r => r.Id != id);
        OverWriteUsers(users);
    }

    // Other
    public string GetTocken(Backend.Models.UserDTO user)
    {
        List<Claim> claims = new List<Claim> {
            new Claim(ClaimTypes.Name, user.Username)
        };

        // SECRET_KEY will be stored safely later
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(
                "010242dabe0ae1e9c8ab6c0f219ae887c0ca0a1d16847b889330f9b4fb261c9c"
            )
        );

        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key,
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha512Signature
        );

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt
            .JwtSecurityTokenHandler().WriteToken(token);
    }
}