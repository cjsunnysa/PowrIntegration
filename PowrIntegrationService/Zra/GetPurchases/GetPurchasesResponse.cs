namespace PowrIntegrationService.Zra.GetPurchases;

public sealed record GetPurchasesResponse : ZraResponse
{
    public sealed record DataRecord
    {
        public sealed record Purchase
        {
            public sealed record PurchaseItem
            {
                public required int ItemSeq { get; init; }
                public required string ItemCd { get; init; }
                public required string ItemClsCd { get; init; }
                public required string ItemNm { get; init; }
                public string? Bcd { get; init; }
                public string? PkgUnitCd { get; init; }
                public decimal? Pkg { get; init; }
                public required string QtyUnitCd { get; init; }
                public required decimal Qty { get; init; }
                public required decimal Prc { get; init; }
                public required decimal SplyAmt { get; init; }
                public required decimal DcRt { get; init; }
                public required decimal DcAmt { get; init; }
                public required string VatCatCd { get; init; }
                public required string IplCatCd { get; init; }
                public required string TlCatCd { get; init; }
                public required string ExciseTxCatC { get; init; }
                public required decimal VatTaxblAmt { get; init; }
                public required decimal ExciseTaxblAmt { get; init; }
                public required decimal IplTaxblAmt { get; init; }
                public required decimal TlTaxblAmt { get; init; }
                public required decimal TaxblAmt { get; init; }
                public required decimal VatAmt { get; init; }
                public required decimal IplAmt { get; init; }
                public required decimal TlAmt { get; init; }
                public required decimal ExciseTxAmt { get; init; }
                public required decimal TotAmt { get; init; }
            }

            public required string spplrTpin { get; init; } // VARCHAR(10)
            public required string spplrNm { get; init; } // VARCHAR(60)
            public string? spplrBhfId { get; init; } // VARCHAR(3), Nullable
            public required long spplrInvcNo { get; init; } // NUMBER(38)
            public required string rcptTyCd { get; init; } // VARCHAR(5)
            public required string pmtTyCd { get; init; } // VARCHAR(5)
            public required string cfmDt { get; init; } // VARCHAR(14), ISO 8601 datetime format
            public required string salesDt { get; init; } // VARCHAR(8), ISO 8601 date format (yyyyMMdd)
            public string? stockRlsDt { get; init; } // VARCHAR(14), Nullable, ISO 8601 datetime format
            public required int totItemCnt { get; init; } // NUMBER(10)
            public required decimal totTaxblAmt { get; init; } // NUMBER(18,4)
            public required decimal totTaxAmt { get; init; } // NUMBER(18,2)
            public required decimal totAmt { get; init; } // NUMBER(18,2)
            public string? remark { get; init; } // VARCHAR(400), Nullable
            public required PurchaseItem[] itemList { get; init; } // Nested List for Item Details
        }
        
        public Purchase[] SaleList { get; init; } = [];
    }

    public required DataRecord Data { get; init; }
}
