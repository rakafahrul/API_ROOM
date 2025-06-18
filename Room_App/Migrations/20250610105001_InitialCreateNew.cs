using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Room_App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // SQL ini sudah menambahkan kolom facilities
            migrationBuilder.Sql(@"DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'rooms') THEN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'rooms' AND column_name = 'facilities') THEN
                            ALTER TABLE rooms ADD COLUMN facilities text[] DEFAULT '{}' NOT NULL;
                        END IF;
                    END IF;
                END $$;"
            );

            // HAPUS BAGIAN INI - sudah ditambahkan via SQL di atas
            // migrationBuilder.AddColumn<List<string>>(
            //     name: "facilities",
            //     table: "rooms",
            //     type: "text[]",
            //     nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "room_photo_url",
                table: "bookings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "location_gps",
                table: "bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "facilities",
                table: "rooms");

            migrationBuilder.AlterColumn<string>(
                name: "room_photo_url",
                table: "bookings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "location_gps",
                table: "bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}