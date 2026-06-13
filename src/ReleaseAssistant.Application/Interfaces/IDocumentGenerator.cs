using ReleaseAssistant.Application.Models.Mcp;

namespace ReleaseAssistant.Application.Interfaces;

public interface IDocumentGenerator
{
    string Generate(ReleasePackage package);
}
