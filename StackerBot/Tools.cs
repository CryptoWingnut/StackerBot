namespace StackerBot;

public static class Tools {
  public static string DatabaseConnectString() {
    var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
    var name = Environment.GetEnvironmentVariable("DATABASE_NAME");
    var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
    var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");
    var port = Environment.GetEnvironmentVariable("DATABASE_PORT");

    return $"Host={host};Database={name};Username={username};Password={password};Port={port}";
  }
}
