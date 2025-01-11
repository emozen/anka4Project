using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblPeriodItem
{
    public int Id { get; set; }

    public int? PeriodId { get; set; }

    public bool? IsIncoming { get; set; }

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public virtual TblPeriod? Period { get; set; }
}
