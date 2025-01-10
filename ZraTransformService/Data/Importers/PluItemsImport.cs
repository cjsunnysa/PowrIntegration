using CsvHelper.Configuration;
using EFCore.BulkExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PowrIntegration.Data.Entities;
using PowrIntegration.Extensions;
using PowrIntegration.Options;
using PowrIntegration.Powertill;
using System.Collections.Immutable;

namespace PowrIntegration.Data.Importers;

public sealed class PluItemsImport(IOptions<PowertillOptions> options, IDbContextFactory<PowrIntegrationDbContext> dbContextFactory, ILogger<PluItemsImport> logger)
    : FileImporter<PluItem>(options, "PluCreat.csa", logger)
{
    private readonly IDbContextFactory<PowrIntegrationDbContext> _dbContextFactory = dbContextFactory;

    protected async override Task<Result<ImmutableArray<PluItem>>> ExecuteImport(CancellationToken cancellationToken)
    {
        try
        {
            var pluItemMap = new PluItemMap();

            var csaFile = new PowertillCsaFile<PluItem>(FilePath, null, pluItemMap);

            var pluItems = csaFile.ReadRecords().ToImmutableArray();

            var outboxItems = pluItems.MapToOutboxItems();

            using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            await dbContext.BulkInsertOrUpdateAsync(pluItems, cancellationToken: cancellationToken);

            await dbContext.BulkInsertOrUpdateAsync(outboxItems, cancellationToken: cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(pluItems);
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError($"An error occured importing PLU items from {FilePath}.", ex));
        }
    }
}

