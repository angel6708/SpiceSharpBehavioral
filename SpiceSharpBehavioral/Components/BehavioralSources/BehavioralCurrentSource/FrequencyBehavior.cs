﻿using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.BehavioralComponents;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Simulations;
using SpiceSharpBehavioral.Parsers.Nodes;
using System;
using System.Numerics;

namespace SpiceSharp.Components.BehavioralCurrentSourceBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="BehavioralCurrentSource"/>.
    /// </summary>
    /// <seealso cref="BiasingBehavior" />
    /// <seealso cref="IFrequencyBehavior" />
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        private readonly OnePort<Complex> _variables;
        private readonly ElementSet<Complex> _elements;
        private readonly Complex[] _values;

        /// <summary>
        /// Gets the complex current.
        /// </summary>
        /// <value>
        /// The complex current.
        /// </value>
        public Complex ComplexCurrent { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, BehavioralComponentContext context)
            : base(name, context)
        {
            var state = context.GetState<IComplexSimulationState>();
            _variables = new OnePort<Complex>(
                state.GetSharedVariable(context.Nodes[0]),
                state.GetSharedVariable(context.Nodes[1]));

            var rows = _variables.GetRhsIndices(state.Map);
            var matLocs = new MatrixLocation[Functions.Length * 2];
            _values = new Complex[Functions.Length * 2];
            for (var i = 0; i < Functions.Length; i++)
            {
                IVariable<Complex> variable = Functions[i].Item1.NodeType switch
                {
                    NodeTypes.Voltage => state.GetSharedVariable(Functions[i].Item1.Name),
                    NodeTypes.Current => context.Branches[Functions[i].Item1].GetValue<IBranchedBehavior<Complex>>().Branch,
                    _ => throw new Exception("Invalid variable"),
                };
                matLocs[i * 2] = new MatrixLocation(rows[0], state.Map[variable]);
                matLocs[i * 2 + 1] = new MatrixLocation(rows[1], state.Map[variable]);
            }

            // Get the matrix elements
            _elements = new ElementSet<Complex>(state.Solver, matLocs);
        }

        void IFrequencyBehavior.InitializeParameters()
        {
            for (var i = 0; i < Functions.Length; i++)
            {
                var value = Functions[i].Item3.Invoke();
                _values[i * 2] = value;
                _values[i * 2 + 1] = -value;
            }
        }

        void IFrequencyBehavior.Load()
        {
            _elements.Add(_values);
        }
    }
}
