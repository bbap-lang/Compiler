using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using BBAP.Functions;
using BBAP.Functions.AbapFunctions;
using BBAP.Functions.BbapFunctions;
using BBAP.Parser.Expressions.Values;
using BBAP.PreTranspiler.Expressions;
using BBAP.PreTranspiler.Variables;
using BBAP.Results;
using BBAP.Types;
using BBAP.Types.Types.FullTypes;

namespace BBAP.PreTranspiler;

public class PreTranspilerState {
    private const string Chars = "ABDEFGHIJKLMNOPQRSTUVWXYZ";

    private readonly Dictionary<string, SecondStageFunctionExpression> _declaredFunctions = new();

    private readonly Dictionary<string, IFunction> _functions = new() {
        { "PRINT", new Print() },
        { "PRINTLINE", new PrintLine() },
        { "CONCATENATE", new Concatenate() },
        { "STRING_TOCHARARRAY", new StringToCharArray() }
    };

    private readonly DefaultClasses.Stack<IVariable[]> _returnVariables = new();

    private record StackItem(string Name, StackType Type);
    private readonly DefaultClasses.Stack<StackItem> _stack = new();

    private readonly Dictionary<string, IType> _variables = new();

    private ulong _currentStackCount;

    private bool _useStack;

    public PreTranspilerState() {
        _stack.Push(new StackItem("", StackType.None));
    }

    public TypeCollection Types { get; } = new();

    public Result<IVariable> GetVariable(string name, int line) {
        foreach (StackItem layer in _stack) {
            string variableName = $"{name}_{layer.Name}";
            if (_variables.TryGetValue(variableName, out IType? type))
                return Ok<IVariable>(new Variable(type, variableName));
        }

        if (_variables.TryGetValue(name, out IType? varType)) return Ok<IVariable>(new Variable(varType, name));

        return Error(line, $"Variable '{name}' was not defined");
    }

    public Result<IVariable> GetVariable(IVariable variable, int line) {
        IVariable[] variableTree = variable.Unwrap();

        Result<IVariable> topVariableResult = GetVariable(variableTree[0].Name, line);
        if (!topVariableResult.TryGetValue(out IVariable? topVariable)) return topVariableResult.ToErrorResult();

        IVariable lastVariable = topVariable;
        foreach (IVariable currentVariable in variableTree.Skip(1)) {
            IType currentType = lastVariable.Type;
            if (currentType is AliasType aliasType) currentType = aliasType.GetRealType();

            if (currentType is not StructType structType)
                return Error(line, $"The type of {lastVariable.Name} is not a struct.");

            IVariable? field = structType.Fields.FirstOrDefault(x => x.Name == currentVariable.Name);
            if (field is null)
                return Error(line, $"The field {currentVariable.Name} was not found in {lastVariable.Name}.");

            field = new FieldVariable(field.Type, field.Name, lastVariable);

            lastVariable = field;
        }

        return Ok(lastVariable);
    }

    public string StackIn(StackType stackType) {
        string stackName = GetNextStackName();

        StackIn(stackName, stackType);

        return stackName;
    }

    public void StackIn(string stackName, StackType stackType) {
        _stack.Push(new StackItem(stackName, stackType));
    }

    public void StackOut() {
        _stack.Pop();
    }
    
    public bool IsIn(StackType stackType) {
        return _stack.Any(layer => layer.Type == stackType);
    }

    public Result<string> CreateVar(string name, IType type, int line) {
        string variableName = _useStack ? $"{name}_{_stack.Peek().Name}" : name;
        if (_variables.ContainsKey(variableName))
            return Error(line, $"The variable '{name}' does already exists in this context.");

        _variables.Add(variableName, type);

        return Ok(variableName);
    }

    public VariableExpression CreateRandomNewVar(int line, IType type) {
        return new VariableExpression(line, new Variable(type, GenerateInternalVariableName(type)));
    }


    private string GenerateInternalVariableName(IType type) {
        string newName;

        do {
            newName = "INTERNAL_" + GenerateRandomString(5);
        } while (_variables.ContainsKey(newName));

        _variables.Add(newName, type);
        return newName;
    }

    private static string GenerateRandomString(int length) {
        var builder = new StringBuilder();
        for (int i = 0; i < length; i++) {
            builder.Append(Chars[Random.Shared.Next(Chars.Length)]);
        }

        return builder.ToString();
    }

    private string GetNextStackName() {
        StringBuilder builder = new();
        ulong id = _currentStackCount;

        uint charsLength = (uint)Chars.Length;
        while (id > 0) {
            builder.Append(Chars[(int)(id % charsLength)]);
            id /= charsLength;
        }

        _currentStackCount++;

        return builder.ToString();
    }

    public Result<IFunction> AddFunction(SecondStageFunctionExpression functionExpression) {
        Result<IFunction> functionResult = AddFunction(functionExpression.Line, functionExpression.Name,
                                                       functionExpression.Parameters.Select(x => x.Variable)
                                                                         .ToImmutableArray(),
                                                       functionExpression.ReturnVariables.Select(x => x.Variable)
                                                                         .ToImmutableArray(),
                                                       functionExpression.Attributes);

        if (!functionResult.TryGetValue(out IFunction? function)) return functionResult.ToErrorResult();

        _declaredFunctions.Add(function.Name, functionExpression);

        return Ok(function);
    }

    private Result<IFunction> AddFunction(int line,
        string name,
        ImmutableArray<IVariable> parameters,
        ImmutableArray<IVariable> returnType,
        FunctionAttributes functionAttributes) {
        if (_functions.ContainsKey(name)) return Error(line, $"The function {name} was already defined.");

        var newFunction = new GenericFunction(name, parameters, returnType, functionAttributes);

        _functions.Add(name, newFunction);
        return Ok<IFunction>(newFunction);
    }

    public Result<IFunction> GetFunction(string name, int line) {
        string[] splittedName = name.Split('.');


        IType? type = null;
        if (splittedName.Length > 1) {
            string typeName = splittedName[0];

            Result<IType> typeResult = Types.Get(line, typeName);
            if (!typeResult.TryGetValue(out type)) throw new UnreachableException();

            name = splittedName[^1];
        }

        do {
            string fullName = type is null ? name : $"{type.Name}_{name}";
            if (_functions.TryGetValue(fullName, out IFunction? function) && function.IsMethod == type is not null)
                return Ok(function);

            type = type?.InheritsFrom;
        } while (type is not null);

        return Error(line, $"Function '{name}' is not defined.");
    }

    public SecondStageFunctionExpression GetDeclaredFunction(string functionName) {
        if (!_declaredFunctions.TryGetValue(functionName, out SecondStageFunctionExpression? declaredFunction))
            throw new UnreachableException();

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

    public void FinishInitialization(bool useStack) {
        _useStack = useStack;
    }

    public void ReplaceType(IType oldType, IType newType) {
        Types.Replace(oldType, newType);

        string[] variablesToReplace = _variables.Where(x => x.Value == oldType).Select(x => x.Key).ToArray();
        foreach (string variable in variablesToReplace) {
            _variables[variable] = newType;
        }
    }

    public record GetFunctionResponse(IFunction Function, IVariable? FirstParameter);
}

public enum StackType {
    None,
    Function,
    Loop,
    Fork
}