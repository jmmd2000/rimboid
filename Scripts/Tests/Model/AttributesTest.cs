using System;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class AttributesTest
{
    static readonly AttributeDef Def = new() { DefName = "TestAttribute", Label = "test attribute" };
    static readonly SkillDef Linked = new() { DefName = "TestSkill", Label = "test skill", Attribute = Def };

    [TestCase]
    public void StartsAtBaselineWithFactorOne()
    {
        var attributes = new Attributes();
        AssertInt(attributes.Get(Def).Level).IsEqual(Attributes.Baseline);
        AssertFloat(attributes.Factor(Def)).IsEqual(1f);
    }

    [TestCase]
    public void FactorRisesAboveBaseline()
    {
        var attributes = new Attributes();
        attributes.Get(Def).Level = Attributes.Baseline + 3;
        AssertFloat(attributes.Factor(Def)).IsEqual(1f + 3 * Attributes.FactorPerLevel);
    }

    [TestCase]
    public void FactorFallsBelowBaseline()
    {
        var attributes = new Attributes();
        attributes.Get(Def).Level = Attributes.Baseline - 2;
        AssertFloat(attributes.Factor(Def)).IsEqual(1f - 2 * Attributes.FactorPerLevel);
    }

    [TestCase]
    public void FactorForNullDefIsOne()
    {
        AssertFloat(new Attributes().Factor(null)).IsEqual(1f);
    }

    [TestCase]
    public void GainBelowTheCurveDoesNotLevel()
    {
        var attributes = new Attributes();
        attributes.Gain(Def, 100f);
        AssertInt(attributes.Get(Def).Level).IsEqual(Attributes.Baseline);
        AssertFloat(attributes.Get(Def).XP).IsEqual(100f);
    }

    [TestCase]
    public void LevellingUpCarriesTheRemainder()
    {
        var attributes = new Attributes();
        // exactly one levels worth of XP from the baseline, plus a 100 remainder
        attributes.Gain(Def, Attributes.XPToLevel(Attributes.Baseline) + 100f);
        AssertInt(attributes.Get(Def).Level).IsEqual(Attributes.Baseline + 1);
        AssertFloat(attributes.Get(Def).XP).IsEqual(100f);
    }

    [TestCase]
    public void LevelIsCappedAtMaxLevel()
    {
        var attributes = new Attributes();
        attributes.Gain(Def, 10_000_000f);
        AssertInt(attributes.Get(Def).Level).IsEqual(Attributes.MaxLevel);
        AssertFloat(attributes.Get(Def).XP).IsLessEqual(Attributes.XPToLevel(Attributes.MaxLevel));
    }

    [TestCase]
    public void GainWithNullDefIsANoOp()
    {
        var attributes = new Attributes();
        attributes.Gain(null, 500f);
        AssertFloat(attributes.Get(Def).XP).IsEqual(0f);
    }

    [TestCase]
    public void RollCentresOnBaselineWithTails()
    {
        var attributes = new Attributes();
        attributes.Get(Def); // seed one record to roll (defs aren't loaded in this suite)
        var rng = new Random(42);

        int min = int.MaxValue, max = int.MinValue;
        long sum = 0;
        const int samples = 2000;
        for (int i = 0; i < samples; i++)
        {
            attributes.Roll(rng);
            int level = attributes.Get(Def).Level;
            sum += level;
            min = Math.Min(min, level);
            max = Math.Max(max, level);
        }

        AssertFloat((float)sum / samples).IsBetween(Attributes.Baseline - 1f, Attributes.Baseline + 1f); // clusters on centre
        AssertInt(min).IsLess(Attributes.Baseline); // weak rolls happen
        AssertInt(max).IsGreater(Attributes.Baseline); // gifted rolls happen
        AssertInt(min).IsGreaterEqual(0);   // never off the scale
        AssertInt(max).IsLessEqual(Attributes.MaxLevel);
    }

    [TestCase]
    public void WorkRateIsOneAtBaselineAndSkillZero()
    {
        AssertFloat(new Guy().WorkRate(Linked)).IsEqual(1f);
    }

    [TestCase]
    public void WorkRateBlendsTheLinkedAttribute()
    {
        var guy = new Guy();
        guy.Attributes.Get(Def).Level = Attributes.Baseline + 4;
        AssertFloat(guy.WorkRate(Linked)).IsEqual(1f + 4 * Attributes.FactorPerLevel);
    }

    [TestCase]
    public void WorkRateMultipliesSkillAndAttribute()
    {
        var guy = new Guy();
        guy.Skills.Gain(Linked, 100f);                          // skill level 1
        guy.Attributes.Get(Def).Level = Attributes.Baseline + 4; // attribute factor above baseline
        AssertFloat(guy.WorkRate(Linked))
            .IsEqual((1f + Skills.WorkRatePerLevel) * (1f + 4 * Attributes.FactorPerLevel));
    }
}