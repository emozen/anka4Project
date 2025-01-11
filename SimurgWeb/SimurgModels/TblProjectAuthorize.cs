using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblProjectAuthorize
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public int UserId { get; set; }

    public virtual TblProject Project { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
