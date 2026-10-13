namespace Zamanla.WebAPI.IntegrationTests.Infrastructure;

// OData listeleri { "@odata.context": ..., "value": [...] } şeklinde döner
public sealed record ODataResponse<T>(List<T> Value);
