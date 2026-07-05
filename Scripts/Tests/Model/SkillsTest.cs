using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class SkillsTest
{
    static readonly SkillDef Def = new() { DefName = "TestSkill", Label = "test skill" };

    [TestCase]
    public void StartsAtLevelZeroWithNoXP()
    {
        var skill = new Skills().Get(Def);
        AssertInt(skill.Level).IsEqual(0);
        AssertFloat(skill.XP).IsEqual(0f);
    }

    [TestCase]
    public void XPBelowTheCurveDoesNotLevel()
    {
        var skills = new Skills();
        skills.Gain(Def, 50f);
        var skill = skills.Get(Def);
        AssertInt(skill.Level).IsEqual(0);
        AssertFloat(skill.XP).IsEqual(50f);
    }

    [TestCase]
    public void LevellingUpCarriesTheRemainder()
    {
        var skills = new Skills();
        skills.Gain(Def, 120f);
        var skill = skills.Get(Def);
        AssertInt(skill.Level).IsEqual(1);
        AssertFloat(skill.XP).IsEqual(20f);
    }

    [TestCase]
    public void OneLumpCanClimbSeveralLevels()
    {
        var skills = new Skills();
        skills.Gain(Def, 350f);
        var skill = skills.Get(Def);
        AssertInt(skill.Level).IsEqual(2);
        AssertFloat(skill.XP).IsEqual(50f);
    }

    [TestCase]
    public void LevelIsCappedAtMaxLevel()
    {
        var skills = new Skills();
        skills.Gain(Def, 1_000_000f);
        AssertInt(skills.Get(Def).Level).IsEqual(Skills.MaxLevel);
        AssertFloat(skills.Get(Def).XP).IsLessEqual(Skills.XPToLevel(Skills.MaxLevel));
    }

    [TestCase]
    public void GainWithNullSkillIsANoOp()
    {
        var skills = new Skills();
        skills.Gain(null, 100f);
        skills.Gain(Def, 50f);
        AssertFloat(skills.Get(Def).XP).IsEqual(50f);
    }

    [TestCase]
    public void WorkRateIsOneAtLevelZero()
    {
        AssertFloat(new Guy().WorkRate(Def)).IsEqual(1f);
    }

    [TestCase]
    public void WorkRateRisesPerLevel()
    {
        var guy = new Guy();
        guy.Skills.Gain(Def, 100f);
        AssertFloat(guy.WorkRate(Def)).IsEqual(1f + Skills.WorkRatePerLevel);
    }

    [TestCase]
    public void WorkRateForNullSkillIsOne()
    {
        AssertFloat(new Guy().WorkRate(null)).IsEqual(1f);
    }
}