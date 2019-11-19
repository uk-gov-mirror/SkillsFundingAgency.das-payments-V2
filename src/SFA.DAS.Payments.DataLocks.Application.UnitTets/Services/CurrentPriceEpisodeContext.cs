﻿using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.DataLocks.Domain.Models;
using SFA.DAS.Payments.DataLocks.Domain.Services.PriceEpidodeChanges;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Payments.DataLocks.Domain.UnitTests.Services
{
    public class CurrentPriceEpisodeContext : DbContext, ICurrentPriceEpisodeForJobStore
    {
        public DbSet<CurrentPriceEpisode> Prices { get; set; }

        public CurrentPriceEpisodeContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrentPriceEpisode>().Property<long>("Id");
        }

        public void Add(CurrentPriceEpisode priceEpisode)
        {
            Prices.Add(priceEpisode);
            SaveChanges();
        }

        public IEnumerable<CurrentPriceEpisode> GetCurentPriceEpisodes(long jobId, long ukprn)
        {
            return Prices;
        }

        public void Replace(long jobId, long ukprn, IEnumerable<CurrentPriceEpisode> priceEpisodes)
        {
            Prices.RemoveRange(Prices.Where(x => x.JobId == jobId && x.Ukprn == ukprn));
            Prices.AddRange(priceEpisodes);
            SaveChanges();

        }
    }
}
