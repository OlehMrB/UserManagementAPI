using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(ILogger<UsersController> logger) : ControllerBase
{
    // In-memory store; replace with a database in a real application.
    // ConcurrentDictionary gives O(1) thread-safe lookups by ID instead of scanning a List.
    private static readonly ConcurrentDictionary<int, User> Users = new();
    private static int _nextId;

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        return Ok(Users.Values.OrderBy(u => u.Id).ToList());
    }

    [HttpGet("{id:int}")]
    public ActionResult<User> GetUser(int id)
    {
        if (!Users.TryGetValue(id, out var user))
        {
            logger.LogWarning("User with ID {UserId} was not found.", id);
            return NotFound($"User with ID {id} was not found.");
        }

        return Ok(user);
    }

    [HttpPost]
    public ActionResult<User> CreateUser([FromBody] User? user)
    {
        if (user is null)
        {
            return BadRequest("A request body with user data is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        NormalizeUser(user);

        if (Users.Values.Any(u => string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(Models.User.Email), "A user with this email already exists.");
            return ValidationProblem(ModelState);
        }

        user.Id = Interlocked.Increment(ref _nextId);
        Users[user.Id] = user;

        logger.LogInformation("Created user {UserId}.", user.Id);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, [FromBody] User? updatedUser)
    {
        if (updatedUser is null)
        {
            return BadRequest("A request body with user data is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!Users.TryGetValue(id, out var user))
        {
            logger.LogWarning("Attempted to update user with ID {UserId}, but they were not found.", id);
            return NotFound($"User with ID {id} was not found.");
        }

        NormalizeUser(updatedUser);

        if (Users.Values.Any(u => u.Id != id && string.Equals(u.Email, updatedUser.Email, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(Models.User.Email), "A user with this email already exists.");
            return ValidationProblem(ModelState);
        }

        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Email = updatedUser.Email;
        user.Department = updatedUser.Department;

        logger.LogInformation("Updated user {UserId}.", id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        if (!Users.TryRemove(id, out _))
        {
            logger.LogWarning("Attempted to delete user with ID {UserId}, but they were not found.", id);
            return NotFound($"User with ID {id} was not found.");
        }

        logger.LogInformation("Deleted user {UserId}.", id);
        return NoContent();
    }

    private static void NormalizeUser(User user)
    {
        user.FirstName = user.FirstName.Trim();
        user.LastName = user.LastName.Trim();
        user.Email = user.Email.Trim();
        user.Department = user.Department.Trim();
    }
}

