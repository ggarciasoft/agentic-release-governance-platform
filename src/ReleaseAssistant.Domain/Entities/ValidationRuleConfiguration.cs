using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Domain.Entities;

public class ValidationRuleConfiguration : EntityBase
{
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string RuleCode { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public FindingSeverity Severity { get; set; } = FindingSeverity.Warning;
    public string ConfigurationJson { get; set; } = "{}";
}
