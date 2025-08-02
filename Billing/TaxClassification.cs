namespace Billing;

/// <summary>
/// Tax classification linking products/services to their appropriate tax codes
/// </summary>
public class TaxClassification
{
    public required Guid Id { get; set; }

    /// <summary>
    /// Reference to the item (Product or Service)
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// The tax code assigned to this item
    /// </summary>
    public String TaxCode { get; set; } = String.Empty;

    /// <summary>
    /// When this classification was assigned
    /// </summary>
    public DateOnly AssignedDate { get; set; }

    /// <summary>
    /// When this classification expires (if applicable)
    /// </summary>
    public DateOnly? ExpirationDate { get; set; }

    /// <summary>
    /// Who assigned this classification
    /// </summary>
    public String AssignedBy { get; set; } = String.Empty;

    /// <summary>
    /// Additional notes about why this classification was chosen
    /// </summary>
    public String Notes { get; set; } = String.Empty;

    /// <summary>
    /// Checks if this classification is valid for a specific date
    /// </summary>
    public bool IsValidOn(DateOnly date)
    {
        return date >= AssignedDate
            && (ExpirationDate == null || date <= ExpirationDate.Value);
    }
}