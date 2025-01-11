using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblUser
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<TblItem> TblItemCreatedUsers { get; set; } = new List<TblItem>();

    public virtual ICollection<TblItem> TblItemDeleteUsers { get; set; } = new List<TblItem>();

    public virtual ICollection<TblLog> TblLogs { get; set; } = new List<TblLog>();

    public virtual ICollection<TblProjectAuthorize> TblProjectAuthorizes { get; set; } = new List<TblProjectAuthorize>();

    public virtual ICollection<TblProject> TblProjects { get; set; } = new List<TblProject>();
}
