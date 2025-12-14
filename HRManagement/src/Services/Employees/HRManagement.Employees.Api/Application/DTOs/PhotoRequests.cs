namespace HRManagement.Employees.Api.Application.DTOs;

public record UploadPhotoRequest(
    IFormFile Photo);

public record PhotoDto(
    Guid EntityId,
    string PhotoUrl);
