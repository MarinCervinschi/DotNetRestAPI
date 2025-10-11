using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seed Books
            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Title", "Author", "ISBN", "Status" },
                values: new object[,]
                {
                    { "The Name of the Rose", "Umberto Eco", "9788845292613", 0 },
                    { "1984", "George Orwell", "9788804679356", 0 },
                    { "The Little Prince", "Antoine de Saint-Exupéry", "9788845292514", 0 },
                    { "Pride and Prejudice", "Jane Austen", "9788804679363", 0 },
                    { "The Lord of the Rings - The Fellowship of the Ring", "J.R.R. Tolkien", "9788845292521", 0 },
                    { "To Kill a Mockingbird", "Harper Lee", "9788804679370", 0 },
                    { "The Great Gatsby", "F. Scott Fitzgerald", "9788845292538", 0 },
                    { "Harry Potter and the Philosopher's Stone", "J.K. Rowling", "9788831003384", 0 },
                    { "Don Quixote", "Miguel de Cervantes", "9788845292545", 0 },
                    { "Moby Dick", "Herman Melville", "9788804679387", 0 }
                });

            // Seed Customers
            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "FirstName", "LastName", "Email" },
                values: new object[,]
                {
                    { "John", "Smith", "john.smith@email.com" },
                    { "Emily", "Johnson", "emily.johnson@email.com" },
                    { "Michael", "Brown", "michael.brown@email.com" },
                    { "Sarah", "Davis", "sarah.davis@email.com" },
                    { "David", "Wilson", "david.wilson@email.com" },
                    { "Jessica", "Garcia", "jessica.garcia@email.com" },
                    { "Christopher", "Martinez", "christopher.martinez@email.com" },
                    { "Ashley", "Anderson", "ashley.anderson@email.com" }
                });

            // Seed Reservations
            var reservationDates = new DateTime[]
            {
                DateTime.UtcNow.AddDays(-10).AddHours(15),    // David reserves "To Kill a Mockingbird" - 10 days ago (EXPIRED)
                DateTime.UtcNow.AddDays(-5).AddHours(10.5),   // John reserves "The Name of the Rose" - 5 days ago
                DateTime.UtcNow.AddDays(-3).AddHours(14.25),  // Emily reserves "1984" - 3 days ago  
                DateTime.UtcNow.AddDays(-1).AddHours(9.75),   // Michael reserves "The Little Prince" - 1 day ago
                DateTime.UtcNow.AddHours(-6),                 // John reserves "Pride and Prejudice" - 6 hours ago
                DateTime.UtcNow.AddHours(-2)                  // Sarah reserves "LOTR" - 2 hours ago (most recent)
            };

            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "CustomerId", "BookId", "ReservationDate", "ExpirationDate" },
                values: new object[,]
                {
                    { 5, 6, reservationDates[0], reservationDates[0].AddDays(7) }, // EXPIRED (10 days ago + 7 = expired 3 days ago)
                    { 1, 1, reservationDates[1], reservationDates[1].AddDays(7) },
                    { 2, 2, reservationDates[2], reservationDates[2].AddDays(7) },
                    { 3, 3, reservationDates[3], reservationDates[3].AddDays(7) },
                    { 1, 4, reservationDates[4], reservationDates[4].AddDays(7) },
                    { 4, 5, reservationDates[5], reservationDates[5].AddDays(7) }
                });

            // Update reserved books status to Unavailable (only active reservations)
            migrationBuilder.Sql("UPDATE \"Books\" SET \"Status\" = 1 WHERE \"Id\" IN (1, 2, 3, 4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No need to explicitly delete Reservations - CASCADE DELETE handles it
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "Email",
                keyValues: new object[]
                {
                    "john.smith@email.com",
                    "emily.johnson@email.com",
                    "michael.brown@email.com",
                    "sarah.davis@email.com",
                    "david.wilson@email.com",
                    "jessica.garcia@email.com",
                    "christopher.martinez@email.com",
                    "ashley.anderson@email.com"
                });

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "ISBN",
                keyValues: new object[]
                {
                    "9788845292613",
                    "9788804679356",
                    "9788845292514",
                    "9788804679363",
                    "9788845292521",
                    "9788804679370",
                    "9788845292538",
                    "9788831003384",
                    "9788845292545",
                    "9788804679387"
                });
        }
    }
}