public class PluItemMap : ClassMap<PluItem>
{
    public PluItemMap()
    {
        Map(m => m.PluNumber).Index(0);
        Map(m => m.PluDescription).Index(1);
        Map(m => m.SizeDescription).Index(2);
        Map(m => m.SellingPrice1).Index(3);
        Map(m => m.SellingPrice2).Index(4);
        Map(m => m.SellingPrice3).Index(5);
        Map(m => m.SellingPrice4).Index(6);
        Map(m => m.SellingPrice5).Index(7);
        Map(m => m.SellingPrice6).Index(8);
        Map(m => m.SellingPrice7).Index(9);
        Map(m => m.SellingPrice8).Index(10);
        Map(m => m.SellingPrice9).Index(11);
        Map(m => m.GrossCost).Index(12);
        Map(m => m.NettCost).Index(13);
        Map(m => m.TargetMargin1).Index(14);
        Map(m => m.TargetMargin2).Index(15);
        Map(m => m.TargetMargin3).Index(16);
        Map(m => m.TargetMargin4).Index(17);
        Map(m => m.TargetMargin5).Index(18);
        Map(m => m.TargetMargin6).Index(19);
        Map(m => m.TargetMargin7).Index(20);
        Map(m => m.TargetMargin8).Index(21);
        Map(m => m.TargetMargin9).Index(22);
        Map(m => m.SalesGroup).Index(23);
        Map(m => m.AccessLevel).Index(24);
        Map(m => m.Flags).Index(25);
        Map(m => m.MixAndMatchGroup).Index(26);
        Map(m => m.DiscountMatrixGroup).Index(27);
        Map(m => m.KpFlags).Index(28);
        Map(m => m.KpPriorityLevel).Index(29);
        Map(m => m.KitchenPrinterGroup).Index(30);
        Map(m => m.PieceCount).Index(31);
        Map(m => m.SoftKeyboard1).Index(32);
        Map(m => m.SoftKeyboard2).Index(33);
        Map(m => m.SoftKeyboard3).Index(34);
        Map(m => m.SoftKeyboard4).Index(35);
        Map(m => m.SoftKeyboard5).Index(36);
        Map(m => m.SoftKeyboard6).Index(37);
        Map(m => m.SoftKeyboard7).Index(38);
        Map(m => m.SoftKeyboard8).Index(39);
        Map(m => m.SoftKeyboard9).Index(40);
        Map(m => m.SoftKeyboard10).Index(41);
        Map(m => m.BillPrintGroup).Index(42);
        Map(m => m.LinkedPlu).Index(43);
        Map(m => m.LoyaltyPoints).Index(44);
        Map(m => m.ExpiryPeriod).Index(45);
        Map(m => m.DealGroup).Index(46);
        Map(m => m.InternalPluFlags).Index(47);
        Map(m => m.LoyaltyPointsPrice1).Index(48);
        Map(m => m.LoyaltyPointsPrice2).Index(49);
        Map(m => m.LoyaltyPointsPrice3).Index(50);
        Map(m => m.LoyaltyPointsPrice4).Index(51);
        Map(m => m.LoyaltyPointsPrice5).Index(52);
        Map(m => m.LoyaltyPointsPrice6).Index(53);
        Map(m => m.LoyaltyPointsPrice7).Index(54);
        Map(m => m.LoyaltyPointsPrice8).Index(55);
        Map(m => m.LoyaltyPointsPrice9).Index(56);
        Map(m => m.CouponNumber).Index(57);
        Map(m => m.DealMaximumPrice).Index(58);
        Map(m => m.DealPremiumPrice).Index(59);
        Map(m => m.ToppingKeyboardNo).Index(60);
        Map(m => m.ToppingGroupNo).Index(61);
        Map(m => m.LastPurchaseDate).Index(62).TypeConverter<DelphiDateConverter>();
        Map(m => m.FreeToppingsQty).Index(63);
        Map(m => m.CateringItemType).Index(64);
        Map(m => m.CateringSize).Index(65);
        Map(m => m.ClassificationGroup1).Index(66);
        Map(m => m.ClassificationGroup2).Index(67);
        Map(m => m.ClassificationGroup3).Index(68);
        Map(m => m.ClassificationGroup4).Index(69);
        Map(m => m.ClassificationGroup5).Index(70);
        Map(m => m.ClassificationGroup6).Index(71);
        Map(m => m.ClassificationGroup7).Index(72);
        Map(m => m.ClassificationGroup8).Index(73);
        Map(m => m.ClassificationGroup9).Index(74);
        Map(m => m.RedemptionPoints).Index(75);
        Map(m => m.VariantGroup).Index(76);
        Map(m => m.ToppingPremiumPrice).Index(77);
        Map(m => m.UseByPeriod).Index(78);
        Map(m => m.CostMaxVariancePercent).Index(79);
        Map(m => m.ContainerContentSize).Index(80);
        Map(m => m.ReferredPluNo).Index(81);
        Map(m => m.ReferredQuantity).Index(82);
        Map(m => m.ReOrderLevel).Index(83);
        Map(m => m.ReOrderQuantity).Index(84);
        Map(m => m.BinLocationNo).Index(85);
        Map(m => m.StocktakingGroup).Index(86);
        Map(m => m.Supplier1AccountNo).Index(87);
        Map(m => m.Supplier2AccountNo).Index(88);
        Map(m => m.Supplier3AccountNo).Index(89);
        Map(m => m.Supplier1StockCode).Index(90);
        Map(m => m.Supplier2StockCode).Index(91);
        Map(m => m.Supplier3StockCode).Index(92);
        Map(m => m.BottleEmptyWeight).Index(93);
        Map(m => m.BottleFullWeight).Index(94);
        Map(m => m.BottleTotCount).Index(95);
        Map(m => m.Supplier1LeadTime).Index(96);
        Map(m => m.Supplier2LeadTime).Index(97);
        Map(m => m.Supplier3LeadTime).Index(98);
        Map(m => m.QuotedPriceSupplier1).Index(99);
        Map(m => m.QuotedPriceSupplier2).Index(100);
        Map(m => m.QuotedPriceSupplier3).Index(101);
        Map(m => m.DateQuotedSupplier1).Index(102).TypeConverter<PowertillDateConverter>();
        Map(m => m.DateQuotedSupplier2).Index(103).TypeConverter<PowertillDateConverter>();
        Map(m => m.DateQuotedSupplier3).Index(104).TypeConverter<PowertillDateConverter>();
        Map(m => m.CheckPriceSupplierFlags).Index(105);
        Map(m => m.StockPurchaseGroup).Index(106);
        Map(m => m.PurchasePriceVariancePercent).Index(107);
        Map(m => m.WebItemDescription).Index(108);
        Map(m => m.BoxQuantity).Index(109);
        Map(m => m.PalletQuantity).Index(110);
        Map(m => m.DateTimeCreated).Index(111).TypeConverter<Iso8601DateConverter>();
        Map(m => m.DateTimeEdited).Index(112).TypeConverter<Iso8601DateConverter>();
        Map(m => m.ClerkEdited).Index(113);
        Map(m => m.Version).Index(120);
    }
}
