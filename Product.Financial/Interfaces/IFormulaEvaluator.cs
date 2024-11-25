using Product.Financial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Financial.Interfaces
{
    public interface IFormulaEvaluator
    {
        decimal Evaluate(string identifier, Dictionary<string, decimal?> inputValues);
    }

}
