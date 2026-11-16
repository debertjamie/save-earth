namespace SaveEarth.Models;

public abstract class EmissionActivity
{
    // public: dapat diakses dari mana saja (di luar class maupun assembly)
    public float Value { get; }

    // public: dapat diakses dari mana saja
    public abstract string ActivityType { get; }

    // protected: hanya dapat diakses di dalam class ini dan class turunannya
    protected abstract float EmissionFactor { get; }

    // protected: constructor hanya dapat dipanggil oleh class turunan
    protected EmissionActivity(float value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        Value = value;
    }

    // public: method dapat diakses dari mana saja
    public float CalculateEmission() => Value * EmissionFactor;

    // public: method abstract dapat diakses dari mana saja dan di-override oleh class turunan
    public abstract string Describe();
}
