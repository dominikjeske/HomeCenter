using System;
using CSharpFunctionalExtensions;

namespace Wirehome.Motion.Model
{
    public class Probability : ValueObject<Probability>
    {
        public static readonly Probability Zero = new Probability(0.0);
        public static readonly Probability Full = new Probability(1.0);

        public double Value { get; }
        public bool IsNoProbability => Equals(Zero);
        public bool IsFullProbability => Equals(Full);

        public Probability(double probability)
        {
            if (probability < 0) probability = 0.0;
            else if (probability > 1) probability = 1.0;

            Value = probability;
        }

        public override string ToString() => $"{Value}";
        protected override bool EqualsCore(Probability other) => Math.Abs(other.Value - Value) < 0.01;
        
        public Probability Decrease(double delta) => new Probability(Value - delta);
        
        public static Probability FromValue(double probability) => new Probability(probability);

        protected override int GetHashCodeCore()
        {
            unchecked
            {
                int hashCode = Value.GetHashCode();
                return (hashCode * 397) ^ Value.GetHashCode();
            }
        }
    }
}