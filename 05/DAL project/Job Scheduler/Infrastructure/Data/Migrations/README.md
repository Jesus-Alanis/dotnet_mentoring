
 dotnet ef migrations add InitialJobStoreSchema --context JobSqlDbContext --startup-project ../Demo  --output-dir Data/Migrations

  dotnet ef database update --context JobSqlDbContext --startup-project ../Demo