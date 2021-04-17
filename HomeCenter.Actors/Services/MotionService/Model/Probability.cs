using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Represent a probability of person to be in room.
    /// </summary>
    public class Probability : ValueObject
    {
        public static readonly Probability Zero = new Probability(0.0);
        public static readonly Probability Full = new Probability(1.0);

        public double Value { get; }

        public bool IsNoProbability => Equals(Zero);

        public bool IsFullProbability => Equals(Full);

        public Probability(double probability)
        {
            if (probability < 0)
            {
                probability = 0.0;
            }
            else if (probability > 1)
            {
                probability = 1.0;
            }

            Value = probability;
        }

        public override string ToString() => $"{(Value*100.0):00.00}%";

        public override bool Equals(object? obj)
        {
            if (obj is not Probability other)
            {
                throw new ArgumentNullException();
            }

            return Math.Abs(other.Value - Value) < 0.01;
        }

        public Probability Decrease(double delta)
        {
            var value = Math.Round(Value - delta, 10);

            return new Probability(value);
        }

        public Probability DecreaseByPercent(double percent) => new Probability(Value - (Value * percent));

        public static Probability FromValue(double probability) => new Probability(probability);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Value.GetHashCode();
                return (hashCode * 397) ^ Value.GetHashCode();
            }
        }
    }
}