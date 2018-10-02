using AuthServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer.Models {
	public class ApplicationDbContext : DbContext {
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

		public DbSet<User> Users { get; set; }

		public override int SaveChanges() {
			AddTimestamps();
			return base.SaveChanges();
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			AddTimestamps();

			return await base.SaveChangesAsync(cancellationToken);
		}

		private void AddTimestamps() {
			IEnumerable<EntityEntry> entities = ChangeTracker.Entries()
				.Where(x => x.Entity is DatedEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

			foreach(EntityEntry entity in entities) {
				DateTime now = DateTime.UtcNow;

				if(entity.State == EntityState.Added) {
					((DatedEntity)entity.Entity).CreatedAt = now;
				}

				((DatedEntity)entity.Entity).UpdatedAt = now;
			}

		}
	}
}
