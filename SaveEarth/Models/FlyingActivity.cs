namespace SaveEarth.Models;

public class FlyingActivity : EmissionActivity
{
    // public: override property ActivityType dari base class
    public override string ActivityType => "flying";

    // protected: override property EmissionFactor dari base class
    protected override float EmissionFactor => 0.254f;

    // public: constructor dapat dipanggil dari luar class
    public FlyingActivity(float value) : base(value)
    {
    }

    // public: override method Describe dari base class
    public override string Describe() => $"Flew {Value:0.##} km";
}
