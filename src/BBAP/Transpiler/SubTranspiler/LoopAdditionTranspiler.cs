namespace BBAP.Transpiler.SubTranspiler;

public class LoopAdditionTranspiler {
    public static void RunBreak(AbapBuilder builder) {
        builder.AppendLine($"EXIT.");
    }
    
    public static void RunContinue(AbapBuilder builder) {
        builder.AppendLine($"CONTINUE.");
    }
}