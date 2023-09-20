public class Compiler
	{

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


		private struct RawFunction
        {
			public string label;
			public string parameters;
			public string returnTypeString;
			public string body;
			public DataTypeEnum returnType;


			public DataTypeEnum stringToEnum( string dataTypeString )
            {
				if ( dataTypeString.CompareTo( "byte" ) == 0 )
                {
					return DataTypeEnum.typeByte;
                }

				if ( dataTypeString.CompareTo( "word" ) == 0 )
                {
					return DataTypeEnum.typeWord;
                }

				if ( dataTypeString.CompareTo( "dword" ) == 0 )
				{
					return DataTypeEnum.typeDword;
				}

				if ( dataTypeString.CompareTo( "qword" ) == 0 )
				{
					return DataTypeEnum.typeQword;
				}

				if ( dataTypeString.CompareTo( "string" ) == 0 )
				{
					return DataTypeEnum.typeString;
				}

				if ( dataTypeString.CompareTo( "bool" ) == 0 )
				{
					return DataTypeEnum.typeBool;
				}

				if ( dataTypeString.CompareTo( "determine" ) == 0 )
				{
					return DataTypeEnum.typeDetermine;
				}

				return DataTypeEnum.typeInvalid;
			}
        };


		public struct ParsedFunction
        {
			public List<object[]> parameters;
			public DataTypeEnum returnType;
			public string[] instructions;
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
			"for",
			"if"
		};


		private string validFunction = "function*(*)returns*{*}";
		private string validVariable = "*=*;";
		private string validLoop = "for(*;*;*){*}";
		private string validIf = "if(*){*}";
		private Dictionary<string, ParsedFunction> functionTable = new();


		private static string getFunctionLabel( string function, int startIndex = 0 )
        {
			return function.Between( "function", '(', startIndex );
        }


		private static string getFunctionParameters( string function, int startIndex = 0 )
		{
			return function.Between( getFunctionLabel( function ) + '(', ')', startIndex );
		}


		private static string getFunctionReturnType( string function, int startIndex = 0 )
		{
			return function.Between( "returns", '{', startIndex );
		}


		private static string getFunctionBody( string function, int startIndex = 0 )
		{
			return function.Between( '{', '}', startIndex );
		}


		private static RawFunction ModelRawFunction( string function )
        {
            RawFunction rawFunction = new()
            {
                label = getFunctionLabel( function ),
                parameters = getFunctionParameters( function ),
                returnTypeString = getFunctionReturnType( function ),
                body = getFunctionBody( function )
            };

            rawFunction.returnType = rawFunction.stringToEnum( rawFunction.returnTypeString );
			return rawFunction;
        }


		public static ParsedFunction ParseFunction( string function )
        {
			List<InstructionTypeEnum> instructionTypeList = new();

			RawFunction rawFunction = ModelRawFunction( function );

			ParsedFunction parsedFunction = new()
			{
				returnType = rawFunction.returnType
			};
			
			foreach ( string keyword in keywords )
            {
				if ( rawFunction.body.Find( keyword ) == 0 )
                {
					keywords.Position( keyword )
                }
            }
        }
	}
