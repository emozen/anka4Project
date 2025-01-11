using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblPeriod
{
    public int Id { get; set; }

    public int? Year { get; set; }

    public int? Month { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<TblPeriodItem> TblPeriodItems { get; set; } = new List<TblPeriodItem>();
}
