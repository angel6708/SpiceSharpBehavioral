﻿using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpBehavioral.Components.Parsers.Double
{
    /// <summary>
    /// Arithmetic operator for behavioral components.
    /// </summary>
    /// <seealso cref="SpiceSharpBehavioral.Parsers.Double.ArithmeticOperator" />
    public class ArithmeticOperator : SpiceSharpBehavioral.Parsers.Double.ArithmeticOperator
    {
        private readonly Func<double> _fudgeFactor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueFactory"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ArithmeticOperator(BehavioralBindingContext context)
        {
            if (context.TryGetSimulationParameterSet(out BiasingParameters bp))
                _fudgeFactor = () => bp.Gmin * 1e-20;
            else
                _fudgeFactor = () => 1e-20;
        }

        /// <summary>
        /// Divides the operands.
        /// </summary>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The division (left) / (right).
        /// </returns>
        public override double Divide(double left, double right)
        {
            // Stay away from 0
            if (right >= 0)
                right += _fudgeFactor();
            else
                right -= _fudgeFactor();
            if (right.Equals(0.0))
                return double.PositiveInfinity;
            return left / right;
        }

        /// <summary>
        /// Raises a base to an exponent.
        /// </summary>
        /// <param name="base">The base value.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ (exponent).
        /// </returns>
        public override double Pow(double @base, double exponent)
        {
            return Math.Pow(Math.Abs(@base), exponent);
        }

        /// <summary>
        /// Raises the base to an integer (fixed) exponent.
        /// </summary>
        /// <param name="base">The base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns>
        /// The power (base) ^ exponent
        /// </returns>
        public override double Pow(double @base, int exponent)
        {
            switch (exponent)
            {
                case 0: return 1.0;
                case 1: return Math.Abs(@base);
                case 2: return @base * @base;
                default: return Math.Pow(Math.Abs(@base), exponent);
            }
        }
    }
}
