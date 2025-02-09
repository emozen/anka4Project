using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblCustomer
{
    public int Id { get; set; }

    public string? CustomerName { get; set; }

    public DateTime? DeletedTime { get; set; }

    public virtual ICollection<TblProject> TblProjects { get; set; } = new List<TblProject>();
}
