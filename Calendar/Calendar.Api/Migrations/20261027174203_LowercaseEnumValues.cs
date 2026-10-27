using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calendar.Api.Migrations
{
    /// <inheritdoc />
    public partial class LowercaseEnumValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Схема не меняется: переводим уже сохранённые значения enum'ов в нижний регистр.
            migrationBuilder.Sql("UPDATE \"CalendarEvents\" SET \"Type\" = lower(\"Type\");");
            migrationBuilder.Sql("UPDATE \"EventParticipants\" SET \"RsvpStatus\" = lower(\"RsvpStatus\");");
            migrationBuilder.Sql("UPDATE \"Notifications\" SET \"Type\" = lower(\"Type\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"CalendarEvents\" SET \"Type\" = initcap(\"Type\");");
            migrationBuilder.Sql("UPDATE \"EventParticipants\" SET \"RsvpStatus\" = initcap(\"RsvpStatus\");");
            migrationBuilder.Sql("UPDATE \"Notifications\" SET \"Type\" = initcap(\"Type\");");
        }
    }
}
