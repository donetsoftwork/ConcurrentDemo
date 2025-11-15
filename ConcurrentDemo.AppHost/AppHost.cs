var builder = DistributedApplication.CreateBuilder(args);

var dbName = "myWeather";
var mysql = builder.AddMySql("mysql")
    .WithEnvironment("MYSQL_DATABASE", dbName)
    .WithLifetime(ContainerLifetime.Persistent);

var initScriptPath = Path.Join(Path.GetDirectoryName(typeof(Program).Assembly.Location), "./Data/myWeather.sql");
var db = mysql.AddDatabase(dbName)
    .WithCreationScript(File.ReadAllText(initScriptPath));

//builder.AddProject<Projects.WeatherApi>("weatherapi")
//    .WithReference(db)
//    .WaitFor(db)
//    .WithReplicas(10);

builder.AddProject<Projects.ConcurrentApi>("concurrentapi")
    .WithReference(db)
    .WaitFor(db)
    .WithReplicas(2);

builder.Build()
    .Run();
