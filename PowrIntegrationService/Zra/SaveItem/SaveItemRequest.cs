namespace PowrIntegrationService.Zra.SaveItem;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
public record SaveItemRequest
{
    public static class ProductTypes
    {
        public const string RawMaterial = "1";
        public const string FinishedProduct = "2";
        public const string Service = "3";
    }

    public static class NationalityCodes
    {
        public const string Zambia = "ZM";
    }

    public static class PackagingUnitCodes
    {
        public const string Each = "EA";
    }

    public static class QuantityUnitCodes
    {
        public const string Each = "EA";
    }

    public static class TaxTypeCode
    {
        public const string StandardRated = "A";
        public const string MinimumTaxableValue = "B";
        public const string Exports = "C1";
        public const string ZeroRatedGrantedExemption = "C2";
        public const string ZeroRatedByNature = "C3";
        public const string VatExempt = "D";
        public const string Disbursment = "E";
        public const string ReverseVat = "RVAT";
        public const string InsurancePremiumLevy = "IPL1";
        public const string ReInsurance = "IPL2";
        public const string TourismLevy = "TL";
        public const string ServiceCharge = "F";
        public const string ExciseOnCoal = "ECM";
        public const string ExciseElectricity = "EXEEG";
        public const string TurnoverTax = "TOT";
    }

    public string tpin { get; init; } // Taxpayer Identification Number (VARCHAR, Required, Length: 10)
    public string bhfId { get; init; } // Branch location identifier (VARCHAR, Required, Length: 3)
    public string itemCd { get; init; } // Item Code (VARCHAR, Required, Length: 100)
    public string itemClsCd { get; init; } // Item Classification Code (VARCHAR, Required, Length: 10)
    public string itemTyCd { get; init; } // Item Type Code (VARCHAR, Required, Length: 5)
    public string itemNm { get; init; } // Item Name (VARCHAR, Required, Length: 200)
    public string? itemStdNm { get; init; } // Item Standard Name (VARCHAR, Optional, Length: 200)
    public string orgnNatCd { get; init; } // Origin Place Code (VARCHAR, Required, Length: 5)
    public string pkgUnitCd { get; init; } // Packaging Unit Code (VARCHAR, Required, Length: 5)
    public string qtyUnitCd { get; init; } // Quantity Unit Code (VARCHAR, Required, Length: 5)
    public string? vatCatCd { get; init; } // VAT Category Code (VARCHAR, Optional, Length: 5)
    public string? iplCatCd { get; init; } // IPL Category Code (VARCHAR, Optional, Length: 5)
    public string? tlCatCd { get; init; } // TL Category Code (VARCHAR, Optional, Length: 5)
    public string? exciseTxCatCd { get; init; } // Excise Tax Category Code (VARCHAR, Optional, Length: 5)
    public string? btchNo { get; init; } // Batch Number (VARCHAR, Optional, Length: 10)
    public string? bcd { get; init; } // Barcode (VARCHAR, Optional, Length: 20)
    public decimal dftPrc { get; init; } // Default Unit Price (NUMBER, Required, Length: 18,4)
    public string? addInfo { get; init; } // Additional Information (VARCHAR, Optional, Length: 100)
    public decimal? sftyQty { get; init; } // Safety Quantity (NUMBER, Optional, Length: 13,2)
    public string? manufactuterTpin { get; init; } // Manufacturer TPIN for MTV product (VARCHAR, Optional, Length: 10)
    public string? manufacturerItemCd { get; init; } // Manufacturer Item Code for MTV product (VARCHAR, Optional, Length: 100)
    public decimal? rrp { get; init; } // Recommended Retail Price (NUMBER, Optional, Length: 18,4)
    public string svcChargeYn { get; init; } // Service Charge Flag (CHAR, Optional, Length: 1)
    public string rentalYn { get; init; } // Rental Flag (CHAR, Optional, Length: 1)
    public string useYn { get; init; } // Usage Status (VARCHAR, Required, Length: 1)
    public string regrNm { get; init; } // Registrant Name (VARCHAR, Required, Length: 60)
    public string regrId { get; init; } // Registrant ID (VARCHAR, Required, Length: 20)
    public string modrNm { get; init; } // Modifier Name (VARCHAR, Required, Length: 60)
    public string modrId { get; init; } // Modifier ID (VARCHAR, Required, Length: 20)
}
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
