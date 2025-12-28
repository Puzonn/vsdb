public record ProjectFile(string Name, string SourceCode);
public record Project(string ProjectId, Node[] Nodes);
public record ProjectCreationResponse(string ProjectId, Node[] Nodes);
public record ProjectRunResult(bool Success, string? Errors, Node[]? Nodes);

public enum PinDirection
{
    Input,
    Output
}