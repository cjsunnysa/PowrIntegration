using PowrIntegration.ZraService.Zra;

namespace PowrIntegration.ZraService.Zra.GetPurchases;

public sealed record GetPurchasesResponse : ZraResponse
{
    public sealed record DataRecord
    {
        public sealed record Purchase
        {
            public sealed record PurchaseItem
            {
                public required int itemSeq { get; init; }
                public required string itemCd { get; init; }
                public required string itemClsCd { get; init; }
                public required string itemNm { get; init; }
                public string? bcd { get; init; }
                public string? pkgUnitCd { get; init; }
                public decimal? pkg { get; init; }
                public required string qtyUnitCd { get; init; }
                public required decimal qty { get; init; }
                public required decimal prc { get; init; }
                public required decimal splyAmt { get; init; }
                public required decimal dcRt { get; init; }
                public required decimal dcAmt { get; init; }
                public required string vatCatCd { get; init; }
                public required string iplCatCd { get; init; }
                public required string tlCatCd { get; init; }
                public required string exciseTxCatC { get; init; }
                public required decimal vatTaxblAmt { get; init; }
                public required decimal exciseTaxblAmt { get; init; }
                public required decimal iplTaxblAmt { get; init; }
                public required decimal tlTaxblAmt { get; init; }
                public required decimal taxblAmt { get; init; }
                public required decimal vatAmt { get; init; }
                public required decimal iplAmt { get; init; }
                public required decimal tlAmt { get; init; }
                public required decimal exciseTxAmt { get; init; }
                public required decimal totAmt { get; init; }
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

        public Purchase[] saleList { get; init; } = [];
    }

    public required DataRecord data { get; init; }
}
