using System;
using System.Collections.Generic;

namespace SimurgWeb.SimurgModels;

public partial class TblItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string? Definition { get; set; }

    public decimal Price { get; set; }

    public int CreatedUserId { get; set; }

    public DateTime CreatedTime { get; set; }

    public bool IsExpenses { get; set; }

    public byte[]? InvoiceFile { get; set; }

    public bool IsActive { get; set; }

    public int? DeleteUserId { get; set; }

    public virtual TblUser CreatedUser { get; set; } = null!;

    public virtual TblUser? DeleteUser { get; set; }

    public virtual TblProject Project { get; set; } = null!;
}
