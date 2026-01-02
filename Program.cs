using Microsoft.Data.SqlClient;

if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("OctopusQL - Multi-threaded SQL Query Runner");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  OctopusQL --server <server> --database <database> --threads <count> [options]");
    Console.WriteLine();
    Console.WriteLine("Required:");
    Console.WriteLine("  --server, -s       SQL Server address");
    Console.WriteLine("  --database, -d     Database name");
    Console.WriteLine("  --threads, -t      Number of threads");
    Console.WriteLine();
    Console.WriteLine("Authentication (choose one):");
    Console.WriteLine("  --integrated       Use Windows Authentication");
    Console.WriteLine("  --user, -u         SQL Server username");
    Console.WriteLine("  --password, -p     SQL Server password");
    Console.WriteLine();
    Console.WriteLine("Query (choose one):");
    Console.WriteLine("  --query, -q        SQL query to execute");
    Console.WriteLine("  --file, -f         Path to SQL file");
    Console.WriteLine();
    Console.WriteLine("Optional:");
    Console.WriteLine("  --delay            Start delay per thread in ms (default: 0)");
    Console.WriteLine("  --timeout          Command timeout in seconds (default: 30)");
    Console.WriteLine();
    Console.WriteLine("Example:");
    Console.WriteLine("  OctopusQL -s localhost -d TestDB -t 5 -q \"SELECT 1\" --integrated --delay 100");
    return;
}

string? server = null;
string? database = null;
string? user = null;
string? password = null;
string? query = null;
string? file = null;
int threads = 1;
int delay = 0;
int timeout = 30;
bool integrated = false;

string GetNextArg(string[] arguments, ref int index, string optionName)
{
    if (index + 1 >= arguments.Length)
    {
        Console.WriteLine($"Error: {optionName} requires a value.");
        Environment.Exit(1);
    }
    return arguments[++index];
}

int ParseInt(string value, string optionName)
{
    if (!int.TryParse(value, out int result))
    {
        Console.WriteLine($"Error: {optionName} must be a valid integer.");
        Environment.Exit(1);
    }
    return result;
}

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--server":
        case "-s":
            server = GetNextArg(args, ref i, "--server");
            break;
        case "--database":
        case "-d":
            database = GetNextArg(args, ref i, "--database");
            break;
        case "--user":
        case "-u":
            user = GetNextArg(args, ref i, "--user");
            break;
        case "--password":
        case "-p":
            password = GetNextArg(args, ref i, "--password");
            break;
        case "--query":
        case "-q":
            query = GetNextArg(args, ref i, "--query");
            break;
        case "--file":
        case "-f":
            file = GetNextArg(args, ref i, "--file");
            break;
        case "--threads":
        case "-t":
            threads = ParseInt(GetNextArg(args, ref i, "--threads"), "--threads");
            break;
        case "--delay":
            delay = ParseInt(GetNextArg(args, ref i, "--delay"), "--delay");
            break;
        case "--timeout":
            timeout = ParseInt(GetNextArg(args, ref i, "--timeout"), "--timeout");
            break;
        case "--integrated":
            integrated = true;
            break;
    }
}

if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
{
    Console.WriteLine("Error: --server and --database are required.");
    return;
}

if (!integrated && (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password)))
{
    Console.WriteLine("Error: Use --integrated or provide --user and --password.");
    return;
}

string sql;
if (!string.IsNullOrEmpty(file))
{
    sql = File.ReadAllText(file);
}
else if (!string.IsNullOrEmpty(query))
{
    sql = query;
}
else
{
    Console.WriteLine("Error: Provide --query or --file.");
    return;
}

var connectionString = integrated
    ? $"Server={server};Database={database};Integrated Security=True;TrustServerCertificate=True;"
    : $"Server={server};Database={database};User Id={user};Password={password};TrustServerCertificate=True;";

Console.WriteLine($"Starting {threads} thread(s) with {delay}ms delay each...");

var tasks = new Task[threads];
for (int i = 0; i < threads; i++)
{
    int threadId = i + 1;
    tasks[i] = Task.Run(async () =>
    {
        if (delay > 0)
        {
            await Task.Delay(delay * threadId);
        }

        Console.WriteLine($"[Thread {threadId}] Starting...");
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = timeout;

            using var reader = await command.ExecuteReaderAsync();
            int rowCount = 0;
            while (await reader.ReadAsync())
            {
                rowCount++;
            }
            Console.WriteLine($"[Thread {threadId}] Completed. Rows: {rowCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Thread {threadId}] Error: {ex.Message}");
        }
    });
}

await Task.WhenAll(tasks);
Console.WriteLine("All threads completed.");
