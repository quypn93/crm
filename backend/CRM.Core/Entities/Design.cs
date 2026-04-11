namespace CRM.Core.Entities;

public class Design : BaseEntity
{
    public string DesignName { get; set; } = string.Empty;
    public string? DesignData { get; set; }              // Canvas JSON data
    public string? SelectedComponents { get; set; }      // JSON array of component IDs
    public string? Designer { get; set; }
    public string? CustomerFullName { get; set; }

    // Size quantities (JSON format: {"S": 5, "M": 10, "L": 8})
    public int? Total { get; set; }
    public string? SizeMan { get; set; }
    public string? SizeWomen { get; set; }
    public string? SizeKid { get; set; }
    public string? Oversized { get; set; }

    // Template / phiếu sản xuất
    public string? FrontImageUrl { get; set; }        // ảnh mặt trước
    public string? BackImageUrl { get; set; }         // ảnh mặt sau
    public string? SizeQuantities { get; set; }       // JSON: {"S":0,"M":5,"L":10,"XL":8,"XXL":3,"NC1":1,"NC2":5,"NC3":0}
    public string? PersonNamesBySize { get; set; }    // JSON: {"L":["Nguyễn A","Trần B"],"M":["Lê C"]}
    public string? MaterialText { get; set; }         // CHẤT LIỆU
    public string? ColorText { get; set; }            // MÀU SẮC
    public string? StyleText { get; set; }            // KIỂU DÁNG
    public DateTime? ReturnDate { get; set; }         // NGÀY TRẢ
    public string? GiftItems { get; set; }            // JSON: [{"imageUrl":"...","description":"Cờ: 1M x 1,5M"}]

    // Production info
    public DateTime? FinishedDate { get; set; }
    public string? NoteConfection { get; set; }
    public string? NoteOldCodeOrder { get; set; }
    public string? NoteAttachTagLabel { get; set; }
    public string? NoteOther { get; set; }
    public string? SaleStaff { get; set; }

    // Foreign keys
    public Guid? ColorFabricId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Navigation properties
    public virtual ColorFabric? ColorFabric { get; set; }
    public virtual Order? Order { get; set; }
    public virtual User? CreatedByUser { get; set; }
}
