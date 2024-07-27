﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Core.Domain.Entities;

[Index(nameof(MaxUSDRange), IsUnique = true)]
public class CommissionRate
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName="decimal(20,9)")]
    [DecimalRange("0", Constants.DecimalMaxValue, ErrorMessage = "The 'MaxUSDRange' Must Be Positive")]
    public decimal MaxUSDRange { get; set; }
    
    [DecimalRange("0","0.5")]
    [Column(TypeName="decimal(6,5)")]
    public decimal CRate { get; set; }
}