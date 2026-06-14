using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
[RequireGodotRuntime]
public class ThinkNodeSleepTest
{
    [TestCase]
    public void ReturnsSleepJobWhenTired()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 0.2f;
        var job = new ThinkNode_Sleep(urgent: false).TryGiveJob(guy);
        AssertObject(job).IsNotNull();
        AssertBool(job.Type == JobType.Sleep).IsTrue();
    }

    [TestCase]
    public void ReturnsNullWhenRested()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 0.5f;
        AssertObject(new ThinkNode_Sleep(urgent: false).TryGiveJob(guy)).IsNull();
    }

    [TestCase]
    public void ThresholdIsExclusive()
    {
        var guy = new Guy();
        guy.Needs.Rest.Level = 0.3f;
        AssertObject(new ThinkNode_Sleep(urgent: false).TryGiveJob(guy)).IsNull();
    }
}