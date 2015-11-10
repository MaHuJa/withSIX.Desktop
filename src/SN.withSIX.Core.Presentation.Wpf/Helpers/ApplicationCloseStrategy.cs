// <copyright company="SIX Networks GmbH" file="ApplicationCloseStrategy.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Presentation.Wpf.Helpers
{
    public class ApplicationCloseStrategy : ICloseStrategy<IScreen>
    {
        Action<bool, IEnumerable<IScreen>> callback;
        IEnumerator<IScreen> enumerator;
        bool finalResult;

        public void Execute(IEnumerable<IScreen> toClose, Action<bool, IEnumerable<IScreen>> callback) {
            enumerator = toClose.GetEnumerator();
            this.callback = callback;
            finalResult = true;

            Evaluate(finalResult);
        }

        void Evaluate(bool result) {
            finalResult = finalResult && result;

            if (!enumerator.MoveNext() || !result)
                callback(finalResult, new List<IScreen>());
            else {
                var current = enumerator.Current;
                var conductor = current as IConductor;
                if (conductor != null) {
                    var tasks = conductor.GetChildren()
                        .OfType<IHaveShutdownTask>()
                        .Select(x => x.GetShutdownTask())
                        .Where(x => x != null);

                    var sequential = new SequentialResult(tasks.GetEnumerator());
                    sequential.Completed += (s, e) => {
                        if (!e.WasCancelled)
                            Evaluate(!e.WasCancelled);
                    };
                    sequential.Execute(new CoroutineExecutionContext());
                } else
                    Evaluate(true);
            }
        }
    }
}