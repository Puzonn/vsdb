using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace vsbd.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectFlowTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "Pins");

            migrationBuilder.DropTable(
                name: "Nodes");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Projects",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "FlowEdges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<string>(type: "text", nullable: false),
                    SourceHandleId = table.Column<string>(type: "text", nullable: true),
                    TargetHandleId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowEdges_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowNodes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ProjectId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowNodes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlowNodeProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlowNodeId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowNodeProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlowNodeProperties_FlowNodes_FlowNodeId",
                        column: x => x.FlowNodeId,
                        principalTable: "FlowNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlowEdges_ProjectId",
                table: "FlowEdges",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowEdges_SourceId",
                table: "FlowEdges",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowEdges_TargetId",
                table: "FlowEdges",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowNodeProperties_FlowNodeId",
                table: "FlowNodeProperties",
                column: "FlowNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowNodes_ProjectId",
                table: "FlowNodes",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowEdges");

            migrationBuilder.DropTable(
                name: "FlowNodeProperties");

            migrationBuilder.DropTable(
                name: "FlowNodes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Projects");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Projects",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionX = table.Column<float>(type: "real", nullable: false),
                    PositionY = table.Column<float>(type: "real", nullable: false),
                    SourceCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Nodes_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pins_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourcePinId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPinId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edges_Nodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Edges_Nodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Edges_Pins_SourcePinId",
                        column: x => x.SourcePinId,
                        principalTable: "Pins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Edges_Pins_TargetPinId",
                        column: x => x.TargetPinId,
                        principalTable: "Pins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Edges_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Edges_ProjectId",
                table: "Edges",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_SourceNodeId",
                table: "Edges",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_SourcePinId",
                table: "Edges",
                column: "SourcePinId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_TargetNodeId",
                table: "Edges",
                column: "TargetNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_TargetPinId",
                table: "Edges",
                column: "TargetPinId");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_ProjectId",
                table: "Nodes",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Pins_NodeId",
                table: "Pins",
                column: "NodeId");
        }
    }
}
