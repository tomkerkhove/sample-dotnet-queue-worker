namespace Keda.Samples.Dotnet.Contracts;

public record Customer(string FirstName, string LastName);
public record Order(string Id, int Amount, string ArticleNumber, Customer Customer);
public record QueueStatus(long MessageCount);