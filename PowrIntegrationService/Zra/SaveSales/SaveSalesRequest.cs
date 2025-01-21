namespace PowrIntegrationService.Zra.SaveSales;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
public class SaveSalesRequest
{
    public string tpin { get; init; } // Taxpayer Identification Number (VARCHAR, Required, Length: 10)
    public string bhfId { get; init; } // Branch location identifier (VARCHAR, Required, Length: 3)
    public long? orgInvcNo { get; init; } // Original invoice number for reversals/debit notes (NUMBER, Optional, Length: 38)
    public string cisInvcNo { get; init; } // Invoice number (VARCHAR, Required, Length: 50)
    public string custTpin { get; init; } // Customer's TPIN (VARCHAR, Optional, Length: 10)
    public string custNm { get; init; } // Customer name (VARCHAR, Optional, Length: 60)
    public string slsTyCd { get; init; } // Type of sale (VARCHAR, Required, Length: 5)
    public string rcptTyCd { get; init; } // Type of receipt (VARCHAR, Required, Length: 5)
    public string pymtTyCd { get; init; } // Mode of payment (VARCHAR, Required, Length: 5)
    public string slsSttsCd { get; init; } // Sale transaction status (VARCHAR, Required, Length: 5)
    public DateTime cnfmDt { get; init; } // Invoice issue date/time (VARCHAR, Required, Format: yyyyMMddhhmmss)
    public DateTime slsDt { get; init; } // Sales date (VARCHAR, Required, Format: yyyyMMdd)
    public DateTime? stockRlssDt { get; init; } // Stock released date (VARCHAR, Optional, Format: yyyyMMddhhmmss)
    public int totItemCnt { get; init; } // Total items on the invoice (NUMBER, Required, Length: 10)
    public decimal txblAmtA { get; init; } // Total standard-rated VAT taxable amount (NUMBER, Required, Length: 18,4)

    // List of items for detailed sales
    public List<SaveSalesItem> itemList { get; init; } = new();
}

public class SaveSalesItem
{
    public int itemSeq { get; init; } // Sequence number for items
    public string itemCd { get; init; } // Code identifying the item (VARCHAR, Length: 20)
    public string itemClsCd { get; init; } // Classification code for the item (VARCHAR, Length: 10)
    public string itemNm { get; init; } // Item name (VARCHAR, Length: 200)
    public decimal qty { get; init; } // Quantity of the item (NUMBER, Length: 13,2)
    public decimal prc { get; init; } // Price per unit (NUMBER, Length: 18,2)
    public decimal splyAmt { get; init; } // Total supply amount for the item (NUMBER, Length: 18,4)
    public decimal vatTxblAmt { get; init; } // VAT taxable amount (NUMBER, Length: 18,4)
    public decimal vatAmt { get; init; } // VAT amount (NUMBER, Length: 18,2)
    public decimal totAmt { get; init; } // Total amount including VAT (NUMBER, Length: 18,2)
}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
