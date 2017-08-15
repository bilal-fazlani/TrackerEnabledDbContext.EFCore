﻿using System.ComponentModel.DataAnnotations;

namespace TrackerEnabledDbContext.EFCore.Tests.Common.Models
{
    [TrackChanges]
    public class SoftDeletableModel :ISoftDeletable
    {
        public long Id { get; set; }

        public bool IsDeleted { get; set; }

        public string Description { get; set; }

        public void Delete()
        {
            IsDeleted = true;
        }
    }
}
