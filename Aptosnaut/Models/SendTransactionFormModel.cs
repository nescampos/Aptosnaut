﻿using System.ComponentModel.DataAnnotations;

namespace Aptosnaut.Models
{
    public class SendTransactionFormModel
    {
        [Required]
        public decimal? Amount { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public string? Token { get; set; }
    }
}
