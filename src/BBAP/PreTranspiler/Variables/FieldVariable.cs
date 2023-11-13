using BBAP.Types;

namespace BBAP.PreTranspiler; 

public record FieldVariable(IType Type, string Name, IVariable SourceVariable) : IVariable;