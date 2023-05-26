using System;
using Microsoft.EntityFrameworkCore;

using Core.Domain.Enums;
using Core.Domain.Entities;

namespace Core.Domain.Infrastructure.Database
{
    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            //Seed the time to a constant to avoid fresh migrations in every migration event
            DateTime seedTime = new DateTime(2021, 4, 23, 18, 40, 0, 000, DateTimeKind.Unspecified);                

        }
    }
}