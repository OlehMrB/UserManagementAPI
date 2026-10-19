using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

public class User : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters.")]
    public string Department { get; set; } = string.Empty;

    // [Required] alone allows whitespace-only strings; reject those too.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FirstName.Length > 0 && string.IsNullOrWhiteSpace(FirstName))
        {
            yield return new ValidationResult("First name cannot be blank or whitespace.", [nameof(FirstName)]);
        }

        if (LastName.Length > 0 && string.IsNullOrWhiteSpace(LastName))
        {
            yield return new ValidationResult("Last name cannot be blank or whitespace.", [nameof(LastName)]);
        }

        if (Department.Length > 0 && string.IsNullOrWhiteSpace(Department))
        {
            yield return new ValidationResult("Department cannot be blank or whitespace.", [nameof(Department)]);
        }
    }
}
