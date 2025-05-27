using System.ComponentModel.DataAnnotations;

public class EventModel {
    //Property getters/setters with validation
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(45, ErrorMessage = "Name cannot exceed 45 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(180, ErrorMessage = "Description cannot exceed 180 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date is required.")]
    public DateOnly? Date { get; set; }

    [Required(ErrorMessage = "Time is required.")]
    public TimeOnly? Time { get; set; } 

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(120, ErrorMessage = "Location cannot exceed 120 characters.")]
    public string Location { get; set; } = string.Empty;

    public string HostEmail { get; set; } = string.Empty;

    public List<UserModel> Attendees { get; set; } = new();
    public int AttendeeCount => Attendees.Count;
}