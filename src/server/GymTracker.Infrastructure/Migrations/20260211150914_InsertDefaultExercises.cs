using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InsertDefaultExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Muscle categories (system defaults, UserId = null)
            migrationBuilder.InsertData(
                table: "muscle_categories",
                columns: new[] { "id", "user_id", "name" },
                values: new object[,]
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), null, "Push" },
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), null, "Pull" },
                    { Guid.Parse("33333333-3333-3333-3333-333333333333"), null, "Legs" },
                    { Guid.Parse("44444444-4444-4444-4444-444444444444"), null, "Core" }
                });

            // Muscle groups (system defaults, UserId = null)
            migrationBuilder.InsertData(
                table: "muscle_groups",
                columns: new[] { "id", "user_id", "category_id", "name" },
                values: new object[,]
                {
                    // Push
                    { Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), null, Guid.Parse("11111111-1111-1111-1111-111111111111"), "Chest Compound" },
                    { Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, Guid.Parse("11111111-1111-1111-1111-111111111111"), "Chest Press" },
                    { Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), null, Guid.Parse("11111111-1111-1111-1111-111111111111"), "Chest Fly" },
                    { Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"), null, Guid.Parse("11111111-1111-1111-1111-111111111111"), "Deltoids Compound" },
                    { Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), null, Guid.Parse("11111111-1111-1111-1111-111111111111"), "Lateral Deltoids" },
                    { Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, Guid.Parse("11111111-1111-1111-1111-111111111111"), "Triceps" },

                    // Pull
                    { Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), null, Guid.Parse("22222222-2222-2222-2222-222222222222"), "Back Pulling" },
                    { Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, Guid.Parse("22222222-2222-2222-2222-222222222222"), "Back Rowing" },
                    { Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), null, Guid.Parse("22222222-2222-2222-2222-222222222222"), "Trapezius" },
                    { Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), null, Guid.Parse("22222222-2222-2222-2222-222222222222"), "Rear Deltoids" },
                    { Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, Guid.Parse("22222222-2222-2222-2222-222222222222"), "Biceps" },
                    { Guid.Parse("bbbbbbbb-0000-0000-0000-000000000006"), null, Guid.Parse("22222222-2222-2222-2222-222222222222"), "Forearms" },

                    // Legs
                    { Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, Guid.Parse("33333333-3333-3333-3333-333333333333"), "Quadriceps" },
                    { Guid.Parse("cccccccc-0000-0000-0000-000000000002"), null, Guid.Parse("33333333-3333-3333-3333-333333333333"), "Quadriceps Compound" },
                    { Guid.Parse("cccccccc-0000-0000-0000-000000000003"), null, Guid.Parse("33333333-3333-3333-3333-333333333333"), "Hamstrings" },
                    { Guid.Parse("cccccccc-0000-0000-0000-000000000004"), null, Guid.Parse("33333333-3333-3333-3333-333333333333"), "Calves" },

                    // Core
                    { Guid.Parse("dddddddd-0000-0000-0000-000000000001"), null, Guid.Parse("44444444-4444-4444-4444-444444444444"), "Anterior Flexion" },
                    { Guid.Parse("dddddddd-0000-0000-0000-000000000002"), null, Guid.Parse("44444444-4444-4444-4444-444444444444"), "Anterior Isometric" },
                    { Guid.Parse("dddddddd-0000-0000-0000-000000000003"), null, Guid.Parse("44444444-4444-4444-4444-444444444444"), "Obliques" }
                });

            // Exercises (system defaults, UserId = null)
            migrationBuilder.InsertData(
                table: "exercises",
                columns: new[] { "id", "muscle_group_id", "user_id", "name" },
                values: new object[,]
                {
                    // Chest Compound
                    { Guid.Parse("e0000000-0000-0000-0000-000000000001"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), null, "Bench Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000002"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), null, "Incline Bench Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000003"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), null, "Weighted Dips" },

                    // Chest Press
                    { Guid.Parse("e0000000-0000-0000-0000-000000000011"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, "Dumbbell Bench Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000012"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, "Incline Dumbbell Bench Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000013"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, "Machine Chest Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000014"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, "Incline Machine Chest Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000015"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, "Decline Bench Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000016"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), null, "Floor Press" },

                    // Chest Fly
                    { Guid.Parse("e0000000-0000-0000-0000-000000000021"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), null, "Dumbbell Fly" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000022"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), null, "Incline Dumbbell Fly" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000023"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), null, "Cable Crossovers" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000024"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), null, "Machine Chest Fly" },

                    // Deltoids Compound
                    { Guid.Parse("e0000000-0000-0000-0000-000000000031"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"), null, "Overhead Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000032"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"), null, "Seated Overhead Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000033"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"), null, "Dumbbell Shoulder Press" },

                    // Lateral Deltoids
                    { Guid.Parse("e0000000-0000-0000-0000-000000000041"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), null, "Lateral Dumbbell Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000042"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), null, "Lateral Cable Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000043"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), null, "Machine Lateral Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000044"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), null, "Cable Y Raise" },

                    // Triceps
                    { Guid.Parse("e0000000-0000-0000-0000-000000000051"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Cable Tricep Extension" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000052"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Dumbbell Tricep Kickback" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000053"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Skullcrushers" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000054"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Dumbbell Skullcrushers" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000055"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Machine Tricep Extension" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000056"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Close-Grip Bench Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000057"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Overhead Tricep Extension" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000058"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Dips" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000059"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"), null, "Bench Dips" },

                    // Back Pulling
                    { Guid.Parse("e0000000-0000-0000-0000-000000000061"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), null, "Pull Ups" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000062"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), null, "Chin Ups" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000063"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), null, "Lat Pulldown" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000064"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), null, "Single-Arm Cable Pulldown" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000065"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), null, "Machine Pullover" },

                    // Back Rowing
                    { Guid.Parse("e0000000-0000-0000-0000-000000000071"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, "Dumbbell Row" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000072"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, "Barbell Row" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000073"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, "T-Bar Row" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000074"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, "Seated Cable Row" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000075"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, "Smith Machine Row" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000076"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), null, "Machine Row" },

                    // Trapezius
                    { Guid.Parse("e0000000-0000-0000-0000-000000000081"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), null, "Barbell Shrugs" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000082"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), null, "Dumbbell Shrugs" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000083"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), null, "Cable Shrugs" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000084"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), null, "Trap Bar Shrugs" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000085"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), null, "Smith Machine Shrugs" },

                    // Rear Deltoids
                    { Guid.Parse("e0000000-0000-0000-0000-000000000091"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), null, "Reverse Pec Deck" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000092"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), null, "Rear Deltoid Fly" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000093"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), null, "Face Pulls" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000094"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), null, "Cable Rear Lateral Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000095"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), null, "Barbell Rear Deltoid Row" },

                    // Biceps
                    { Guid.Parse("e0000000-0000-0000-0000-000000000101"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Dumbbell Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000102"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Concentration Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000103"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Dumbbell Preacher Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000104"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Hammer Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000105"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Barbell Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000106"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "EZ Bar Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000107"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Preacher Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000108"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Cable Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000109"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), null, "Machine Curl" },

                    // Forearms
                    { Guid.Parse("e0000000-0000-0000-0000-00000000011a"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000006"), null, "Wrist Curl" },

                    // Quadriceps
                    { Guid.Parse("e0000000-0000-0000-0000-000000000121"), Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, "Leg Press" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000122"), Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, "Bulgarian Split Squat" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000123"), Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, "Dumbbell Lunge" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000124"), Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, "Barbell Lunge" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000125"), Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, "Hack Squat" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000126"), Guid.Parse("cccccccc-0000-0000-0000-000000000001"), null, "Leg Extension" },

                    // Quadriceps Compound
                    { Guid.Parse("e0000000-0000-0000-0000-000000000131"), Guid.Parse("cccccccc-0000-0000-0000-000000000002"), null, "Squat" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000132"), Guid.Parse("cccccccc-0000-0000-0000-000000000002"), null, "Front Squat" },

                    // Hamstrings
                    { Guid.Parse("e0000000-0000-0000-0000-000000000141"), Guid.Parse("cccccccc-0000-0000-0000-000000000003"), null, "Stiff-Leg Deadlift" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000142"), Guid.Parse("cccccccc-0000-0000-0000-000000000003"), null, "Romanian Deadlift" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000143"), Guid.Parse("cccccccc-0000-0000-0000-000000000003"), null, "Dumbbell Stiff-Leg Deadlift" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000144"), Guid.Parse("cccccccc-0000-0000-0000-000000000003"), null, "Lying Leg Curl" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000145"), Guid.Parse("cccccccc-0000-0000-0000-000000000003"), null, "Seated Leg Curl" },

                    // Calves
                    { Guid.Parse("e0000000-0000-0000-0000-000000000151"), Guid.Parse("cccccccc-0000-0000-0000-000000000004"), null, "Seated Calf Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000152"), Guid.Parse("cccccccc-0000-0000-0000-000000000004"), null, "Standing Calf Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000153"), Guid.Parse("cccccccc-0000-0000-0000-000000000004"), null, "Single-Calf Stair Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000154"), Guid.Parse("cccccccc-0000-0000-0000-000000000004"), null, "Donkey Calf Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000155"), Guid.Parse("cccccccc-0000-0000-0000-000000000004"), null, "Smith Machine Calf Raise" },

                    // Anterior Flexion
                    { Guid.Parse("e0000000-0000-0000-0000-000000000161"), Guid.Parse("dddddddd-0000-0000-0000-000000000001"), null, "Leg Raise" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000162"), Guid.Parse("dddddddd-0000-0000-0000-000000000001"), null, "Weighted Decline Sit Ups" },

                    // Anterior Isometric
                    { Guid.Parse("e0000000-0000-0000-0000-000000000171"), Guid.Parse("dddddddd-0000-0000-0000-000000000002"), null, "Dead Bug" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000172"), Guid.Parse("dddddddd-0000-0000-0000-000000000002"), null, "Plank" },

                    // Obliques
                    { Guid.Parse("e0000000-0000-0000-0000-000000000181"), Guid.Parse("dddddddd-0000-0000-0000-000000000003"), null, "Woodchoppers" },
                    { Guid.Parse("e0000000-0000-0000-0000-000000000182"), Guid.Parse("dddddddd-0000-0000-0000-000000000003"), null, "Russian Twists" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete exercises first (FK to muscle_groups)
            migrationBuilder.DeleteData(
                table: "exercises",
                keyColumn: "id",
                keyValues: new object[] {
                    Guid.Parse("e0000000-0000-0000-0000-000000000001"), Guid.Parse("e0000000-0000-0000-0000-000000000002"), Guid.Parse("e0000000-0000-0000-0000-000000000003"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000011"), Guid.Parse("e0000000-0000-0000-0000-000000000012"), Guid.Parse("e0000000-0000-0000-0000-000000000013"), Guid.Parse("e0000000-0000-0000-0000-000000000014"), Guid.Parse("e0000000-0000-0000-0000-000000000015"), Guid.Parse("e0000000-0000-0000-0000-000000000016"), Guid.Parse("e0000000-0000-0000-0000-000000000017"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000021"), Guid.Parse("e0000000-0000-0000-0000-000000000022"), Guid.Parse("e0000000-0000-0000-0000-000000000023"), Guid.Parse("e0000000-0000-0000-0000-000000000024"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000031"), Guid.Parse("e0000000-0000-0000-0000-000000000032"), Guid.Parse("e0000000-0000-0000-0000-000000000033"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000041"), Guid.Parse("e0000000-0000-0000-0000-000000000042"), Guid.Parse("e0000000-0000-0000-0000-000000000043"), Guid.Parse("e0000000-0000-0000-0000-000000000044"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000051"), Guid.Parse("e0000000-0000-0000-0000-000000000052"), Guid.Parse("e0000000-0000-0000-0000-000000000053"), Guid.Parse("e0000000-0000-0000-0000-000000000054"), Guid.Parse("e0000000-0000-0000-0000-000000000055"), Guid.Parse("e0000000-0000-0000-0000-000000000056"), Guid.Parse("e0000000-0000-0000-0000-000000000057"), Guid.Parse("e0000000-0000-0000-0000-000000000058"), Guid.Parse("e0000000-0000-0000-0000-000000000059"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000061"), Guid.Parse("e0000000-0000-0000-0000-000000000062"), Guid.Parse("e0000000-0000-0000-0000-000000000063"), Guid.Parse("e0000000-0000-0000-0000-000000000064"), Guid.Parse("e0000000-0000-0000-0000-000000000065"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000071"), Guid.Parse("e0000000-0000-0000-0000-000000000072"), Guid.Parse("e0000000-0000-0000-0000-000000000073"), Guid.Parse("e0000000-0000-0000-0000-000000000074"), Guid.Parse("e0000000-0000-0000-0000-000000000075"), Guid.Parse("e0000000-0000-0000-0000-000000000076"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000081"), Guid.Parse("e0000000-0000-0000-0000-000000000082"), Guid.Parse("e0000000-0000-0000-0000-000000000083"), Guid.Parse("e0000000-0000-0000-0000-000000000084"), Guid.Parse("e0000000-0000-0000-0000-000000000085"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000091"), Guid.Parse("e0000000-0000-0000-0000-000000000092"), Guid.Parse("e0000000-0000-0000-0000-000000000093"), Guid.Parse("e0000000-0000-0000-0000-000000000094"), Guid.Parse("e0000000-0000-0000-0000-000000000095"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000101"), Guid.Parse("e0000000-0000-0000-0000-000000000102"), Guid.Parse("e0000000-0000-0000-0000-000000000103"), Guid.Parse("e0000000-0000-0000-0000-000000000104"), Guid.Parse("e0000000-0000-0000-0000-000000000105"), Guid.Parse("e0000000-0000-0000-0000-000000000106"), Guid.Parse("e0000000-0000-0000-0000-000000000107"), Guid.Parse("e0000000-0000-0000-0000-000000000108"), Guid.Parse("e0000000-0000-0000-0000-000000000109"), Guid.Parse("e0000000-0000-0000-0000-00000000010a"),
                    Guid.Parse("e0000000-0000-0000-0000-00000000011a"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000121"), Guid.Parse("e0000000-0000-0000-0000-000000000122"), Guid.Parse("e0000000-0000-0000-0000-000000000123"), Guid.Parse("e0000000-0000-0000-0000-000000000124"), Guid.Parse("e0000000-0000-0000-0000-000000000125"), Guid.Parse("e0000000-0000-0000-0000-000000000126"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000131"), Guid.Parse("e0000000-0000-0000-0000-000000000132"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000141"), Guid.Parse("e0000000-0000-0000-0000-000000000142"), Guid.Parse("e0000000-0000-0000-0000-000000000143"), Guid.Parse("e0000000-0000-0000-0000-000000000144"), Guid.Parse("e0000000-0000-0000-0000-000000000145"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000151"), Guid.Parse("e0000000-0000-0000-0000-000000000152"), Guid.Parse("e0000000-0000-0000-0000-000000000153"), Guid.Parse("e0000000-0000-0000-0000-000000000154"), Guid.Parse("e0000000-0000-0000-0000-000000000155"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000161"), Guid.Parse("e0000000-0000-0000-0000-000000000162"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000171"), Guid.Parse("e0000000-0000-0000-0000-000000000172"),
                    Guid.Parse("e0000000-0000-0000-0000-000000000181"), Guid.Parse("e0000000-0000-0000-0000-000000000182")
                });

            // Delete muscle groups
            migrationBuilder.DeleteData(
                table: "muscle_groups",
                keyColumn: "id",
                keyValues: new object[] {
                    Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"), Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"),
                    Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000004"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000005"), Guid.Parse("bbbbbbbb-0000-0000-0000-000000000006"),
                    Guid.Parse("cccccccc-0000-0000-0000-000000000001"), Guid.Parse("cccccccc-0000-0000-0000-000000000002"), Guid.Parse("cccccccc-0000-0000-0000-000000000003"), Guid.Parse("cccccccc-0000-0000-0000-000000000004"),
                    Guid.Parse("dddddddd-0000-0000-0000-000000000001"), Guid.Parse("dddddddd-0000-0000-0000-000000000002"), Guid.Parse("dddddddd-0000-0000-0000-000000000003")
                });

            // Delete muscle categories
            migrationBuilder.DeleteData(
                table: "muscle_categories",
                keyColumn: "id",
                keyValues: new object[] {
                    Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("22222222-2222-2222-2222-222222222222"), Guid.Parse("33333333-3333-3333-3333-333333333333"), Guid.Parse("44444444-4444-4444-4444-444444444444")
                });
        }
    }
}
