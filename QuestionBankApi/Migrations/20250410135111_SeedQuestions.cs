using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuestionBankApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Questions",
                columns: new[] { "Id", "Analysis", "AnswersJson", "Category", "Content", "CreateTime", "Difficulty", "ExamplesJson", "OptionsJson", "ReferenceAnswer", "TagsJson", "Type" },
                values: new object[,]
                {
                    { 1, "中国的首都是北京。", "[\"北京\"]", null, "中国的首都是哪里？", new DateTime(2025, 4, 10, 21, 51, 11, 740, DateTimeKind.Local).AddTicks(2201), 1, null, "[\"北京\", \"上海\", \"广州\", \"深圳\"]", null, null, 0 },
                    { 2, "太阳是一颗恒星。", "[\"正确\"]", null, "太阳是恒星。", new DateTime(2025, 4, 10, 21, 51, 11, 740, DateTimeKind.Local).AddTicks(2213), 1, null, null, null, null, 10 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Questions",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
