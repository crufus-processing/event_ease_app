using System.ComponentModel.DataAnnotations;

public class UserModel {
    //Property getters/setters with validation
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(20, ErrorMessage = "Password must be between 6 and 20 characters long.", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    public List<EventModel> EventsHosting { get; set; } = new();
    public List<EventModel> EventsAttending { get; set; } = new();
}