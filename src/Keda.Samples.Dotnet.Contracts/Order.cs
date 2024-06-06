namespace Keda.Samples.Dotnet.Contracts;


public record Order(string Id, int Amount, string ArticleNumber, Customer Customer);
