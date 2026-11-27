namespace CARELINK.Services;

public static class MedicalServiceCollectionExtensions
{
    /// <summary>Registers the Medical System services (Member 4).</summary>
    public static IServiceCollection AddMedicalServices(this IServiceCollection services)
    {
        services.AddScoped<MedicalHistoryService>();
        return services;
    }
}