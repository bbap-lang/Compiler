using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using BBAP.Functions;
using BBAP.Functions.AbapFunctions;
using BBAP.Functions.BbapFunctions;
using BBAP.Parser.Expressions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.Results;
using BBAP.Types;
using Error = BBAP.Results.Error;

namespace BBAP.PreTranspiler;

public class PreTranspilerState {

    private readonly Dictionary<string, IType> _internalVariables = new();

    private ulong _currentStackCount = 0;
    private readonly DefaultClasses.Stack<string> _stack = new();
    
    private readonly DefaultClasses.Stack<IVariable[]> _returnVariables = new();

    private readonly Dictionary<string, IFunction> _functions = new() {
        { "PRINT", new Print() },
        { "PRINTLINE", new PrintLine() },
        { "CONCATENATE", new Concatenate()},
        { "STRING_TOCHARARRAY", new StringToCharArray()},
    };

    private readonly Dictionary<string, SecondStageFunctionExpression> _declaredFunctions = new();
    
    public TypeCollection Types { get; } = new();

    public PreTranspilerState() {
        _stack.Push("");
    }

    public Result<IVariable> GetVariable(string name, int line) {
        foreach (string layer in _stack) {
            string variableName = $"{name}_{layer}";
            if (_internalVariables.TryGetValue(variableName, out IType? type)) {
                return Ok<IVariable>(new Variable(type, variableName));
            }
        }

        if (_internalVariables.TryGetValue(name, out IType? varType)) {
            return Ok<IVariable>(new Variable(varType, name));
        }
        
        return Error(line, $"Variable '{name}' was not defined");
    }

    public Result<IVariable> GetVariable(IVariable variable, int line) {
        IVariable[] variableTree = variable.Unwrap();

        Result<IVariable> topVariableResult = GetVariable(variableTree[0].Name, line);
        if(!topVariableResult.TryGetValue(out IVariable? topVariable)) {
            return topVariableResult.ToErrorResult();
        }
        
        IVariable? lastVariable = topVariable;
        foreach (IVariable currentVariable in variableTree.Skip(1)) {
            if (lastVariable.Type is not StructType structType) {
                return Error(line, $"The type of {lastVariable.Name} is not a struct.");
            }

            IVariable? field = structType.Fields.FirstOrDefault(x => x.Name == currentVariable.Name);
            if (field is null) {
                return Error(line, $"The field {currentVariable.Name} was not found in {lastVariable.Name}.");
            }

            field = new FieldVariable(field.Type, field.Name, lastVariable);

            lastVariable = field;
        }

        return Ok(lastVariable);
    }

    public string StackIn() {
        string stackName = GetNextStackName();

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
    
    public VariableExpression CreateRandomNewVar(int line, IType type) => new(line, new Variable( type, GenerateInternalVariableName(type)));

    
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

    private string GetNextStackName() {
        StringBuilder builder = new();
        ulong id = _currentStackCount;

        uint charsLength = (uint) Chars.Length;
        while (id > 0) {
            builder.Append(Chars[(int) (id % charsLength)]);
            id /= charsLength;
        }
        
        _currentStackCount++;

        return builder.ToString();
    }

    public Result<IFunction> AddFunction(SecondStageFunctionExpression functionExpression) {
        
        Result<IFunction> functionResult = AddFunction(functionExpression.Line, functionExpression.Name, functionExpression.Parameters.Select(x => x.Variable).ToImmutableArray(), functionExpression.ReturnVariables.Select(x => x.Variable).ToImmutableArray());

        if (!functionResult.TryGetValue(out IFunction? function)) {
            return functionResult.ToErrorResult();
        }
        
        _declaredFunctions.Add(function.Name, functionExpression);

        return Ok(function);
    }

    private Result<IFunction> AddFunction(int line, string name, ImmutableArray<IVariable> parameters, ImmutableArray<IVariable> returnType) {
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

    public void GoIntoFunction(IVariable[] returnVariables) {
        _returnVariables.Push(returnVariables);
    }

    public IVariable[] GetCurrentReturnVariables() {
        return _returnVariables.Peek();
    }
    
    public void GoOutOfFunction() {
        _returnVariables.Pop();
    }
}