﻿using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralCapacitorBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCapacitor"/>.
    /// </summary>
    [BehaviorFor(typeof(BehavioralCapacitor), typeof(IFrequencyBehavior), 1)]
    public class FrequencyBehavior : BiasingBehavior,
        IFrequencyBehavior
    {
        private readonly IComplexSimulationState _state;
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly Func<Complex>[] _derivatives;
        private readonly IVariable<Complex>[] _derivativeVariables;

        /// <summary>
        /// Gets the complex voltage.
        /// </summary>
        /// <value>The complex voltage.</value>
        [ParameterName("v"), ParameterInfo("The complex voltage")]
        public Complex ComplexVoltage => _variables.Positive.Value - _variables.Negative.Value;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        [ParameterName("i"), ParameterInfo("The complex current")]
        public Complex ComplexCurrent
        {
            get
            {
                Complex total = 0.0;
                for (var i = 0; i < _derivatives.Length; i++)
                    total += _derivatives[i]() * _derivativeVariables[i].Value;
                return total * _state.Laplace;
            }
        }

        /// <summary>
        /// Gets the complex power.
        /// </summary>
        [ParameterName("p"), ParameterInfo("The complex power")]
        public Complex ComplexPower => ComplexVoltage * Complex.Conjugate(ComplexCurrent);

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(BehavioralBindingContext context)
            : base(context)
        {
            var bp = context.GetParameterSet<Parameters>();
            _state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                _state.GetSharedVariable(context.Nodes[0]),
                _state.GetSharedVariable(context.Nodes[1]));

            _derivatives = new Func<Complex>[Derivatives.Count];
            _derivativeVariables = new IVariable<Complex>[Derivatives.Count];
            var nVariables = new Dictionary<VariableNode, IVariable<Complex>>(Derivatives.Comparer);
            foreach (var variable in Derivatives.Keys)
            {
                var orig = DerivativeVariables[variable];
                nVariables.Add(variable, new FuncVariable<Complex>(orig.Name, () => orig.Value, orig.Unit));
            }
            var builder = context.CreateBuilder(bp.ComplexBuilderFactory, nVariables);
            var matLocs = new MatrixLocation[Derivatives.Count * 2];
            var rhsLocs = _variables.GetRhsIndices(_state.Map);
            int index = 0;
            foreach (var pair in Derivatives)
            {
                _derivatives[index] = builder.Build(pair.Value);
                var variable = context.MapNode(_state, pair.Key);
                _derivativeVariables[index] = variable;
                matLocs[index * 2] = new MatrixLocation(rhsLocs[0], _state.Map[variable]);
                matLocs[index * 2 + 1] = new MatrixLocation(rhsLocs[1], _state.Map[variable]);
                index++;
            }
            _elements = new ElementSet<Complex>(_state.Solver, matLocs);
        }

        /// <summary>
        /// The frequency-dependent initialization.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
        }

        /// <summary>
        /// The frequency-dependent load behavior.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var values = new Complex[_derivatives.Length * 2];
            for (var i = 0; i < _derivatives.Length; i++)
            {
                var g = _state.Laplace * _derivatives[i]();
                values[i * 2] = g;
                values[i * 2 + 1] = -g;
            }
            _elements.Add(values);
        }
    }
}
