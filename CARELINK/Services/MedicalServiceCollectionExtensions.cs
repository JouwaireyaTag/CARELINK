
namespace CARELINK.Services;

public static class MedicalServiceCollectionExtensions
{
    /// <summary>Registers the Medical System services (Member 4).</summary>
    public static IServiceCollection AddMedicalServices(this IServiceCollection services)
    {
        services.AddScoped<MedicalHistoryService>();
        services.AddScoped<PatientProfileService>();

        // Medical CRUD services
        services.AddScoped<MedicalConditionService>();
        services.AddScoped<AllergyService>();
        services.AddScoped<SurgeryService>();
        services.AddScoped<MedicationService>();
        services.AddScoped<MedicalVisitService>();

        return services;
    }
}
