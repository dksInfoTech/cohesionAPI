using Product.Financial.Interfaces;
using Product.Financial.Models;

namespace Product.Financial
{
    public class FormulaEvaluatorService: IFormulaEvaluator
    {
        private readonly Dictionary<string, FinancialFormula> _formulas;

        public FormulaEvaluatorService(List<FinancialFormula> predefinedFormulas)
        {
            _formulas = new Dictionary<string, FinancialFormula>();
            foreach (var formula in predefinedFormulas)
            {
                _formulas[formula.Identifier] = formula;
            }
        }

        public decimal Evaluate(string identifier, Dictionary<string, decimal?> inputValues)
        {
            if (!_formulas.ContainsKey(identifier))
            {
                throw new ArgumentException($"Formula with identifier '{identifier}' not found.");
            }
            
            var formula = _formulas[identifier];
            var expression = new NCalc.Expression(formula.Expression);

            // Inject input values into the expression
            foreach (var input in inputValues)
            {
                expression.Parameters[input.Key] = input.Value != null ? input.Value : 0m;
            }

            // Evaluate the expression
            try
            {
                var result = expression.Evaluate();
                return Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to evaluate the expression '{formula.Expression}': {ex.Message}");
            }
        }
    }
}
