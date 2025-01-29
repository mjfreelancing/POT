﻿using Microsoft.EntityFrameworkCore;
using Pot.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Pot.Data.Entities
{
    [Index("AccountId", nameof(Description), IsUnique = true)]
    [Index(nameof(NextDue), IsUnique = false)]
    public sealed class ExpenseEntity : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public required string Description { get; set; }

        public DateOnly NextDue { get; set; }
        public DateOnly AccrualStart { get; set; }
        public required ExpenseFrequency Frequency { get; set; }
        public int FrequencyCount { get; set; }
        public bool Recurring { get; set; }
        public double Amount { get; set; }
        public double Allocated { get; set; }

        //[Required]
        public AccountEntity? Account { get; set; }
    }
}