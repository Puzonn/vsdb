public class ProjectPinDb
{
    public Guid Id { get; set; }
    public Guid NodeId { get; set; }

    public string Label { get; set; } = null!;
    public PinDirection Direction { get; set; }

    public ProjectNodeDb Node { get; set; } = null!;
}
