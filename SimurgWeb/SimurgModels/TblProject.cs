using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblProject
{
    public int Id { get; set; }

    public string ProjectName { get; set; } = null!;

    public int CreatedUserId { get; set; }

    public DateTime CreatedTime { get; set; }

    public bool IsActive { get; set; }

    public string? Explanation { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? StartDatetime { get; set; }

    public DateTime? EndDatetime { get; set; }

    public int? CustomerId { get; set; }

    public virtual TblUser CreatedUser { get; set; } = null!;

    public virtual TblCustomer? Customer { get; set; }

    public virtual ICollection<TblItem> TblItems { get; set; } = new List<TblItem>();

    public virtual ICollection<TblProjectAuthorize> TblProjectAuthorizes { get; set; } = new List<TblProjectAuthorize>();
}
