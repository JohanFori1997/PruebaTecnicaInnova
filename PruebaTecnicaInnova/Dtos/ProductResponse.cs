
namespace PruebaTecnicaInnova.Dtos;

public sealed record ProductResponse(
    int Id,
    string? Name,
    string? Description,
    decimal Price
);
