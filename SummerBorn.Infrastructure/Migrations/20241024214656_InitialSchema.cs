using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummerBorn.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "establishment_group",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_establishment_group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "establishment_status",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_establishment_status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "establishment_type",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_establishment_type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "local_authority",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_local_authority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "phase_of_education",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phase_of_education", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "school",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    URN = table.Column<int>(type: "integer", nullable: false),
                    UKPRN = table.Column<int>(type: "integer", nullable: false),
                    EstablishmentNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Street = table.Column<string>(type: "text", nullable: true),
                    Locality = table.Column<string>(type: "text", nullable: true),
                    AddressThree = table.Column<string>(type: "text", nullable: true),
                    Town = table.Column<string>(type: "text", nullable: true),
                    County = table.Column<string>(type: "text", nullable: true),
                    PostCode = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    OpenDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CloseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PhaseOfEducationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocalAuthorityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstablishmentStatusId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school", x => x.Id);
                    table.ForeignKey(
                        name: "FK_school_establishment_group_EstablishmentGroupId",
                        column: x => x.EstablishmentGroupId,
                        principalTable: "establishment_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_establishment_status_EstablishmentStatusId",
                        column: x => x.EstablishmentStatusId,
                        principalTable: "establishment_status",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_establishment_type_EstablishmentTypeId",
                        column: x => x.EstablishmentTypeId,
                        principalTable: "establishment_type",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_local_authority_LocalAuthorityId",
                        column: x => x.LocalAuthorityId,
                        principalTable: "local_authority",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_school_phase_of_education_PhaseOfEducationId",
                        column: x => x.PhaseOfEducationId,
                        principalTable: "phase_of_education",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_school_AddressThree",
                table: "school",
                column: "AddressThree");

            migrationBuilder.CreateIndex(
                name: "IX_school_County",
                table: "school",
                column: "County");

            migrationBuilder.CreateIndex(
                name: "IX_school_EstablishmentGroupId",
                table: "school",
                column: "EstablishmentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_school_EstablishmentNumber",
                table: "school",
                column: "EstablishmentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_school_EstablishmentStatusId",
                table: "school",
                column: "EstablishmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_school_EstablishmentTypeId",
                table: "school",
                column: "EstablishmentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_school_LocalAuthorityId",
                table: "school",
                column: "LocalAuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_school_Locality",
                table: "school",
                column: "Locality");

            migrationBuilder.CreateIndex(
                name: "IX_school_Name",
                table: "school",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_school_PhaseOfEducationId",
                table: "school",
                column: "PhaseOfEducationId");

            migrationBuilder.CreateIndex(
                name: "IX_school_PostCode",
                table: "school",
                column: "PostCode");

            migrationBuilder.CreateIndex(
                name: "IX_school_Street",
                table: "school",
                column: "Street");

            migrationBuilder.CreateIndex(
                name: "IX_school_Town",
                table: "school",
                column: "Town");

            migrationBuilder.CreateIndex(
                name: "IX_school_UKPRN",
                table: "school",
                column: "UKPRN");

            migrationBuilder.CreateIndex(
                name: "IX_school_URN",
                table: "school",
                column: "URN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "school");

            migrationBuilder.DropTable(
                name: "establishment_group");

            migrationBuilder.DropTable(
                name: "establishment_status");

            migrationBuilder.DropTable(
                name: "establishment_type");

            migrationBuilder.DropTable(
                name: "local_authority");

            migrationBuilder.DropTable(
                name: "phase_of_education");
        }
    }
}
