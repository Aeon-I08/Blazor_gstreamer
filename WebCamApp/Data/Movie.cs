using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebCamApp.Data {
	public partial class MovieDbContext : DbContext {
		public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }
		public DbSet<Movie> Movies { get; set; } = null!;
		public DbSet<MovieData> MovieDatas { get; set; } = null!;
	}

	[Table("movie")]
	public class Movie {
		[Column("id"), Required, Key]
		public int Id { get; set; }

		[Column("name"), Required]
		public string Name { get; set; }

		[Column("start_time"), Required]
		public DateTime StartTime { get; set; }

		[Column("end_time"), Required]
		public DateTime EndTime { get; set; }
	}

	[Table("movie_data")]
	public class MovieData {
		[Column("id"), Required, Key]
		public int Id { get; set; }

		[Column("record_data")]
		public byte[] RecordData { get; set; }
	}
}
