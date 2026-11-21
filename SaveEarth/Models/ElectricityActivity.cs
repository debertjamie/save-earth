namespace SaveEarth.Models;

public class ElectricityActivity : EmissionActivity
{
    // public: override property ActivityType dari base class
    public override string ActivityType => "electricity";

    // protected: override property EmissionFactor dari base class
    protected override float EmissionFactor => 0.0005f;

    // public: constructor dapat dipanggil dari luar class
    public ElectricityActivity(float value) : base(value)
    {
    }

    // public: override method Describe dari base class
    public override string Describe() => $"Used {Value:0.##} Wh of electricity";
}
