using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using AssetGazeV2;

[TestFixture]
public class BondTests
{
    // Test Data
    private const string ValidISIN = "XS1234567890";
    private const string ValidPriceSource = "Bloomberg";
    private const string ValidPriceSourceCode = "BGN";

    [Test]
    public void Bond_Constructor_WithValidArguments_CreatesInstance()
    {
        // Arrange & Act
        var bond = new Bond(ValidISIN, ValidPriceSource, ValidPriceSourceCode);

        // Assert (using Assert.That and fluent API)
        Assert.That(bond, Is.Not.Null);
        Assert.That(bond.ISIN, Is.EqualTo(ValidISIN));
        Assert.That(bond.PriceSource, Is.EqualTo(ValidPriceSource));
        Assert.That(bond.PriceSourceCode, Is.EqualTo(ValidPriceSourceCode));
    }

    
    [Test]
    [TestCase("", ValidPriceSource, ValidPriceSourceCode, "isin")]
    [TestCase("   ", ValidPriceSource, ValidPriceSourceCode, "isin")]
    [TestCase(ValidISIN, "", ValidPriceSourceCode, "priceSource")]
    [TestCase(ValidISIN, "   ", ValidPriceSourceCode, "priceSource")]
    [TestCase(ValidISIN, ValidPriceSource, "", "priceSourceCode")]
    [TestCase(ValidISIN, ValidPriceSource, "   ", "priceSourceCode")]
    public void Bond_Constructor_WithInvalidMandatoryArguments_ThrowsArgumentException(
        string isin, string priceSource, string priceSourceCode, string expectedParamName)
    {
        var caughtException = Assert.Catch<ArgumentException>(() =>
            new Bond(isin, priceSource, priceSourceCode));
        
        Assert.That(caughtException, Is.Not.Null);
        Assert.That(caughtException.ParamName, Is.EqualTo(expectedParamName));
    }

    [Test]
    public void Bond_FtLinkType_ReturnsCorrectValue()
    {
        // Arrange
        var bond = new Bond(ValidISIN, ValidPriceSource, ValidPriceSourceCode);

        // Act
        string ftLinkType = bond.FtLinkType;

        // Assert
        Assert.That(ftLinkType, Is.EqualTo("bond"));
    }

    [Test]
    public void Bond_FtLinkType_IsReadOnly()
    {
        // Arrange
        var bond = new Bond(ValidISIN, ValidPriceSource, ValidPriceSourceCode);
        
        // Assert that the value remains consistent after creation (direct assignment is a compile-time error)
        Assert.That(bond.FtLinkType, Is.EqualTo("bond"));
    }

    [Test]
    public void Bond_NullableProperties_CanBeSetAndRetrieved()
    {
        // Arrange
        var bond = new Bond(ValidISIN, ValidPriceSource, ValidPriceSourceCode);
        string expectedName = "Test Bond Name";
        string expectedSedol = "B0Y3M7";
        string expectedIncomeTreatment = "Accrued Interest";
        decimal expectedTotalExpenseRatio = 0.005M;
        string expectedDenomination = "USD";

        // Act
        bond.Name = expectedName;
        bond.Sedol = expectedSedol;
        bond.IncomeTreatment = expectedIncomeTreatment;
        bond.TotalExpenseRatio = expectedTotalExpenseRatio;
        bond.Denomination = expectedDenomination;

        // Assert
        Assert.That(bond.Name, Is.EqualTo(expectedName));
        Assert.That(bond.Sedol, Is.EqualTo(expectedSedol));
        Assert.That(bond.IncomeTreatment, Is.EqualTo(expectedIncomeTreatment));
        Assert.That(bond.TotalExpenseRatio, Is.EqualTo(expectedTotalExpenseRatio));
        Assert.That(bond.Denomination, Is.EqualTo(expectedDenomination));
    }

    [Test]
    public void Bond_NullableProperties_CanBeNull()
    {
        // Arrange
        var bond = new Bond(ValidISIN, ValidPriceSource, ValidPriceSourceCode);

        // Act
        bond.Name = null;
        bond.Sedol = null;
        bond.IncomeTreatment = null;
        bond.TotalExpenseRatio = null;
        bond.Denomination = null;

        // Assert
        Assert.That(bond.Name, Is.Null);
        Assert.That(bond.Sedol, Is.Null);
        Assert.That(bond.IncomeTreatment, Is.Null);
        Assert.That(bond.TotalExpenseRatio, Is.Null);
        Assert.That(bond.Denomination, Is.Null);
    }
}