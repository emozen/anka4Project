using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblLog
{
    public int Id { get; set; }

    public int RecordId { get; set; }

    public string PageName { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string Definition { get; set; } = null!;

    public int CreatedUserId { get; set; }

    public DateTime CreatedTime { get; set; }

    public virtual TblUser CreatedUser { get; set; } = null!;
}
