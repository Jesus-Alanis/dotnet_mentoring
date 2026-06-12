using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPartitionSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobExecution",
                table: "JobExecution");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "scheduled_time",
                table: "JobExecution",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "error_message",
                table: "JobExecution",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Job",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "is_recurrent",
                table: "Job",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_execution_at",
                table: "Job",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobExecution",
                table: "JobExecution",
                columns: new[] { "job_id", "scheduled_time" });

            migrationBuilder.CreateIndex(
                name: "IX_Job_Id_Status",
                table: "Job",
                columns: new[] { "Id", "Status" });

            // EF Core doesn't do this natively
            // Inject the Partition Function and Scheme
            migrationBuilder.Sql(@"
                CREATE PARTITION FUNCTION pf_JobScheduledTime (datetimeoffset)
                AS RANGE RIGHT FOR VALUES ('2026-06-01T00:00:00', '2026-07-01T00:00:00', '2026-08-01T00:00:00');

                CREATE PARTITION SCHEME ps_JobScheduledTime
                AS PARTITION pf_JobScheduledTime ALL TO ([PRIMARY]);
            ");

            // Bind the table to the scheme
            migrationBuilder.Sql("CREATE UNIQUE CLUSTERED INDEX PK_JobExecution ON JobExecution(job_id, scheduled_time) WITH (DROP_EXISTING = ON) ON ps_JobScheduledTime(scheduled_time);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_JobExecution",
                table: "JobExecution");

            migrationBuilder.DropIndex(
                name: "IX_Job_Id_Status",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "scheduled_time",
                table: "JobExecution");

            migrationBuilder.DropColumn(
                name: "error_message",
                table: "JobExecution");

            migrationBuilder.DropColumn(
                name: "is_recurrent",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "next_execution_at",
                table: "Job");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Job",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_JobExecution",
                table: "JobExecution",
                column: "Id");

            migrationBuilder.Sql(@"
                DROP PARTITION SCHEME ps_JobScheduledTime;
                DROP PARTITION FUNCTION pf_JobScheduledTime;
            ");
        }
    }
}
