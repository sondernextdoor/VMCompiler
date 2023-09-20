public class Compiler
    {

		private class ParsedInstruction
        {
			public InstructionTypeEnum instructionType;

			public ParsedInstruction( InstructionTypeEnum type )
            {
				instructionType = type;
            }
        }


		private class ParsedFunction : ParsedInstruction
		{
			public string label;
			public List<DataTypeEnum> paramDataTypes;
			public List<string> paramLabels;
			public DataTypeEnum returnType;
			public string instructions;
			public long[] opcodes;

			public ParsedFunction() : base( InstructionTypeEnum.typeFunction ) { paramDataTypes = new(); paramLabels = new(); }
		}


		private class ParsedCall : ParsedInstruction
		{
			public ParsedFunction function;
			public List<object> parameters;
			public List<DataTypeEnum> paramDataTypes;

			public ParsedCall() : base( InstructionTypeEnum.typeCall ) { parameters = new(); paramDataTypes = new(); }
		}


		private class ParsedVariable : ParsedInstruction
        {
			public DataTypeEnum dataType;
			public string dataTypeString;
			public string label;
			public object value;
			public string instructions;

			public ParsedVariable() : base( InstructionTypeEnum.typeVariable ) { }
		}


		private class ParsedAssignment : ParsedInstruction
		{
			public string variableLabel;
			public DataTypeEnum dataType;
			public object value;
			public string instructions;

			public ParsedAssignment() : base( InstructionTypeEnum.typeAssignment ) { }
		}


		private class ParsedLoop : ParsedInstruction
        {
			public string condition;
			public string body;
			public string instructions;

			public ParsedLoop() : base( InstructionTypeEnum.typeLoop ) { }
        }


		private class ParsedIf : ParsedInstruction
        {
			public string condition;
			public string body;
			public string instructions;
			public string _operator;
			public Tuple<ParsedVariable, ParsedVariable> operands;

			public ParsedIf() : base( InstructionTypeEnum.typeIf) { }
		}


		private readonly string validFunction = "function*(*)returns*{*}";
		private readonly string validVariable = "*=*;";
		private readonly string validLoop = "while(*){*}";
		private readonly string validIf = "if(*){*}";

		private static readonly Dictionary<string, ParsedVariable> variableTable = new();
		private static readonly Dictionary<string, ParsedFunction> functionTable = new();


		public enum DataTypeEnum
		{
			typeInvalid,
			typeByte,
			typeWord,
			typeDword,
			typeQword,
			typeString,
			typeBool,
			typeDetermine
		};


		public enum InstructionTypeEnum
		{
			typeInvalid,
			typeVariable,
			typeFunction,
			typeLoop,
			typeIf,
			typeCall,
			typeAssignment
		};


		static public readonly string[] dataTypes =
		{
			"zeroPlaceholder",
			"byte",
			"word",
			"dword",
			"qword",
			"string",
			"bool",
			"determine"
		};


		static public readonly string[] keywords =
		{
			"zeroPlaceholder",
			"byte",
			"word",
			"dword",
			"qword",
			"string",
			"bool",
			"determine",
			"function",
			"while",
			"if"
		};


		static public readonly string[] operators =
		{
			"zeroPlaceholder",
			"=",
			"+",
			"-",
			"*",
			"/",
			"%",
			"==",
			"!=",
			">=",
			"<=",
			">",
			"<",
			"&&",
			"||",
			">>",
			"<<",
			"^",
			"&"
		};


		public static DataTypeEnum StringToDataType( string dataTypeString )
		{
			if ( dataTypeString.StartsWith( "byte" ) )
			{
				return DataTypeEnum.typeByte;
			}

			if ( dataTypeString.StartsWith( "word" ) )
			{
				return DataTypeEnum.typeWord;
			}

			if ( dataTypeString.StartsWith( "dword" ) )
			{
				return DataTypeEnum.typeDword;
			}

			if ( dataTypeString.StartsWith( "qword" ) )
			{
				return DataTypeEnum.typeQword;
			}

			if ( dataTypeString.StartsWith( "string" ) )
			{
				return DataTypeEnum.typeString;
			}

			if ( dataTypeString.StartsWith( "bool" ) )
			{
				return DataTypeEnum.typeBool;
			}

			if ( dataTypeString.StartsWith( "determine" ) )
			{
				return DataTypeEnum.typeDetermine;
			}

			return DataTypeEnum.typeInvalid;
		}


		public static string DataTypeToString( DataTypeEnum dataType )
		{
            switch ( dataType )
            {
                case DataTypeEnum.typeByte:
                    return "byte";
                    break;

                case DataTypeEnum.typeWord:
                    return "word";
                    break;

                case DataTypeEnum.typeDword:
                    return "dword";
                    break;

                case DataTypeEnum.typeQword:
                    return "qword";
                    break;

                case DataTypeEnum.typeString:
                    return "string";
                    break;

                case DataTypeEnum.typeBool:
                    return "bool";
                    break;

                case DataTypeEnum.typeDetermine:
                    return "determine";
                    break;

                case DataTypeEnum.typeInvalid:
                    break;

                default:
                    break;
            }

            return string.Empty;
        }


        private static bool IsLiteral( string str )
        {
			return variableTable.TryGetValue( str, out _ ) == false;
        }


		private static ParsedFunction ParseFunctionParameters( string str, ref ParsedFunction parsedFunction, out int length )
        {
			StringPosition stringPosition = str.Between( '(', ')' );

			if ( stringPosition.Empty() )
            {
				length = 0;
				return parsedFunction; // ERROR
            }

			length = stringPosition.Length();

			StringPosition currentParam = stringPosition.str.Between( '(', ',' );

			if ( currentParam.Empty() )
            {
				currentParam = stringPosition;

				parsedFunction.paramDataTypes.Add( StringToDataType( currentParam.str ) );
				parsedFunction.paramLabels.Add( currentParam.str[( DataTypeToString( parsedFunction.paramDataTypes[0] ).Length - 1 )..] );

				return parsedFunction;
            }

			parsedFunction.paramDataTypes.Add( StringToDataType( currentParam.str ) );
			parsedFunction.paramLabels.Add( currentParam.str[( DataTypeToString( parsedFunction.paramDataTypes[0] ).Length - 1 )..] );

			for ( int i = 0; i < length; i++ )
            {
				currentParam = stringPosition.str.Between( ',', ',' );

				if ( currentParam.Empty() )
                {
					currentParam = stringPosition.str.Between( ',', ')' );

					if ( currentParam.Empty() )
                    {
						return parsedFunction; // ERROR
                    }

					parsedFunction.paramDataTypes.Add( StringToDataType( currentParam.str ) );
					parsedFunction.paramLabels.Add(currentParam.str[ ( DataTypeToString( parsedFunction.paramDataTypes[i]).Length - 1 )..] );

					return parsedFunction;
				}

				parsedFunction.paramDataTypes.Add( StringToDataType( currentParam.str ) );
				parsedFunction.paramLabels.Add( currentParam.str[( DataTypeToString( parsedFunction.paramDataTypes[i] ).Length - 1 )..] );
				stringPosition.str = stringPosition.str[( DataTypeToString( parsedFunction.paramDataTypes[i] ).Length - 1 + parsedFunction.paramLabels[i].Length - 1 )..];
			}

			return parsedFunction;
        }


		private static ParsedCall ParseCallParameters( string str, ref ParsedCall parsedCall, out int length )
		{
			// isExpression? check func table, check for operator signs. todo
			// Does it match the data type of the function call? todo
			// Does the func param count match the number of params passed in? todo


			StringPosition stringPosition = str.Between( '(', ')' );

			if ( stringPosition.Empty() )
			{
				length = 0;
				return parsedCall; // ERROR
			}

			length = stringPosition.Length();

			StringPosition currentParam = stringPosition.str.Insert( 0, "(" ).Between( '(', ',' );

			if ( currentParam.Empty() )
			{
				currentParam = stringPosition;

				if ( IsLiteral( currentParam.str ) )
                {
					if ( currentParam.str.StartsWith( '\"' ) )
					{
						parsedCall.paramDataTypes.Add( DataTypeEnum.typeString );
						parsedCall.parameters.Add( currentParam.str );
					}
					else
                    {
						parsedCall.paramDataTypes.Add( DataTypeEnum.typeQword );
						parsedCall.parameters.Add( Int64.Parse( currentParam.str ) );
                    }
                }
				else
                {
					if ( variableTable.TryGetValue( currentParam.str, out ParsedVariable parsedVariable ) )
                    {
						parsedCall.paramDataTypes.Add( parsedVariable.dataType );
						parsedCall.parameters.Add( parsedVariable.value );
                    }
                }

				return parsedCall;
			}

			int paramLength = 0;

			if ( IsLiteral( currentParam.str ) )
			{
				if ( currentParam.str.StartsWith( '\"' ) )
				{
					parsedCall.paramDataTypes.Add( DataTypeEnum.typeString );
					parsedCall.parameters.Add( currentParam.str );
				}
				else
				{
					parsedCall.paramDataTypes.Add( DataTypeEnum.typeQword );
					parsedCall.parameters.Add( long.Parse( currentParam.str ) );
				}
			}
			else
			{
				if ( variableTable.TryGetValue( currentParam.str, out ParsedVariable parsedVariable ) )
				{
					parsedCall.paramDataTypes.Add( parsedVariable.dataType );
					parsedCall.parameters.Add( parsedVariable.value );
				}
			}

			for ( int i = 0; i < length; i++ )
			{
				currentParam = stringPosition.str.Between( ',', ',' );

				if ( currentParam.Empty() )
				{
					currentParam = new string( stringPosition.str.Append( ')' ).ToArray() ).Between( ',', ')' );

					if ( currentParam.Empty() )
					{
						return parsedCall; // ERROR
					}

					if ( IsLiteral( currentParam.str ) )
					{
						if ( currentParam.str.StartsWith( '\"' ) )
						{
							parsedCall.paramDataTypes.Add( DataTypeEnum.typeString );
							parsedCall.parameters.Add( currentParam.str );
						}
						else
						{
							parsedCall.paramDataTypes.Add( DataTypeEnum.typeQword );
							parsedCall.parameters.Add( long.Parse( currentParam.str ) );
						}
					}
					else
					{
						if ( variableTable.TryGetValue( currentParam.str, out ParsedVariable parsedVariable ) )
						{
							parsedCall.paramDataTypes.Add( parsedVariable.dataType );
							parsedCall.parameters.Add( parsedVariable.value );
						}
					}

					return parsedCall;
				}

				if ( IsLiteral( currentParam.str ) )
				{
					if ( currentParam.str.StartsWith( '\"' ) )
					{
						parsedCall.paramDataTypes.Add( DataTypeEnum.typeString );
						parsedCall.parameters.Add( currentParam.str );
					}
					else
					{
						parsedCall.paramDataTypes.Add( DataTypeEnum.typeQword );
						parsedCall.parameters.Add( long.Parse( currentParam.str ) );
					}

					paramLength = currentParam.Length();
				}
				else
				{
					if ( variableTable.TryGetValue( currentParam.str, out ParsedVariable parsedVariable ) )
					{
						parsedCall.paramDataTypes.Add( parsedVariable.dataType );
						parsedCall.parameters.Add( parsedVariable.value );

						paramLength = parsedVariable.label.Length;
					}
				}

				stringPosition.str = stringPosition.str[paramLength..];
			}

			return parsedCall;
		}


		private static object ParseAssignmentValue( string str, out int length )
        {
			StringPosition value = str.Between( '=', ';' );
			length = value.Length();

			return value.str.StartsWith( '\"' ) ? value.str.Trim( '\"' ) : Int64.Parse( value.str );
        }


		private static string GetFunctionLabel( string function, int startIndex = 0 )
		{
			return function.Between( "function", "(", startIndex ).str;
		}


		private static string GetFunctionParameters( string function, int startIndex = 0 )
		{
			return function.Between( GetFunctionLabel( function ) + '(', ")", startIndex ).str;
		}


		private static string GetFunctionReturnType( string function, int startIndex = 0 )
		{
			return function.Between( "returns", "{", startIndex ).str;
		}


		private static string GetFunctionBody( string function, out int length, int startIndex = 0 )
		{
			StringPosition stringPosition = function.Between( '{', '}', startIndex );
			length = stringPosition.Length();

			return stringPosition.str;
		}


		private static List<ParsedInstruction> ParseCode( string str )
		{
			List<ParsedInstruction> instructions = new();

			for ( int i = 0; i < str.Length; i++ )
            {
				if ( str[i] == '\"' )
                {
					i += 1;
					
					while ( str[i] != '\"' )
                    {
						i += 1;
                    }

					i += 1;
                }

				if ( str[i] == ' ' )
                {
					str = str.Remove( i, 1 );
					i -= 1;
                }
            }

			for ( int i = 0; i < str.Length; i++ )
			{ 
				start:

				if ( i >= str.Length )
                {
					break;
                }

				// is it a function call?
				for ( int j = 0; j < functionTable.Count; j++ )
				{
					if ( str[i..].StartsWith( functionTable.ElementAt( j ).Key ) )
					{
						ParsedCall parsedCall = new();

						parsedCall.function = functionTable.ElementAt( j ).Value;
						ParseCallParameters( str[i..], ref parsedCall, out int length );
						instructions.Add( parsedCall );

						i += length;
						goto start;
					}
				}

				// is it a variable assignment?
				for ( int j = 0; j < variableTable.Count; j++ )
				{
					if ( str[i..].StartsWith( variableTable.ElementAt( j ).Key ) )
					{
						ParsedVariable parsedVariable = variableTable.ElementAt( j ).Value;
						ParsedAssignment parsedAssignment = new();

						parsedAssignment.variableLabel = parsedVariable.label;
						parsedAssignment.dataType = parsedVariable.dataType;
						parsedAssignment.value = ParseAssignmentValue( str[i..], out int length );

						instructions.Add( parsedAssignment );

						i += length;
						goto start;
					}
				}

				// is it a variable declaration?
				if ( str[i..].StartsWith( "byte" ) || str[i..].StartsWith( "word" ) || str[i..].StartsWith( "dword" )
					|| str[i..].StartsWith( "qword" ) || str[i..].StartsWith( "string" ) || str[i..].StartsWith( "bool" ) || str[i..].StartsWith( "determine" ) )
				{
					ParsedVariable parsedVariable = new();

					parsedVariable.dataType = StringToDataType( str[i..] );
					parsedVariable.dataTypeString = DataTypeToString( parsedVariable.dataType );

					i += parsedVariable.dataTypeString.Length;

					if ( i >= str.Length )
                    {
						break;
                    }

					StringPosition stringPosition = str[i..].Find( "=" );

					parsedVariable.label = str[( i - 1 )..].Between( parsedVariable.dataTypeString.Last(), '=' ).str;
					parsedVariable.value = ParseAssignmentValue( str[i..], out int length );

					instructions.Add( parsedVariable );
					variableTable.Add( parsedVariable.label, parsedVariable );

					i += parsedVariable.label.Length + 1 + length + 1; // i += [label] [=] [value] [;]
					goto start;
				}

				// is it a function declaration?
				if ( str[i..].StartsWith( "function" ) )
				{
					ParsedFunction parsedFunction = new();

					parsedFunction.label = GetFunctionLabel( str[i..] );
					parsedFunction.returnType = StringToDataType( GetFunctionReturnType( str[i..] ) );
					ParseFunctionParameters( str[i..], ref parsedFunction, out _ );
					parsedFunction.instructions = GetFunctionBody( str[i..], out int length );

					instructions.Add( parsedFunction );
					functionTable.Add( parsedFunction.label, parsedFunction );

					i += length;
					goto start;
				}

				// is it a loop?
				if ( str[i..].StartsWith( "while" ) )
				{
					ParsedLoop parsedLoop = new();

					parsedLoop.condition = str[i..].Between( '(', ')' ).str;

					StringPosition stringPosition = str[i..].Between( '{', '}' );
					parsedLoop.body = stringPosition.str;

					instructions.Add( parsedLoop );

					i += stringPosition.Length();
					goto start;
				}

				// is it an if statement?
				if ( str[i..].StartsWith( "if" ) )
				{
					ParsedIf parsedIf = new();

					parsedIf.condition = str[i..].Between( '(', ')' ).str;

					StringPosition stringPosition = str[i..].Between( '{', '}' );
					parsedIf.body = stringPosition.str;

					for ( int j = 0; j < parsedIf.condition.Length; j++ )
                    {
						if ( parsedIf.condition.Contains( operators[i] ) )
                        {
							parsedIf._operator = operators[i];
							break;
                        }
                    }

					if ( variableTable.TryGetValue( str[i..].Between( "(", parsedIf._operator ).str, out ParsedVariable parsedVariable ) != true )
                    {
						return instructions; // ERROR
                    }

					StringPosition secondOperandPosition = str[i..].Between( parsedIf._operator, ")" );
					ParsedVariable parsedVariableTwo = new();

					if ( variableTable.TryGetValue( secondOperandPosition.str, out parsedVariableTwo ) == false )
					{
						if ( secondOperandPosition.str.StartsWith( '\"' ) )
                        {
							if ( parsedVariable.dataType != DataTypeEnum.typeString )
                            {
								return instructions; // ERROR
                            }

							parsedVariableTwo.dataType = DataTypeEnum.typeString;
							parsedVariableTwo.dataTypeString = "string";
							parsedVariableTwo.value = secondOperandPosition.str;

							parsedIf.operands = new( parsedVariable, parsedVariableTwo );
                        }
						else
                        {
							parsedVariableTwo.dataType = DataTypeEnum.typeQword;
							parsedVariableTwo.dataTypeString = "qword";
							parsedVariableTwo.value = long.Parse( secondOperandPosition.str );

							parsedIf.operands = new( parsedVariable, parsedVariableTwo );
                        }
					}
					else
                    {
						parsedIf.operands = new( parsedVariable, parsedVariableTwo );
                    }

					instructions.Add( parsedIf );
					i += stringPosition.Length();
					goto start;
				}
			}

			return instructions;
		}


		private static ParsedCall CompileFunctionCall( ref ParsedCall parsedCall )
        {
			if ( parsedCall.parameters.Count > parsedCall.function.paramLabels.Count )
            {
				return parsedCall; // ERROR
            }

			for ( int i = 0; i < parsedCall.parameters.Count; i++ )
            {
				if ( parsedCall.paramDataTypes[i] != parsedCall.function.paramDataTypes[i] )
                {
					return parsedCall; // ERROR
                }

				StringPosition labelPosition = parsedCall.function.instructions.Find
				( 
					parsedCall.function.paramLabels[i] 
				);

				if ( labelPosition.Empty() )
				{
					return parsedCall; // ERROR
				}

				if ( parsedCall.paramDataTypes[i] == DataTypeEnum.typeString )
				{
					parsedCall.function.instructions = parsedCall.function.instructions.Remove( labelPosition.startOffset, labelPosition.Length() );
					parsedCall.function.instructions = parsedCall.function.instructions.Insert
					( 
						labelPosition.startOffset, 
						( ( string )parsedCall.parameters[i] )
					);
				}
				else
                {
					parsedCall.function.instructions = parsedCall.function.instructions.Remove( labelPosition.startOffset, labelPosition.Length() );
					parsedCall.function.instructions = parsedCall.function.instructions.Insert
					( 
						labelPosition.startOffset, 
						( ( long )parsedCall.parameters[i]).ToString() 
					);
                }
            }

			return parsedCall;
        }


		private static ParsedVariable CompileVariable( ref ParsedVariable parsedVariable )
        {
			parsedVariable.instructions = "entry push " + 
			( 
				parsedVariable.dataType == DataTypeEnum.typeString ?
				( ( string )parsedVariable.value ).ToPointer().ToString() : 
				( long )parsedVariable.value 
			) + " ret";

			return parsedVariable;
        }


		private static ParsedIf CompileIf( ref ParsedIf parsedIf )
        {
			// fill start with body, jmp 0 if condition true

			string[] compiledBody = Compile( parsedIf.body );

			parsedIf.instructions = "entry mov gra " +
			(
				parsedIf.operands.Item1.dataType == DataTypeEnum.typeString ?
				( ( string )parsedIf.operands.Item1.value ).ToPointer().ToString() :
				( long )parsedIf.operands.Item1.value
			) + ' ';

			switch ( parsedIf._operator )
            {
                case "==":
					parsedIf.instructions = parsedIf.instructions.Insert( parsedIf.instructions.Length - 1, "cmp gra " + 
					(
						parsedIf.operands.Item2.dataType == DataTypeEnum.typeString ?
						( ( string )parsedIf.operands.Item2.value ) :
						( long )parsedIf.operands.Item2.value
					) ) + " tjmp 0 ret";

					break;

				case "!=":
					parsedIf.instructions = parsedIf.instructions.Insert( parsedIf.instructions.Length - 1, "cmp gra " + 
					(
						parsedIf.operands.Item2.dataType == DataTypeEnum.typeString ?
						( ( string )parsedIf.operands.Item2.value ) :
						( long )parsedIf.operands.Item2.value
					) ) + " fjmp 0 ret";

					break;

				case ">=":

					dynamic operandTwo =
					(
						parsedIf.operands.Item2.dataType == DataTypeEnum.typeString ?
						( ( string )parsedIf.operands.Item2.value ) :
						( long )parsedIf.operands.Item2.value
					);

					parsedIf.instructions = parsedIf.instructions.Insert
					( 
						parsedIf.instructions.Length - 1, "cmp gra " +
						operandTwo + " tjmp 0 greater gra " + operandTwo + " tjmp 0 ret"
					);

					break;

				case "<=":
					operandTwo =
					(
						parsedIf.operands.Item2.dataType == DataTypeEnum.typeString ?
						( (string )parsedIf.operands.Item2.value ) :
						( long )parsedIf.operands.Item2.value
					);

					parsedIf.instructions = parsedIf.instructions.Insert
					(
						parsedIf.instructions.Length - 1, "cmp gra " +
						operandTwo + " tjmp 0 less gra " + operandTwo + " tjmp 0 ret"
					);

					break;

				case ">":

					operandTwo =
					(
						parsedIf.operands.Item2.dataType == DataTypeEnum.typeString ?
						( ( string )parsedIf.operands.Item2.value ) :
						( long )parsedIf.operands.Item2.value
					);

					parsedIf.instructions = parsedIf.instructions.Insert
					(
						parsedIf.instructions.Length - 1, "greater gra " +
						operandTwo + " tjmp 0 ret"
					);

					break;

				case "<":

					operandTwo =
					(
						parsedIf.operands.Item2.dataType == DataTypeEnum.typeString ?
						((string)parsedIf.operands.Item2.value) :
						(long)parsedIf.operands.Item2.value
					);

					parsedIf.instructions = parsedIf.instructions.Insert
					(
						parsedIf.instructions.Length - 1, "less gra " +
						operandTwo + " tjmp 0 ret"
					);

					break;
			}

			return parsedIf;
        }


		public static string[] Compile( string code = null )
		{
			List<string> compiledInstructions = new();
			string path = "";

			if ( code == null )
			{
				path = System.IO.Path.GetDirectoryName
				(
					System.Reflection.Assembly.GetExecutingAssembly().Location
				) + "\\main.vmc";

				code = System.IO.File.ReadAllText( path );
			}

			while ( true )
			{
				StringPosition commentPosition = code.Find( "//*\r" );

				if ( commentPosition.Empty() == true )
				{
					break;
				}

				code = code.Remove( commentPosition.startOffset, commentPosition.Length() );
			}

			List<ParsedInstruction> rawInstructions = ParseCode( code );

			foreach ( ParsedInstruction instruction in rawInstructions )
			{

				switch ( instruction.instructionType )
                {
					case InstructionTypeEnum.typeCall:
						ParsedCall parsedCall = ( ParsedCall )instruction;
						CompileFunctionCall( ref parsedCall );
						compiledInstructions.Add( parsedCall.function.instructions );
						break;

					case InstructionTypeEnum.typeVariable:
						ParsedVariable parsedVariable = ( ParsedVariable )instruction;
						CompileVariable( ref parsedVariable );
						compiledInstructions.Add( parsedVariable.instructions );
						break;

					case InstructionTypeEnum.typeFunction:
						break;

					case InstructionTypeEnum.typeIf:
						ParsedIf parsedIf = ( ParsedIf )instruction;
						CompileIf( ref parsedIf );
						compiledInstructions.Add( parsedIf.instructions );
						break;
                }
			}

			return compiledInstructions.ToArray();
		}


		public Compiler( ref VirtualMachine.Registers registers )
        {
			variableTable.Clear();
			functionTable.Clear();

			ParsedFunction print = new();
			ParsedFunction add = new();

			print.paramDataTypes.Add( DataTypeEnum.typeString );
			print.returnType = DataTypeEnum.typeBool;
			print.paramLabels.Add( "text" );
			print.instructions = "entry strout text ret";
			print.label = "print";

			add.paramDataTypes.Add( DataTypeEnum.typeQword );
			add.paramDataTypes.Add( DataTypeEnum.typeQword );
			add.returnType = DataTypeEnum.typeQword;
			add.paramLabels.Add( "var" );
			add.paramLabels.Add( "_var" );
			add.instructions = "entry mov gra var add gra _var mov rr acc ret";
			add.label = "add";

			functionTable.Add( "print", print );
			functionTable.Add( "add", add );

			string[] instructions = Compile();

			foreach ( string s in instructions )
            {
				registers = Assembler.Assemble( s );
            }
		}
    }
