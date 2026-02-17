using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradeProgressMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassOfferings",
                columns: table => new
                {
                    ClassOfferingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubjectCode = table.Column<string>(type: "TEXT", nullable: false),
                    SubjectTitle = table.Column<string>(type: "TEXT", nullable: false),
                    Units = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CourseYearSection = table.Column<string>(type: "TEXT", nullable: false),
                    ClassType = table.Column<int>(type: "INTEGER", nullable: false),
                    Term = table.Column<string>(type: "TEXT", nullable: false),
                    SchoolYear = table.Column<string>(type: "TEXT", nullable: false),
                    Days = table.Column<string>(type: "TEXT", nullable: true),
                    Time = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassOfferings", x => x.ClassOfferingId);
                });

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    StudentProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Program = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    YearLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.StudentProfileId);
                });

            migrationBuilder.CreateTable(
                name: "SubjectCatalogs",
                columns: table => new
                {
                    SubjectCatalogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SubjectCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    SubjectTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Units = table.Column<decimal>(type: "TEXT", nullable: false),
                    Program = table.Column<string>(type: "TEXT", maxLength: 80, nullable: true),
                    YearLevel = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubjectCatalogs", x => x.SubjectCatalogId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradingComponents",
                columns: table => new
                {
                    GradingComponentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassOfferingId = table.Column<int>(type: "INTEGER", nullable: false),
                    ComponentType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    WeightPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradingComponents", x => x.GradingComponentId);
                    table.ForeignKey(
                        name: "FK_GradingComponents_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalTable: "ClassOfferings",
                        principalColumn: "ClassOfferingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    StudentProfileId = table.Column<int>(type: "INTEGER", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComponentItems",
                columns: table => new
                {
                    ComponentItemId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GradingComponentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    OrderNo = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxScore = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsOpenForEncoding = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsApproved = table.Column<bool>(type: "INTEGER", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentItems", x => x.ComponentItemId);
                    table.ForeignKey(
                        name: "FK_ComponentItems_GradingComponents_GradingComponentId",
                        column: x => x.GradingComponentId,
                        principalTable: "GradingComponents",
                        principalColumn: "GradingComponentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabRubricCriteria",
                columns: table => new
                {
                    LabRubricCriterionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GradingComponentId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    WeightPercent = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabRubricCriteria", x => x.LabRubricCriterionId);
                    table.ForeignKey(
                        name: "FK_LabRubricCriteria_GradingComponents_GradingComponentId",
                        column: x => x.GradingComponentId,
                        principalTable: "GradingComponents",
                        principalColumn: "GradingComponentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "BLOB", maxLength: 1024, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassOfferingId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentProfileId = table.Column<int>(type: "INTEGER", nullable: true),
                    StudentUserId = table.Column<string>(type: "TEXT", nullable: true),
                    StudentNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    StudentName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Section = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK_Enrollments_AspNetUsers_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Enrollments_ClassOfferings_ClassOfferingId",
                        column: x => x.ClassOfferingId,
                        principalTable: "ClassOfferings",
                        principalColumn: "ClassOfferingId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_StudentProfiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "StudentProfiles",
                        principalColumn: "StudentProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabActivitySubmissions",
                columns: table => new
                {
                    LabActivitySubmissionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComponentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentUserId = table.Column<string>(type: "TEXT", nullable: false),
                    DaysLate = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPlagiarized = table.Column<bool>(type: "INTEGER", nullable: false),
                    EncodedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabActivitySubmissions", x => x.LabActivitySubmissionId);
                    table.ForeignKey(
                        name: "FK_LabActivitySubmissions_ComponentItems_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "ComponentItems",
                        principalColumn: "ComponentItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoreEntries",
                columns: table => new
                {
                    ScoreEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ComponentItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    StudentUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", nullable: true),
                    EncodedByUserId = table.Column<string>(type: "TEXT", nullable: true),
                    EncodedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreEntries", x => x.ScoreEntryId);
                    table.ForeignKey(
                        name: "FK_ScoreEntries_AspNetUsers_StudentUserId",
                        column: x => x.StudentUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreEntries_ComponentItems_ComponentItemId",
                        column: x => x.ComponentItemId,
                        principalTable: "ComponentItems",
                        principalColumn: "ComponentItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabRubricScores",
                columns: table => new
                {
                    LabRubricScoreId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LabActivitySubmissionId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriterionName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabRubricScores", x => x.LabRubricScoreId);
                    table.ForeignKey(
                        name: "FK_LabRubricScores_LabActivitySubmissions_LabActivitySubmissionId",
                        column: x => x.LabActivitySubmissionId,
                        principalTable: "LabActivitySubmissions",
                        principalColumn: "LabActivitySubmissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_StudentProfileId",
                table: "AspNetUsers",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassOfferings_SubjectCode_Term_SchoolYear_CourseYearSection_ClassType",
                table: "ClassOfferings",
                columns: new[] { "SubjectCode", "Term", "SchoolYear", "CourseYearSection", "ClassType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComponentItems_GradingComponentId",
                table: "ComponentItems",
                column: "GradingComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_ClassOfferingId_StudentNo",
                table: "Enrollments",
                columns: new[] { "ClassOfferingId", "StudentNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentProfileId",
                table: "Enrollments",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentUserId",
                table: "Enrollments",
                column: "StudentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GradingComponents_ClassOfferingId",
                table: "GradingComponents",
                column: "ClassOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_LabActivitySubmissions_ComponentItemId_StudentUserId",
                table: "LabActivitySubmissions",
                columns: new[] { "ComponentItemId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabRubricCriteria_GradingComponentId",
                table: "LabRubricCriteria",
                column: "GradingComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_LabRubricScores_LabActivitySubmissionId",
                table: "LabRubricScores",
                column: "LabActivitySubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEntries_ComponentItemId_StudentUserId",
                table: "ScoreEntries",
                columns: new[] { "ComponentItemId", "StudentUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEntries_StudentUserId",
                table: "ScoreEntries",
                column: "StudentUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "LabRubricCriteria");

            migrationBuilder.DropTable(
                name: "LabRubricScores");

            migrationBuilder.DropTable(
                name: "ScoreEntries");

            migrationBuilder.DropTable(
                name: "SubjectCatalogs");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "LabActivitySubmissions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ComponentItems");

            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "GradingComponents");

            migrationBuilder.DropTable(
                name: "ClassOfferings");
        }
    }
}
