namespace HotelReservation.Application.DTOs.UserManagement;

public class DependentSlimDto
{
    public string FullName { get; set; } = string.Empty;
    public string NationalCode { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
}

public class UserWithDependentsDto
{
    public Guid Id { get; set; }
    public string SystemUserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalCode { get; set; } = string.Empty;
    public List<DependentSlimDto> Dependents { get; set; } = new();

    // برای نمایش بهتر در کامپوننت Autocomplete
    public override string ToString()
    {
        return $"{FullName} ({SystemUserId})";
    }
}