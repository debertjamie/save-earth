namespace SaveEarth.Models;

public class DrivingActivity : EmissionActivity
{
    // public: override property ActivityType dari base class
    public override string ActivityType => "driving";

    // protected: override property EmissionFactor dari base class
    protected override float EmissionFactor => 0.411f;

    // public: constructor dapat dipanggil dari luar class
    public DrivingActivity(float value) : base(value)
    {
    }

    // public: override method Describe dari base class
    public override string Describe() => $"Drove {Value:0.##} km";
}
