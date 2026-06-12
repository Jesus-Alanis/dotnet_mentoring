
 dotnet ef migrations add InitialJobStoreSchema --context JobStoreDbContext --startup-project ../Demo  --output-dir Data/Migrations

  dotnet ef database update --context JobStoreDbContext --startup-project ../Demo