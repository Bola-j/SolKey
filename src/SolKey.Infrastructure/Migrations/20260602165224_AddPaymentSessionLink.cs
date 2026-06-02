using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SolKey.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSessionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Payments",
                keyColumn: "Id",
                keyValue: new Guid("62f9fa0b-167d-4207-3000-07b9fa06f510"),
                column: "SessionId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_ExplanationSessions_SessionId",
                table: "Payments",
                column: "SessionId",
                principalTable: "ExplanationSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_ExplanationSessions_SessionId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_SessionId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Payments");
        }
    }
}
