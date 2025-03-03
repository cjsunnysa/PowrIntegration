using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowrIntegration.Shared.Dtos;

public sealed record PluItemDto
{
    public sealed class DateTimePowertillConverter : JsonConverter<DateTime>
    {
        private readonly string _format = @"dd/MM/yyyy HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            return 
                value == "30/12/1899" 
                ? DateTime.MinValue 
                : DateTime.ParseExact(value ?? "", _format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    public required long PluNumber { get; init; }
    public string? PluDescription { get; init; }
    public string? SizeDescription { get; init; }
    public decimal SellingPrice1 { get; init; }
    public int SalesGroup { get; init; }
    public string Flags { get; init; } = string.Empty;
    public bool IsFlag48 => Flags.Length > 0 && Flags[0] == 'Y';
    public bool IsFlag47 => Flags.Length > 1 && Flags[1] == 'Y';
    public bool IsFlag46 => Flags.Length > 2 && Flags[2] == 'Y';
    public bool IsFlag45 => Flags.Length > 3 && Flags[3] == 'Y';
    public bool IsFlag44 => Flags.Length > 4 && Flags[4] == 'Y';
    public bool IsFlag43 => Flags.Length > 5 && Flags[5] == 'Y';
    public bool IsFlag42 => Flags.Length > 6 && Flags[6] == 'Y';
    public bool IsBottleContentPlu => Flags.Length > 7 && Flags[7] == 'Y';
    public bool IsFridgeStockPlu => Flags.Length > 8 && Flags[8] == 'Y';
    public bool IsScaleComparisonPlu => Flags.Length > 9 && Flags[9] == 'Y';
    public bool IsThirdsPizzaPlu => Flags.Length > 10 && Flags[10] == 'Y';
    public bool IsPriceComparePlu => Flags.Length > 11 && Flags[11] == 'Y';
    public bool IsPrepSteersMainPlu => Flags.Length > 12 && Flags[12] == 'Y';
    public bool IsTrafficType3Plu => Flags.Length > 13 && Flags[13] == 'Y';
    public bool IsTrafficType2Plu => Flags.Length > 14 && Flags[14] == 'Y';
    public bool IsTrafficType1Plu => Flags.Length > 15 && Flags[15] == 'Y';
    public bool IsSelectedIngredientPlu => Flags.Length > 16 && Flags[16] == 'Y';
    public bool IsCheckStockOnHand => Flags.Length > 17 && Flags[17] == 'Y';
    public bool IsQuarterPizzaPlu => Flags.Length > 18 && Flags[18] == 'Y';
    public bool IsNoAddToSlsHistory => Flags.Length > 19 && Flags[19] == 'Y';
    public bool IsCyoPizzaPlu => Flags.Length > 20 && Flags[20] == 'Y';
    public bool IsHalfNHalfPizzaPlu => Flags.Length > 21 && Flags[21] == 'Y';
    public bool IsPrePaidVoucherPlu => Flags.Length > 22 && Flags[22] == 'Y';
    public bool IsReferenceNoComp => Flags.Length > 23 && Flags[23] == 'Y';
    public bool IsBottleContainerPlu => Flags.Length > 24 && Flags[24] == 'Y';
    public bool IsStockCountingPlu => Flags.Length > 25 && Flags[25] == 'Y';
    public bool IsKPrinterMessagePlu => Flags.Length > 26 && Flags[26] == 'Y';
    public bool IsMealAllowancePlu => Flags.Length > 27 && Flags[27] == 'Y';
    public bool IsPreparedItem => Flags.Length > 28 && Flags[28] == 'Y';
    public bool IsToppingPlu => Flags.Length > 29 && Flags[29] == 'Y';
    public bool IsPizzaPlu => Flags.Length > 30 && Flags[30] == 'Y';
    public bool IsAddToQuarterAndHalfHrRpt => Flags.Length > 31 && Flags[31] == 'Y';
    public bool IsPromptForItemCount => Flags.Length > 32 && Flags[32] == 'Y';
    public bool IsScalePresetItem => Flags.Length > 33 && Flags[33] == 'Y';
    public bool IsOutOfStock => Flags.Length > 34 && Flags[34] == 'Y';
    public bool IsOnPromotion => Flags.Length > 35 && Flags[35] == 'Y';
    public bool IsImeiNoCompulsed => Flags.Length > 36 && Flags[36] == 'Y';
    public bool IsSerialNoCompulsed => Flags.Length > 37 && Flags[37] == 'Y';
    public bool IsSpecialDealPlu => Flags.Length > 38 && Flags[38] == 'Y';
    public bool IsChainedPlu => Flags.Length > 39 && Flags[39] == 'Y';
    public bool IsScalePlu => Flags.Length > 40 && Flags[40] == 'Y';
    public bool IsFinishedProduct => Flags.Length > 41 && Flags[41] == 'Y';
    public bool IsStockOnlyPlu => Flags.Length > 42 && Flags[42] == 'Y';
    public bool IsOpenPricePlu => Flags.Length > 43 && Flags[43] == 'Y';
    public bool IsOpenDescriptionPlu => Flags.Length > 44 && Flags[44] == 'Y';
    public bool IsPreparationPlu => Flags.Length > 45 && Flags[45] == 'Y';
    public bool IsCuttingSchedulePlu => Flags.Length > 46 && Flags[46] == 'Y';
    public bool IsPromptForPrice => Flags.Length > 47 && Flags[47] == 'Y';
    public string? Supplier1StockCode { get; init; }
    public string? Supplier2StockCode { get; init; }
    public DateTime DateTimeCreated { get; init; }
    public DateTime DateTimeEdited { get; init; }
}