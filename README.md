# OctopusQL

Multi-threaded SQL Query Runner - a simple C# console application that executes SQL queries across multiple threads.

## Build

```bash
dotnet build
```

## Usage

```bash
OctopusQL --server <server> --database <database> --threads <count> [options]
```

### Required Parameters

- `--server, -s` - SQL Server address
- `--database, -d` - Database name  
- `--threads, -t` - Number of threads

### Authentication (choose one)

- `--integrated` - Use Windows Authentication
- `--user, -u` and `--password, -p` - SQL Server credentials

### Query (choose one)

- `--query, -q` - SQL query to execute
- `--file, -f` - Path to SQL file

### Optional Parameters

- `--delay` - Start delay per thread in milliseconds (default: 0)
- `--timeout` - Command timeout in seconds (default: 30)

## Examples

Using Windows Authentication:
```bash
dotnet run -- -s localhost -d TestDB -t 5 -q "SELECT 1" --integrated
```

Using SQL Authentication with delay:
```bash
dotnet run -- -s localhost -d TestDB -t 3 -u sa -p myPassword -q "SELECT * FROM Users" --delay 100
```

Using SQL file:
```bash
dotnet run -- -s localhost -d TestDB -t 10 -f query.sql --integrated --delay 50
```