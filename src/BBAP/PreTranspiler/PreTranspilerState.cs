using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using BBAP.Functions;
using BBAP.Functions.AbapFunctions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;

namespace BBAP.PreTranspiler;

public class PreTranspilerState {

    private readonly Dictionary<string, IType> _internalVariables = new();

    private readonly HashSet<string> _stackNames = new();
    private readonly DefaultClasses.Stack<string> _stack = new();
    
    private readonly DefaultClasses.Stack<Variable[]> _returnVariables = new();

    private readonly Dictionary<string, IFunction> _functions = new() {
        { "PRINT", new Print() },
        { "PRINTLINE", new PrintLine() },
    };

    private readonly Dictionary<string, SecondStageFunctionExpression> _declaredFunctions = new();
    
    public TypeCollection Types { get; } = new();

    public PreTranspilerState() {
        _stackNames.Add("");
        _stack.Push("");
    }

    public Result<Variable> GetVariable(string name, int line) {
        foreach (string layer in _stack) {
            string variableName = $"{name}_{layer}";
            if (_internalVariables.TryGetValue(variableName, out IType? type)) {
                return Ok(new Variable(type, variableName));
            }
        }

        if (_internalVariables.TryGetValue(name, out IType? varType)) {
            return Ok(new Variable(varType, name));
        }
        
        return Error(line, $"Variable '{name}' was not defined");
    }

    public string StackIn() {
        string stackName;

        do {
            stackName = GenerateRandomString(3);
        } while (!_stackNames.Add(stackName));


        StackIn(stackName);
        return stackName;
    }
    
    public void StackIn(string stackName) {
        _stack.Push(stackName);
    }

    public void StackOut() {
        _stack.Pop();
    }

    public Result<string> CreateVar(string name, IType type, int line) {
        string variableName = $"{name}_{_stack.Peek()}";
        if (_internalVariables.ContainsKey(variableName)) {
            return Error(line, $"The variable '{name}' does already exists in this context.");
        }
        
        _internalVariables.Add(variableName, type);
        
        return Ok(variableName);
    }
    
    public VariableExpression CreateRandomNewVar(int line, IType type) => new(line, GenerateInternalVariableName(type), type);

    
    private string GenerateInternalVariableName(IType type) {
        string newName;

        do {
            newName = "INTERNAL_" + GenerateRandomString(5);
        } while (_internalVariables.ContainsKey(newName));
        
        _internalVariables.Add(newName, type);
        return newName;
    }
    
    private const string Chars = "ABDEFGHIJKLMNOPQRSTUVWXYZ";
    private static string GenerateRandomString(int length){
        var builder = new StringBuilder();
        for (int i = 0; i < length; i++) {
            builder.Append(Chars[Random.Shared.Next(Chars.Length)]);
        }

        return builder.ToString();
    }

    public Result<IFunction> AddFunction(SecondStageFunctionExpression functionExpression) {
        
        Result<IFunction> functionResult = AddFunction(functionExpression.Line, functionExpression.Name, functionExpression.Parameters, functionExpression.ReturnVariables);

        if (!functionResult.TryGetValue(out IFunction? function)) {
            return functionResult.ToErrorResult();
        }
        
        _declaredFunctions.Add(function.Name, functionExpression);

        return Ok(function);
    }

    private Result<IFunction> AddFunction(int line, string name, ImmutableArray<Variable> parameters, ImmutableArray<Variable> returnType) {
        if (_functions.ContainsKey(name)) {
            return Error(line, $"The function {name} was already defined.");
        }
        
        var newFunction = new GenericFunction(name, parameters, returnType);
        
        _functions.Add(name, newFunction);
        return Ok<IFunction>(newFunction);
    }

    public Result<IFunction> GetFunction(string name, int line) {
        if (!_functions.TryGetValue(name, out IFunction? function)) {
            return Error(line, $"Function '{name}' is not defined.");
        }

        return Ok(function);
    }

    public SecondStageFunctionExpression GetDeclaredFunction(string functionName) {
        if (!_declaredFunctions.TryGetValue(functionName, out var declaredFunction)) {
            throw new UnreachableException();
        }

        return declaredFunction;
    }

    public void GoIntoFunction(Variable[] returnVariables) {
        _returnVariables.Push(returnVariables);
    }

    public Variable[] GetCurrentReturnVariables() {
        return _returnVariables.Peek();
    }
    
    public void GoOutOfFunction() {
        _returnVariables.Pop();
    }
}