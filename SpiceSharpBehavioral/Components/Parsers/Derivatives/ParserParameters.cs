﻿using SpiceSharp;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Components.BehavioralBehaviors;
using SpiceSharpBehavioral.Parsers;
using SpiceSharpBehavioral.Parsers.Derivatives;
using System;

namespace SpiceSharpBehavioral.Components.Parsers
{
    /// <summary>
    /// Parser parameters for components.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IParserParameters{T}" />
    public class ParserParameters : ParameterSet, IParserParameters<IDerivatives<Variable, Func<double>>>
    {
        /// <summary>
        /// Gets the arithmetic operators.
        /// </summary>
        /// <value>
        /// The arithmetic operators.
        /// </value>
        public IArithmeticOperator<IDerivatives<Variable, Func<double>>> Arithmetic { get; }

        /// <summary>
        /// Gets the conditional operators.
        /// </summary>
        /// <value>
        /// The conditional operators.
        /// </value>
        public IConditionalOperator<IDerivatives<Variable, Func<double>>> Conditional { get; }

        /// <summary>
        /// Gets the relational operators.
        /// </summary>
        /// <value>
        /// The relational operators.
        /// </value>
        public IRelationalOperator<IDerivatives<Variable, Func<double>>> Relational { get; }

        /// <summary>
        /// Gets the value factory.
        /// </summary>
        /// <value>
        /// The value factory.
        /// </value>
        public IValueFactory<IDerivatives<Variable, Func<double>>> ValueFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserParameters"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ParserParameters(BehavioralBindingContext context)
        {
            var parent = new DoubleFunc.ParserParameters(context);
            var factory = new DerivativeFactory<Variable, Func<double>>();
            Arithmetic = new ArithmeticOperator<Variable, Func<double>>(parent, factory);
            Conditional = new ConditionalOperator<Variable, Func<double>>(parent, factory);
            Relational = new RelationalOperator<Variable, Func<double>>(parent, factory);
            ValueFactory = new ValueFactory<Variable, Func<double>>(parent, factory);
        }
    }
}
