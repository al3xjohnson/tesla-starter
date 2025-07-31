namespace TeslaStarter.Infrastructure.Configuration;

public class DescopeOptions
{
    public const string SectionName = "Descope";

    public string ProjectId { get; set; } = string.Empty;
    public string ManagementKey { get; set; } = string.Empty;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ProjectId))
            throw new InvalidOperationException($"{nameof(ProjectId)} is required in {SectionName} configuration");

        if (string.IsNullOrWhiteSpace(ManagementKey))
            throw new InvalidOperationException($"{nameof(ManagementKey)} is required in {SectionName} configuration");
    }
}
