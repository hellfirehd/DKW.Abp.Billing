namespace Billing;

/// <summary>
/// Item classification linking products/services to their appropriate tax codes
/// </summary>
public class ItemClassification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the item (Product or Service)
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// The tax code assigned to this item
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;

    /// <summary>
    /// When this classification was assigned
    /// </summary>
    public DateOnly AssignedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// When this classification expires (if applicable)
    /// </summary>
    public DateOnly? ExpirationDate { get; set; }

    /// <summary>
    /// Who assigned this classification
    /// </summary>
    public string AssignedBy { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about why this classification was chosen
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Whether this classification is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Checks if this classification is valid for a specific date
    /// </summary>
    public bool IsValidOn(DateOnly date)
    {
        return IsActive 
            && date >= AssignedDate 
            && (ExpirationDate == null || date <= ExpirationDate.Value);
    }
}