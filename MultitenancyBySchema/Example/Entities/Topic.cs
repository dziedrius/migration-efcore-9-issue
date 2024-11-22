namespace MultitenancyBySchema.Example.Entities;

public class Topic
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
}