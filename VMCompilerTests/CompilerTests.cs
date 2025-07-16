using System;
using System.Reflection;
using Xunit;

public class CompilerTests
{
    private static Type CompilerType => typeof(Compiler);

    private static object InvokePrivate(string name, params object[] args)
    {
        var method = CompilerType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        if(method == null) throw new Exception("Method not found"+name);
        return method.Invoke(null, args)!;
    }

    [Fact]
    public void Parse_Variable_Declaration()
    {
        var result = Compiler.Compile("qword x = 5;");
        Assert.Contains("push 5", string.Join(' ', result));
    }

    [Fact]
    public void Parse_Function_Call()
    {
        var funcType = CompilerType.GetNestedType("ParsedFunction", BindingFlags.NonPublic)!;
        dynamic func = Activator.CreateInstance(funcType);
        funcType.GetField("label")!.SetValue(func, "foo");
        var dtListType = typeof(System.Collections.Generic.List<>).MakeGenericType(CompilerType.GetNestedType("DataTypeEnum")!);
        var dtList = Activator.CreateInstance(dtListType) as System.Collections.IList;
        dtList!.Add(Enum.GetValues(CompilerType.GetNestedType("DataTypeEnum")!).GetValue(4));
        funcType.GetField("paramDataTypes")!.SetValue(func, dtList);
        var labelList = new System.Collections.Generic.List<string>{"p"};
        funcType.GetField("paramLabels")!.SetValue(func, labelList);
        funcType.GetField("returnType")!.SetValue(func, Enum.GetValues(CompilerType.GetNestedType("DataTypeEnum")!).GetValue(4));
        funcType.GetField("instructions")!.SetValue(func, "entry ret");
        var table = (System.Collections.IDictionary)CompilerType.GetField("functionTable", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        table["foo"] = func;
        var result = Compiler.Compile("foo(1);");
        Assert.Contains("ret", result[0]);
        table.Clear();
    }

    [Fact]
    public void CompileVariable_WritesPush()
    {
        var varType = CompilerType.GetNestedType("ParsedVariable", BindingFlags.NonPublic);
        dynamic varObj = Activator.CreateInstance(varType!);
        varType!.GetField("dataType")!.SetValue(varObj, Enum.GetValues(CompilerType.GetNestedType("DataTypeEnum")!).GetValue(4));
        varType.GetField("value")!.SetValue(varObj, 10L);
        InvokePrivate("CompileVariable", new object[]{varObj});
        string instr = varType.GetField("instructions", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)!.GetValue(varObj) as string;
        Assert.Contains("push", instr);
    }

    [Fact]
    public void CompileIf_GeneratesJump()
    {
        var varType = CompilerType.GetNestedType("ParsedVariable", BindingFlags.NonPublic)!;
        dynamic left = Activator.CreateInstance(varType);
        varType.GetField("dataType")!.SetValue(left, Enum.GetValues(CompilerType.GetNestedType("DataTypeEnum")!).GetValue(4));
        varType.GetField("value")!.SetValue(left, 5L);
        dynamic right = Activator.CreateInstance(varType);
        varType.GetField("dataType")!.SetValue(right, Enum.GetValues(CompilerType.GetNestedType("DataTypeEnum")!).GetValue(4));
        varType.GetField("value")!.SetValue(right, 5L);
        var ifType = CompilerType.GetNestedType("ParsedIf", BindingFlags.NonPublic)!;
        dynamic ifObj = Activator.CreateInstance(ifType);
        ifType.GetField("body")!.SetValue(ifObj, "");
        ifType.GetField("_operator")!.SetValue(ifObj, "==");
        var tupleType = typeof(Tuple<,>).MakeGenericType(varType, varType);
        var tuple = Activator.CreateInstance(tupleType, left, right);
        ifType.GetField("operands")!.SetValue(ifObj, tuple);
        InvokePrivate("CompileIf", new object[]{ifObj});
        string instr = ifType.GetField("instructions", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)!.GetValue(ifObj) as string;
        Assert.Contains("tjmp", instr);
    }
}
